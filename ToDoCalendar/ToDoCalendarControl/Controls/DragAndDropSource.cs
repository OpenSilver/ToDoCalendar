using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using ToDoCalendarControl.Helpers;

namespace ToDoCalendarControl.Controls
{
    public class DragAndDropSource : ContentControl
    {
        //-------------------------------
        // IMPORTANT:
        // Keep this class very small because it is designed
        // as an example of implementation so that other classes
        // can use a similar system to initiate a drag and drop
        // operation.
        //-------------------------------

        private DragAndDropRoot _dragAndDropRoot;
        private Point _dragDeltaOrigin;
        private Point _cursorPositionRelativeToSource;
        private bool _dragAndDropStarted;
        private FrameworkElement _layoutRoot;

        public object GroupId { get; set; }
        public double DistanceForDragOperationToBeConsideredIntentional { get; set; } // in pixels.
        public bool DontCollapseDuringDrag { get; set; }
        public bool HoldToStartDrag { get; set; }
        public bool EnlargeSourceDuringDrag { get; set; }

        public event EventHandler Click;
        public event EventHandler DragAndDropStarted;
        public event EventHandler DragAndDropStopped;

        private const int MaxDragDelta = 5;
        private readonly DispatcherTimer _holdTimer = new() { Interval = TimeSpan.FromMilliseconds(200) };
        private object _mouseLeftButtonDownSender;
        private MouseEventArgs _mouseLeftButtonDownEventArgs;
        private Point _originPosition;

        public DragAndDropSource()
        {
            Template = (ControlTemplate)XamlReader.Load(@"
                <ControlTemplate TargetType=""ContentControl""
                    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                        <Border x:Name=""PART_LayoutRoot"" Background=""Transparent"">
                            <ContentPresenter Content=""{TemplateBinding Content}""/>
                        </Border>
                </ControlTemplate>");

            // Set default values for properties:
            GroupId = "";
            DistanceForDragOperationToBeConsideredIntentional = 10; // in pixels.
            DontCollapseDuringDrag = false;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            // Unregister previous events if any:
            if (_layoutRoot != null)
            {
                _layoutRoot.RemoveHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(LayoutRoot_MouseLeftButtonDown));
                _layoutRoot.RemoveHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(LayoutRoot_MouseLeftButtonUp));
                _layoutRoot.MouseMove -= LayoutRoot_MouseMove;
            }

            // Attempt to get a reference to the objects in the template:
            _layoutRoot = GetTemplateChild("PART_LayoutRoot") as FrameworkElement;

            // Register events:
            if (_layoutRoot != null)
            {
                _layoutRoot.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(LayoutRoot_MouseLeftButtonDown), true);
                _layoutRoot.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(LayoutRoot_MouseLeftButtonUp), true);
                _layoutRoot.MouseMove += LayoutRoot_MouseMove;
            }
        }

        private void LayoutRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _originPosition = e.GetPosition(Application.Current.RootVisual);

            if (!HoldToStartDrag)
            {
                // Verify that the user is not dragging a scrollbar:
                if (e.OriginalSource is DependencyObject && UIHelper.GetParentOfType<ScrollBar>((DependencyObject)e.OriginalSource) == null)
                {
                    // Initialize the drag and drop operation:
                    StartDragOperation(sender,
                        _originPosition,
                        e.GetPosition((FrameworkElement)sender));
                }
            }
            else
            {
                _mouseLeftButtonDownSender = sender;
                _mouseLeftButtonDownEventArgs = e;
                _holdTimer.Tick += OnHoldTimerTick;
                _holdTimer.Start();
            }
        }

        private void OnHoldTimerTick(object sender, EventArgs e)
        {
            ResetHoldTimer();

            StartDragOperation(_mouseLeftButtonDownSender,
                    _mouseLeftButtonDownEventArgs.GetPosition(Application.Current.RootVisual),
                    _mouseLeftButtonDownEventArgs.GetPosition((FrameworkElement)_mouseLeftButtonDownSender));
            OnMouseMove(new Point(_dragDeltaOrigin.X, _dragDeltaOrigin.Y), distanceForDragOperationToBeConsideredIntentional: 0);

            _mouseLeftButtonDownSender = null;
            _mouseLeftButtonDownEventArgs = null;
        }

        private void ResetHoldTimer()
        {
            _holdTimer.Tick -= OnHoldTimerTick;
            _holdTimer.Stop();
        }

        private void StartDragOperation(object sender, Point dragDeltaOrigin, Point cursorPositionRelativeToSource)
        {
            // Initialize the drag and drop operation:
            _dragDeltaOrigin = dragDeltaOrigin;
            _cursorPositionRelativeToSource = cursorPositionRelativeToSource;
            _dragAndDropRoot = UIHelper.GetParentOfType<DragAndDropRoot>(this);
            ((UIElement)sender).CaptureMouse();
            _dragAndDropStarted = false; // Note: we start drag and drop later (in the MouseMove event) so as to not interfere in case the user is just clicking somewhere in the content, with no drag-and-drop intention.
        }

        private void LayoutRoot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).ReleaseMouseCapture();
            _dragAndDropRoot?.StopDragAndDrop();
            _dragAndDropRoot = null;

            if (!_dragAndDropStarted)
            {
                // The "Click" event is raised when the user does a simple click or very small unintentional drag (rather than a full drag-and-drop operation).
                Click?.Invoke(this, new EventArgs());
            }
            else
            {
                DragAndDropStopped?.Invoke(this, new EventArgs());
            }

            if (_holdTimer.IsEnabled)
            {
                ResetHoldTimer();
            }
        }

        private void LayoutRoot_MouseMove(object sender, MouseEventArgs e)
        {
            var pointerPosition = e.GetPosition(Application.Current.RootVisual);

            if (_holdTimer.IsEnabled && !IsSameSpot(_originPosition, pointerPosition))
            {
                ResetHoldTimer();
            }

            OnMouseMove(pointerPosition, DistanceForDragOperationToBeConsideredIntentional);
        }

        private void OnMouseMove(Point pointerPosition, double distanceForDragOperationToBeConsideredIntentional)
        {
            var cumulativeDragDelta = new Point(pointerPosition.X - _dragDeltaOrigin.X, pointerPosition.Y - _dragDeltaOrigin.Y);
            if (_dragAndDropRoot != null)
            {
                if (!_dragAndDropStarted && Math.Sqrt(Math.Pow(cumulativeDragDelta.X, 2) + Math.Pow(cumulativeDragDelta.Y, 2)) >= distanceForDragOperationToBeConsideredIntentional)
                {
                    // Cancel if another drag and drop operation is taking place:
                    if (_dragAndDropRoot.IsDragAndDropOperationTakingPlace)
                        _dragAndDropRoot = null;
                    else
                    {
                        // Start drag and drop:
                        _dragAndDropRoot.StartDragAndDrop(
                            this,
                            groupId: GroupId,
                            cursorPositionRelativeToSource: _cursorPositionRelativeToSource,
                            sourceShouldNotBeCollapsedDuringDrag: DontCollapseDuringDrag,
                            sourceShouldBeEnlargedDuringDrag: EnlargeSourceDuringDrag);

                        _dragAndDropStarted = true;
                        DragAndDropStarted?.Invoke(this, new EventArgs());
                    }
                }
                else
                {
                    // Update drag and drop:
                    _dragAndDropRoot.UpdateDragAndDrop(cumulativeDragDelta);
                }
            }
        }

        public static bool IsSameSpot(Point originPosition, Point newPosition) =>
            Math.Abs(newPosition.X - originPosition.X) < MaxDragDelta &&
            Math.Abs(newPosition.Y - originPosition.Y) < MaxDragDelta;
    }
}
