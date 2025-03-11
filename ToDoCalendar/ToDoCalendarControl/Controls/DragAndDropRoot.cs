using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ToDoCalendarControl.Helpers;
using static ToDoCalendarControl.RenderingHelpers;

namespace ToDoCalendarControl.Controls
{
    public class DragAndDropRoot : ContentControl
    {
        //--------------------------------------------------------------------
        // How to use this Drag and Drop system:
        // 1) Define the places where the elements can be dropped. These are called "Drag and Drop Targets". To do so, just add some instances of the "DragAndDropTarget" controls to the visual tree. Optionally, set their "GroupName" if you want to ensure that only some elements can be dropped on some targets.
        // 2) Add an instance of the "DragAndDropRoot" control to the visual tree. It must be placed either near the root of the XAML or at least at a higher level than all the drop targets.
        // 3) Place an instance of the "DragAndDropSource" around the elements that you want to be draggable. Alternatively, you can copy the code of "DragAndDropSource" into your own control to adapt it to your needs (for example if you wish to use a method other than UIElement.CaptureMouse()/MouseMove or Thumb_DragDelta).
        //--------------------------------------------------------------------

        private const string DefaultGroupId = ""; // Note: we use this because "null" cannot be used as the key of a dictionary.
        private const double LowerBoundForMinimumDistanceToConsiderDropTarget = 10d; // in pixels
        private const double GhostOpacityDuringDragAndDrop = 0.7; // This is the opacity that the element being dragged must have during the drag-and-drop operation.
        public const double DefaultDurationOfAnimations = 0.5d; // in seconds

        private InformationAboutElementBeingDragged _informationAboutElementBeingDragged;
        private AutoScrollIfAtEdgeOfScrollViewer _autoScrollIfAtEdgeOfScrollViewer = new(); // This is used to scroll automatically if the user is dragging the object near the edge of a scrollviewer.

        // The following dictionary is used to keep track of the "targets" so that when the drag & drop operation is initiated, the system knows where the targets are. Note: the targets are grouped by their "GroupId". The "Key" of the dictionary is the GroupId:
        private Dictionary<object, HashSet<DragAndDropTarget>> _dragAndDropTargets = [];

        public bool IsDragAndDropOperationTakingPlace => _informationAboutElementBeingDragged != null;

        private double _thresholdForScrollViewerAutomaticScroll = 30; // in pixels
        public double ThresholdForScrollViewerAutomaticScroll
        {
            get => _thresholdForScrollViewerAutomaticScroll;
            set => _thresholdForScrollViewerAutomaticScroll = value;
        }

        public void RegisterDragAndDropTarget(DragAndDropTarget dragAndDropTarget)
        {
            // Get the list of targets where to add the new target (note: they are grouped by "GroupId"):
            var groupId = dragAndDropTarget.GroupId ?? DefaultGroupId;
            HashSet<DragAndDropTarget> listOfTargets;
            if (_dragAndDropTargets.ContainsKey(groupId))
                listOfTargets = _dragAndDropTargets[groupId];
            else
            {
                listOfTargets = [];
                _dragAndDropTargets.Add(groupId, listOfTargets);
            }

            // Register the target:
            if (!listOfTargets.Contains(dragAndDropTarget))
                listOfTargets.Add(dragAndDropTarget);
        }

        public void UnregisterDragAndDropTarget(DragAndDropTarget dragAndDropTarget)
        {
            // Unregister the target (if found):
            var groupId = dragAndDropTarget.GroupId ?? DefaultGroupId;
            if (_dragAndDropTargets.ContainsKey(groupId)
                && _dragAndDropTargets[groupId].Contains(dragAndDropTarget))
                _dragAndDropTargets[groupId].Remove(dragAndDropTarget);
        }

        public void StartDragAndDrop(FrameworkElement source, object groupId = null, Point? cursorPositionRelativeToSource = null, bool sourceShouldNotBeCollapsedDuringDrag = false, bool sourceShouldBeEnlargedDuringDrag = false)
        {
            // Make sure the previous drag and drop operation is stopped:
            if (_informationAboutElementBeingDragged != null)
                StopDragAndDrop();

            // Start the auto-scroller (to scroll automatically if near the edge of a scrollviewer):
            _autoScrollIfAtEdgeOfScrollViewer.ThresholdForScrollViewerAutomaticScroll = _thresholdForScrollViewerAutomaticScroll;
            _autoScrollIfAtEdgeOfScrollViewer.StartDetectionBySettingAbsolutePositionOnARegularBasis();

            // Highlight all targets:
            foreach (var target in GetTargetsWithGiveGroupId(groupId ?? DefaultGroupId))
                target.IsHighlightedForSignalingAvailableTargets = true;

            // Show ghost of element being dragged:
            if (TryGetElementRect(source, Application.Current.RootVisual, out Rect initialAbsoluteCoordinates))
            {
                // Create a ghost for the dragged control:
                var popup = new Popup();
                var ghost = CreateGhostFromSource(source, initialAbsoluteCoordinates.Width, initialAbsoluteCoordinates.Height, sourceShouldBeEnlargedDuringDrag);
                popup.Child = ghost;
                popup.IsOpen = true;

                // Hide the source control:
                var sourceOpacityBeforeDrag = source.Opacity;
                source.Opacity = 0d;

                // Remember information about the source control:
                _informationAboutElementBeingDragged = new InformationAboutElementBeingDragged(
                    groupId ?? DefaultGroupId,
                    initialAbsoluteCoordinates,
                    popup,
                    source,
                    sourceOpacityBeforeDrag,
                    source.Width,
                    source.Height,
                    cursorPositionRelativeToSource,
                    sourceShouldNotBeCollapsedDuringDrag,
                    sourceShouldBeEnlargedDuringDrag);

            }
            else
                _informationAboutElementBeingDragged = null;
            UpdateDragAndDrop(new Point(0, 0));
        }

        public void StopDragAndDrop()
        {
            if (_informationAboutElementBeingDragged != null)
            {
                // Remove highlights of targets:
                foreach (var target in GetTargetsWithGiveGroupId(_informationAboutElementBeingDragged.GroupId))
                {
                    target.IsHighlightedForSignalingAvailableTargets = false;
                    target.HideHighlightingForSignalingSelectedTarget(instantly: true);
                }

                // Remove the ghost of the element being dragged:
                _informationAboutElementBeingDragged.GhostWhileDragging.IsOpen = false;

                // Revert to the original opacity of the source element:
                _informationAboutElementBeingDragged.Source.Opacity = _informationAboutElementBeingDragged.SourceOpacityBeforeDrag;

                // Restore the original size of the source element:
                if (_informationAboutElementBeingDragged.SourceHasBeenCollapsed
                    && !_informationAboutElementBeingDragged.SourceShouldNotBeCollapsedDuringDrag)
                {
                    UncollapseTheSource(instantly: true);
                }

                // Stop the auto-scroller:
                _autoScrollIfAtEdgeOfScrollViewer.StopDetection();

                // Raise the "SourceDropped" event:
                _informationAboutElementBeingDragged.TargetOnWhichTheSourceIsOver?.OnSourceDropped(new DragAndDropEventArgs(_informationAboutElementBeingDragged.Source));

                // Reset:
                _informationAboutElementBeingDragged = null;
            }
        }

        public void UpdateDragAndDrop(Point cumulativeDragDelta)
        {
            if (_informationAboutElementBeingDragged != null)
            {
                // Calculate current absolute position:
                Point absolutePositionOfTopLeftCornerOfSource = new(_informationAboutElementBeingDragged.InitialAbsoluteCoordinates.X + cumulativeDragDelta.X, _informationAboutElementBeingDragged.InitialAbsoluteCoordinates.Y + cumulativeDragDelta.Y);
                var absolutePositionOfCursorPositionOrOfCenterOfControl =
                    _informationAboutElementBeingDragged.CursorPositionRelativeToSource.HasValue
                    ? new Point(absolutePositionOfTopLeftCornerOfSource.X + _informationAboutElementBeingDragged.CursorPositionRelativeToSource.Value.X, absolutePositionOfTopLeftCornerOfSource.Y + _informationAboutElementBeingDragged.CursorPositionRelativeToSource.Value.Y)
                    : new Point(absolutePositionOfTopLeftCornerOfSource.X + _informationAboutElementBeingDragged.InitialAbsoluteCoordinates.Width / 2, absolutePositionOfTopLeftCornerOfSource.Y + _informationAboutElementBeingDragged.InitialAbsoluteCoordinates.Height / 2);

                // Update the auto-scroller (to scroll automatically if near the edge of a scrollviewer):
                _autoScrollIfAtEdgeOfScrollViewer.SetAbsolutePosition(absolutePositionOfCursorPositionOrOfCenterOfControl);

                // Move the ghost control:
                var popup = _informationAboutElementBeingDragged.GhostWhileDragging;
                // adjust the offset for a new ghost event
                if (_informationAboutElementBeingDragged.Source is DragAndDropSource source && !source.HoldToStartDrag)
                {
                    var ghost = _informationAboutElementBeingDragged.GhostWhileDragging.Child as FrameworkElement;
                    popup.HorizontalOffset = absolutePositionOfCursorPositionOrOfCenterOfControl.X - ghost.ActualWidth;
                    popup.VerticalOffset = absolutePositionOfCursorPositionOrOfCenterOfControl.Y - ghost.ActualHeight;
                }
                else
                {
                    popup.HorizontalOffset = absolutePositionOfTopLeftCornerOfSource.X;
                    popup.VerticalOffset = absolutePositionOfTopLeftCornerOfSource.Y;
                }

                // Find closest target:
                DragAndDropTarget closestTarget = null;
                double closesTargetDistance = double.MaxValue;
                var rootControl = Application.Current.RootVisual;
                foreach (var target in GetTargetsWithGiveGroupId(_informationAboutElementBeingDragged.GroupId))
                {
                    if (TryGetElementRect(target, rootControl, out Rect targetRect))
                    {
                        var distanceToTargetRect = CalculateDistanceToRect(absolutePositionOfCursorPositionOrOfCenterOfControl, targetRect);
                        if (distanceToTargetRect < closesTargetDistance)
                        {
                            closesTargetDistance = distanceToTargetRect;
                            closestTarget = target;
                        }
                    }
                }

                // If the closest target is within the minimum allowed distance, highlight it. Also remove the highlighting of all the other targets:
                var minimumDistanceToConsiderDropTarget = CalculateMinimumDistanceToConsiderDropTarget(_informationAboutElementBeingDragged.InitialAbsoluteCoordinates.Width, _informationAboutElementBeingDragged.InitialAbsoluteCoordinates.Height);
                foreach (var target in GetTargetsWithGiveGroupId(_informationAboutElementBeingDragged.GroupId))
                {

                    if (target == closestTarget && closesTargetDistance < minimumDistanceToConsiderDropTarget)
                    {
                        target.ShowHighlightingForSignalingSelectedTarget(_informationAboutElementBeingDragged.InitialAbsoluteCoordinates.Width, _informationAboutElementBeingDragged.InitialAbsoluteCoordinates.Height);
                        _informationAboutElementBeingDragged.TargetOnWhichTheSourceIsOver = target;
                    }
                    else
                        target.HideHighlightingForSignalingSelectedTarget();
                }

                // If we highlighted a target, collapse the source element:
                if (_informationAboutElementBeingDragged.TargetOnWhichTheSourceIsOver != null)
                {
                    CollapseTheSource();
                }
                else
                {
                    UncollapseTheSource();
                }
            }
        }

        private HashSet<DragAndDropTarget> GetTargetsWithGiveGroupId(object groupId)
        {
            if (_dragAndDropTargets.ContainsKey(groupId))
                return _dragAndDropTargets[groupId];
            else
                return [];
        }

        private void CollapseTheSource()
        {
            var source = _informationAboutElementBeingDragged.Source;

            if (!_informationAboutElementBeingDragged.SourceShouldNotBeCollapsedDuringDrag)
            {
                ApplyActualSizeToWidthAndHeight(_informationAboutElementBeingDragged.Source);

                AnimationHelper.SetPropertyWithAnimation(source, "(FrameworkElement.Width)", 0, DefaultDurationOfAnimations);
                AnimationHelper.SetPropertyWithAnimation(source, "(FrameworkElement.Height)", 0, DefaultDurationOfAnimations);

                _informationAboutElementBeingDragged.SourceHasBeenCollapsed = true;
            }
        }

        private void UncollapseTheSource(bool instantly = false)
        {
            var source = _informationAboutElementBeingDragged.Source;
            var sourceWidthBeforeDrag = _informationAboutElementBeingDragged.SourceWidthBeforeDrag;
            var sourceHeightBeforeDrag = _informationAboutElementBeingDragged.SourceHeightBeforeDrag;

            if (_informationAboutElementBeingDragged.SourceHasBeenCollapsed
                && !_informationAboutElementBeingDragged.SourceShouldNotBeCollapsedDuringDrag)
            {
                ApplyActualSizeToWidthAndHeight(_informationAboutElementBeingDragged.Source);

                AnimationHelper.SetPropertyWithAnimation(_informationAboutElementBeingDragged.Source, "(FrameworkElement.Width)", _informationAboutElementBeingDragged.InitialAbsoluteCoordinates.Width, instantly ? 0 : DefaultDurationOfAnimations, actionWhenCompleted: () =>
                {
                    source.Width = sourceWidthBeforeDrag;
                });

                AnimationHelper.SetPropertyWithAnimation(_informationAboutElementBeingDragged.Source, "(FrameworkElement.Height)", _informationAboutElementBeingDragged.InitialAbsoluteCoordinates.Height, instantly ? 0 : DefaultDurationOfAnimations, actionWhenCompleted: () =>
                {
                    source.Height = sourceHeightBeforeDrag;
                });

                // Note: when the "instantly" value is True, we could have set the properties directly without using an animation. However, using an animation (with a duration of 0 seconds) has the advantage that it stops all the other animations that are pending on the same property.

                _informationAboutElementBeingDragged.SourceHasBeenCollapsed = false;
            }
        }

        private static void ApplyActualSizeToWidthAndHeight(FrameworkElement element)
        {
            if (double.IsNaN(element.Width) && !double.IsNaN(element.ActualWidth))
                element.Width = element.ActualWidth;
            if (double.IsNaN(element.Height) && !double.IsNaN(element.ActualHeight))
                element.Height = element.ActualHeight;
        }

        private static bool TryGetElementRect(FrameworkElement element, UIElement rootControl, out Rect rect)
        {
            try
            {
                rect = element.TransformToVisual(rootControl)
                              .TransformBounds(new Rect(element.RenderSize));
                return true;
            }
            catch { }

            rect = new Rect();
            return false;
        }

        private static double CalculateDistanceToRect(Point point, Rect rect)
        {
            if (rect.Contains(point))
                // If the point is inside the Rect, the distance is 0:
                return 0d;
            else
            {
                // Otherwise we calculate the minimum distance between the point and each segment of the rect:
                var distanceToLeft = CalculateDistanceToSegment(point, new Point(rect.Left, rect.Top), new Point(rect.Left, rect.Bottom));
                var distanceToTop = CalculateDistanceToSegment(point, new Point(rect.Left, rect.Top), new Point(rect.Right, rect.Top));
                var distanceToRight = CalculateDistanceToSegment(point, new Point(rect.Right, rect.Top), new Point(rect.Right, rect.Bottom));
                var distanceToBottom = CalculateDistanceToSegment(point, new Point(rect.Left, rect.Bottom), new Point(rect.Right, rect.Bottom));
                return Math.Min(distanceToLeft, Math.Min(distanceToTop, Math.Min(distanceToRight, distanceToBottom)));
            }
        }

        // Calculate the distance between point pt and the segment p1 --> p2.
        // Credits: http://blog.csharphelper.com/2010/03/26/find-the-shortest-distance-between-a-point-and-a-line-segment-in-c.aspx
        private static double CalculateDistanceToSegment(Point point, Point segmentStart, Point segmentEnd)
        {
            Point closestPoint;
            double dx = segmentEnd.X - segmentStart.X;
            double dy = segmentEnd.Y - segmentStart.Y;
            if (dx == 0 && dy == 0)
            {
                // It's a point not a line segment.
                closestPoint = segmentStart;
                dx = point.X - segmentStart.X;
                dy = point.Y - segmentStart.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            // Calculate the t that minimizes the distance.
            double t = ((point.X - segmentStart.X) * dx + (point.Y - segmentStart.Y) * dy) / (dx * dx + dy * dy);

            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closestPoint = new Point(segmentStart.X, segmentStart.Y);
                dx = point.X - segmentStart.X;
                dy = point.Y - segmentStart.Y;
            }
            else if (t > 1)
            {
                closestPoint = new Point(segmentEnd.X, segmentEnd.Y);
                dx = point.X - segmentEnd.X;
                dy = point.Y - segmentEnd.Y;
            }
            else
            {
                closestPoint = new Point(segmentStart.X + t * dx, segmentStart.Y + t * dy);
                dx = point.X - closestPoint.X;
                dy = point.Y - closestPoint.Y;
            }

            return Math.Sqrt(dx * dx + dy * dy);
        }

        private UIElement CreateGhostFromSource(FrameworkElement source, double sourceWidth, double sourceHeight, bool sourceShouldBeEnlargedDuringDrag)
        {
            UIElement ghostControl;
            const bool useRectangle = false;
            if (useRectangle) // note: we keep this section for compatibility with systems where writableBitmap.Render is not available (like Windows 8.0).
            {
                //var rect = new Rectangle()
                //{
                //    Fill = new SolidColorBrush(Colors.Purple),
                //    Width = sourceWidth,
                //    Height = sourceHeight,
                //    RenderTransform = null,
                //    Opacity = GhostOpacityDuringDragAndDrop
                //};
                //ghostControl = rect;
            }
            else
            {
                var image = new Border
                {
                    Child = new TextBlock
                    {
                        Text = (source.DataContext as InformationAboutEventBeingDragged)?.EventModel.Title,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Background = new SolidColorBrush(Color.FromRgb(210, 210, 210)),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(5),
                    MinWidth = 30,
                    MinHeight = 20,
                    Opacity = GhostOpacityDuringDragAndDrop,
                    //RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = sourceShouldBeEnlargedDuringDrag ? new ScaleTransform() { ScaleX = 1.5, ScaleY = 1.5 } : null
                };
                ghostControl = image;
            }

            ghostControl.MouseLeftButtonDown += OnGhostControlMouseLeftButtonDown;
            return ghostControl;
        }

        private void OnGhostControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // If the user clicks the ghost, it can only be that the ghost was stuck (ie. remained visible on screen) due to an issue with the "MouseButtonUp" not having been raised. This can happen for example in Silverlight if the user starts the drag operation, and right-clicks while dragging, and then presses Esc.
            StopDragAndDrop();
        }

        private static double CalculateMinimumDistanceToConsiderDropTarget(double sourceWidth, double sourceHeight)
        {
            var minimumDistanceToConsiderDropTarget = Math.Min(sourceWidth, sourceHeight) / 2;
            if (minimumDistanceToConsiderDropTarget < LowerBoundForMinimumDistanceToConsiderDropTarget)
                minimumDistanceToConsiderDropTarget = LowerBoundForMinimumDistanceToConsiderDropTarget;
            return minimumDistanceToConsiderDropTarget;
        }

        private class InformationAboutElementBeingDragged
        {
            public InformationAboutElementBeingDragged(
                object groupId,
                Rect initialAbsoluteCoordinates,
                Popup ghostWhileDragging,
                FrameworkElement source,
                double sourceOpacityBeforeDrag,
                double sourceWidthBeforeDrag,
                double sourceHeightBeforeDrag,
                Point? cursorPositionRelativeToSource,
                bool sourceShouldNotBeCollapsedDuringDrag,
                bool sourceShouldBeEnlargedDuringDrag)
            {
                GroupId = groupId;
                InitialAbsoluteCoordinates = initialAbsoluteCoordinates;
                GhostWhileDragging = ghostWhileDragging;
                Source = source;
                SourceOpacityBeforeDrag = sourceOpacityBeforeDrag;
                SourceWidthBeforeDrag = sourceWidthBeforeDrag;
                SourceHeightBeforeDrag = sourceHeightBeforeDrag;
                CursorPositionRelativeToSource = cursorPositionRelativeToSource;
                SourceShouldNotBeCollapsedDuringDrag = sourceShouldNotBeCollapsedDuringDrag;
                SourceShouldBeEnlargedDuringDrag = sourceShouldBeEnlargedDuringDrag;
            }

            // Read-only fields:
            public readonly object GroupId;
            public readonly Rect InitialAbsoluteCoordinates;
            public readonly Popup GhostWhileDragging;
            public readonly FrameworkElement Source;
            public readonly double SourceOpacityBeforeDrag;
            public readonly double SourceWidthBeforeDrag;
            public readonly double SourceHeightBeforeDrag;
            public readonly Point? CursorPositionRelativeToSource;
            public readonly bool SourceShouldNotBeCollapsedDuringDrag;
            public readonly bool SourceShouldBeEnlargedDuringDrag;

            // Properties that are going to change during the drag and drop operation:
            public DragAndDropTarget TargetOnWhichTheSourceIsOver { get; set; }
            public bool SourceHasBeenCollapsed { get; set; } // note: We collapse (ie. reduce the size) the source during the drag and drop animation when the user is dragging over a target.
        }
    }
}
