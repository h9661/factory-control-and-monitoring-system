using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SmartFactory.Presentation.Controls.FloorPlan;

/// <summary>
/// Custom Canvas control with zoom and pan capabilities for floor plan display.
/// </summary>
public class FloorPlanCanvas : Canvas
{
    #region Dependency Properties

    public static readonly DependencyProperty ZoomLevelProperty =
        DependencyProperty.Register(nameof(ZoomLevel), typeof(double), typeof(FloorPlanCanvas),
            new PropertyMetadata(1.0, OnZoomLevelChanged));

    public static readonly DependencyProperty MinZoomProperty =
        DependencyProperty.Register(nameof(MinZoom), typeof(double), typeof(FloorPlanCanvas),
            new PropertyMetadata(0.25));

    public static readonly DependencyProperty MaxZoomProperty =
        DependencyProperty.Register(nameof(MaxZoom), typeof(double), typeof(FloorPlanCanvas),
            new PropertyMetadata(4.0));

    public static readonly DependencyProperty PanOffsetXProperty =
        DependencyProperty.Register(nameof(PanOffsetX), typeof(double), typeof(FloorPlanCanvas),
            new PropertyMetadata(0.0, OnPanOffsetChanged));

    public static readonly DependencyProperty PanOffsetYProperty =
        DependencyProperty.Register(nameof(PanOffsetY), typeof(double), typeof(FloorPlanCanvas),
            new PropertyMetadata(0.0, OnPanOffsetChanged));

    public static readonly DependencyProperty GridVisibleProperty =
        DependencyProperty.Register(nameof(GridVisible), typeof(bool), typeof(FloorPlanCanvas),
            new PropertyMetadata(true, OnGridVisibleChanged));

    public static readonly DependencyProperty GridSizeProperty =
        DependencyProperty.Register(nameof(GridSize), typeof(double), typeof(FloorPlanCanvas),
            new PropertyMetadata(50.0, OnGridVisibleChanged));

    #endregion

    #region Properties

    public double ZoomLevel
    {
        get => (double)GetValue(ZoomLevelProperty);
        set => SetValue(ZoomLevelProperty, Math.Clamp(value, MinZoom, MaxZoom));
    }

    public double MinZoom
    {
        get => (double)GetValue(MinZoomProperty);
        set => SetValue(MinZoomProperty, value);
    }

    public double MaxZoom
    {
        get => (double)GetValue(MaxZoomProperty);
        set => SetValue(MaxZoomProperty, value);
    }

    public double PanOffsetX
    {
        get => (double)GetValue(PanOffsetXProperty);
        set => SetValue(PanOffsetXProperty, value);
    }

    public double PanOffsetY
    {
        get => (double)GetValue(PanOffsetYProperty);
        set => SetValue(PanOffsetYProperty, value);
    }

    public bool GridVisible
    {
        get => (bool)GetValue(GridVisibleProperty);
        set => SetValue(GridVisibleProperty, value);
    }

    public double GridSize
    {
        get => (double)GetValue(GridSizeProperty);
        set => SetValue(GridSizeProperty, value);
    }

    #endregion

    private Point _lastMousePosition;
    private bool _isPanning;
    private readonly ScaleTransform _scaleTransform;
    private readonly TranslateTransform _translateTransform;
    private readonly TransformGroup _transformGroup;

    public FloorPlanCanvas()
    {
        _scaleTransform = new ScaleTransform(1, 1);
        _translateTransform = new TranslateTransform(0, 0);
        _transformGroup = new TransformGroup();
        _transformGroup.Children.Add(_scaleTransform);
        _transformGroup.Children.Add(_translateTransform);
        RenderTransform = _transformGroup;

        Background = Brushes.Transparent;
        ClipToBounds = true;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        if (GridVisible)
        {
            DrawGrid(dc);
        }
    }

    private void DrawGrid(DrawingContext dc)
    {
        var gridPen = new Pen(new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)), 1);
        gridPen.Freeze();

        var width = ActualWidth / ZoomLevel;
        var height = ActualHeight / ZoomLevel;
        var gridSize = GridSize;

        // Draw vertical lines
        for (double x = 0; x < width; x += gridSize)
        {
            dc.DrawLine(gridPen, new Point(x, 0), new Point(x, height));
        }

        // Draw horizontal lines
        for (double y = 0; y < height; y += gridSize)
        {
            dc.DrawLine(gridPen, new Point(0, y), new Point(width, y));
        }
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        var mousePos = e.GetPosition(this);
        var zoomDelta = e.Delta > 0 ? 1.1 : 0.9;
        var newZoom = Math.Clamp(ZoomLevel * zoomDelta, MinZoom, MaxZoom);

        if (Math.Abs(newZoom - ZoomLevel) > 0.001)
        {
            // Zoom towards mouse position
            var oldZoom = ZoomLevel;
            ZoomLevel = newZoom;

            // Adjust pan to keep the point under the mouse stationary
            var zoomRatio = newZoom / oldZoom;
            PanOffsetX = mousePos.X - (mousePos.X - PanOffsetX) * zoomRatio;
            PanOffsetY = mousePos.Y - (mousePos.Y - PanOffsetY) * zoomRatio;
        }

        e.Handled = true;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        // Only start panning if clicking on the canvas background (not on equipment)
        if (e.OriginalSource == this)
        {
            _isPanning = true;
            _lastMousePosition = e.GetPosition(Parent as UIElement);
            CaptureMouse();
            Cursor = Cursors.Hand;
            e.Handled = true;
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (_isPanning)
        {
            _isPanning = false;
            ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
            e.Handled = true;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_isPanning)
        {
            var currentPosition = e.GetPosition(Parent as UIElement);
            var delta = currentPosition - _lastMousePosition;

            PanOffsetX += delta.X;
            PanOffsetY += delta.Y;

            _lastMousePosition = currentPosition;
            e.Handled = true;
        }
    }

    private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FloorPlanCanvas canvas)
        {
            canvas._scaleTransform.ScaleX = (double)e.NewValue;
            canvas._scaleTransform.ScaleY = (double)e.NewValue;
            canvas.InvalidateVisual();
        }
    }

    private static void OnPanOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FloorPlanCanvas canvas)
        {
            canvas._translateTransform.X = canvas.PanOffsetX;
            canvas._translateTransform.Y = canvas.PanOffsetY;
        }
    }

    private static void OnGridVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FloorPlanCanvas canvas)
        {
            canvas.InvalidateVisual();
        }
    }

    /// <summary>
    /// Resets the view to default zoom and position.
    /// </summary>
    public void ResetView()
    {
        ZoomLevel = 1.0;
        PanOffsetX = 0;
        PanOffsetY = 0;
    }

    /// <summary>
    /// Fits all content within the visible area.
    /// </summary>
    public void FitToContent()
    {
        if (Children.Count == 0) return;

        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;

        foreach (UIElement child in Children)
        {
            var left = GetLeft(child);
            var top = GetTop(child);

            if (double.IsNaN(left)) left = 0;
            if (double.IsNaN(top)) top = 0;

            var fe = child as FrameworkElement;
            var width = fe?.ActualWidth ?? 80;
            var height = fe?.ActualHeight ?? 80;

            minX = Math.Min(minX, left);
            minY = Math.Min(minY, top);
            maxX = Math.Max(maxX, left + width);
            maxY = Math.Max(maxY, top + height);
        }

        if (minX == double.MaxValue) return;

        var contentWidth = maxX - minX + 100; // Add padding
        var contentHeight = maxY - minY + 100;

        var scaleX = ActualWidth / contentWidth;
        var scaleY = ActualHeight / contentHeight;
        var scale = Math.Min(scaleX, scaleY);

        ZoomLevel = Math.Clamp(scale, MinZoom, MaxZoom);

        // Center the content
        PanOffsetX = (ActualWidth - contentWidth * ZoomLevel) / 2 - minX * ZoomLevel + 50;
        PanOffsetY = (ActualHeight - contentHeight * ZoomLevel) / 2 - minY * ZoomLevel + 50;
    }

    /// <summary>
    /// Zooms in by a fixed step.
    /// </summary>
    public void ZoomIn()
    {
        ZoomLevel = Math.Min(ZoomLevel * 1.25, MaxZoom);
    }

    /// <summary>
    /// Zooms out by a fixed step.
    /// </summary>
    public void ZoomOut()
    {
        ZoomLevel = Math.Max(ZoomLevel / 1.25, MinZoom);
    }
}
