// Uncomment the following directive to use the alternative implementation based on the use of a Thumb.
//#define ALTERNATIVE_IMPLEMENTATON_USING_A_THUMB

#if SILVERLIGHT
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
#elif WINRT
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Markup;
#endif

namespace MetroStyleApps
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


#if !ALTERNATIVE_IMPLEMENTATON_USING_A_THUMB

        DragAndDropRoot _dragAndDropRoot;
        Point _dragDeltaOrigin;
        Point _cursorPositionRelativeToSource;
        bool _dragAndDropStarted;
        FrameworkElement _layoutRoot;

        public object GroupId { get; set; }
        public double DistanceForDragOperationToBeConsideredIntentional { get; set; } // in pixels.
        public bool DontCollapseDuringDrag { get; set; }
        public bool HoldToStartDrag { get; set; }
        public bool EnlargeSourceDuringDrag { get; set; }

        public event EventHandler Click;
        public event EventHandler DragAndDropStarted;
        public event EventHandler DragAndDropStopped;

        public DragAndDropSource()
        {
            this.Template = (ControlTemplate)XamlReader.Load(@"
                <ControlTemplate TargetType=""ContentControl""
                    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                        <Border x:Name=""PART_LayoutRoot"" Background=""Transparent"">
                            <ContentPresenter/>
                        </Border>
                </ControlTemplate>");

            // Set default values for properties:
            GroupId = "";
            DistanceForDragOperationToBeConsideredIntentional = 10; // in pixels.
            DontCollapseDuringDrag = false;
        }

#if !WINRT

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            // Unregister previous events if any:
            if (_layoutRoot != null)
            {
                _layoutRoot.RemoveHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LayoutRoot_MouseLeftButtonDown));
                _layoutRoot.RemoveHandler(FrameworkElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(LayoutRoot_MouseLeftButtonUp));
                _layoutRoot.MouseMove -= LayoutRoot_MouseMove;
            }

            // Attempt to get a reference to the objects in the template:
            _layoutRoot = GetTemplateChild("PART_LayoutRoot") as FrameworkElement;

            // Register events:
            if (_layoutRoot != null)
            {
                _layoutRoot.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(LayoutRoot_MouseLeftButtonDown), true);
                _layoutRoot.AddHandler(FrameworkElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(LayoutRoot_MouseLeftButtonUp), true);
#if !OPENSILVER
                _layoutRoot.AddHandler(UIElement.HoldEvent, new EventHandler<GestureEventArgs>(LayoutRoot_Hold), true);
#endif
                _layoutRoot.MouseMove += LayoutRoot_MouseMove;
#if !OPENSILVER
                _layoutRoot.Hold += LayoutRoot_Hold;
#endif

            }
        }

#if !OPENSILVER
        void LayoutRoot_Hold(object sender, GestureEventArgs e)
        {
            if (HoldToStartDrag)
            {
                // Initialize the drag and drop operation:
                StartDragOperation(sender,
                    e.GetPosition(MetroHelpers.GetRootVisual()),
                    e.GetPosition((FrameworkElement)sender));
                OnMouseMove(new Point(_dragDeltaOrigin.X, _dragDeltaOrigin.Y), distanceForDragOperationToBeConsideredIntentional: 0);

                e.Handled = true;
            }
        }
#endif

        void LayoutRoot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!HoldToStartDrag)
            {
                // Verify that the user is not dragging a scrollbar:
                if (e.OriginalSource is DependencyObject && MetroHelpers.GetParentOfType<ScrollBar>((DependencyObject)e.OriginalSource) == null)
                {
                    // Initialize the drag and drop operation:
                    StartDragOperation(sender,
                        e.GetPosition(MetroHelpers.GetRootVisual()),
                        e.GetPosition((FrameworkElement)sender));
                }
            }
        }

        void StartDragOperation(object sender, Point dragDeltaOrigin, Point cursorPositionRelativeToSource)
        {
            // Initialize the drag and drop operation:
            _dragDeltaOrigin = dragDeltaOrigin;
            _cursorPositionRelativeToSource = cursorPositionRelativeToSource;
            _dragAndDropRoot = MetroHelpers.GetParentOfType<DragAndDropRoot>(this);
            ((UIElement)sender).CaptureMouse();
            _dragAndDropStarted = false; // Note: we start drag and drop later (in the MouseMove event) so as to not interfere in case the user is just clicking somewhere in the content, with no drag-and-drop intention.
        }

        void LayoutRoot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).ReleaseMouseCapture();
            if (_dragAndDropRoot != null)
                _dragAndDropRoot.StopDragAndDrop();
            _dragAndDropRoot = null;
            if (!_dragAndDropStarted)
            {
                // The "Click" event is raised when the user does a simple click or very small unintentional drag (rather than a full drag-and-drop operation).
                if (Click != null)
                    Click(this, new EventArgs());
            }
            else
            {
                if (DragAndDropStopped != null)
                    DragAndDropStopped(this, new EventArgs());
            }
        }

        void LayoutRoot_MouseMove(object sender, MouseEventArgs e)
        {
            var pointerPosition = e.GetPosition(MetroHelpers.GetRootVisual());
            OnMouseMove(pointerPosition, DistanceForDragOperationToBeConsideredIntentional);
        }

        void OnMouseMove(Point pointerPosition, double distanceForDragOperationToBeConsideredIntentional)
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
                        if (DragAndDropStarted != null)
                            DragAndDropStarted(this, new EventArgs());
                    }
                }
                else
                {
                    // Update drag and drop:
                    _dragAndDropRoot.UpdateDragAndDrop(cumulativeDragDelta);
                }
            }
        }
#endif

#else

        DragAndDropRoot _dragAndDropRoot;
        Point _cumulativeDragDelta;
        Thumb _mainThumb;

        public object GroupId { get; set; }

        public DragAndDropSource()
        {
            this.Template = (ControlTemplate)XamlReader.Load(@"
                <ControlTemplate TargetType=""ContentControl""
                    xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                        <Grid>
                            <ContentPresenter/>
                            <Thumb x:Name=""PART_MainThumb"" Opacity=""0""/>
                        </Grid>
                </ControlTemplate>");

            // Set default values for properties:
            GroupId = "";
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Unregister previous events if any:
            if (_mainThumb != null)
            {
                _mainThumb.DragStarted -= MainThumb_DragStarted;
                _mainThumb.DragDelta -= MainThumb_DragDelta;
                _mainThumb.DragCompleted -= MainThumb_DragCompleted;
            }

            // Attempt to get a reference to the objects in the template:
            _mainThumb = GetTemplateChild("PART_MainThumb") as Thumb;

            // Register events:
            if (_mainThumb != null)
            {
                _mainThumb.DragStarted += MainThumb_DragStarted;
                _mainThumb.DragDelta += MainThumb_DragDelta;
                _mainThumb.DragCompleted += MainThumb_DragCompleted;
            }
        }

        void MainThumb_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _cumulativeDragDelta = new Point(0, 0);
            _dragAndDropRoot = MetroHelpers.GetParentOfType<DragAndDropRoot>(this);
            if (_dragAndDropRoot != null)
                _dragAndDropRoot.StartDragAndDrop(this, groupId: GroupId);
        }
        
        void MainThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            _cumulativeDragDelta = new Point(_cumulativeDragDelta.X + e.HorizontalChange, _cumulativeDragDelta.Y + e.VerticalChange);
            if (_dragAndDropRoot != null)
                _dragAndDropRoot.UpdateDragAndDrop(_cumulativeDragDelta);
        }

        void MainThumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (_dragAndDropRoot != null)
                _dragAndDropRoot.StopDragAndDrop();
        }

#endif
            }
        }
