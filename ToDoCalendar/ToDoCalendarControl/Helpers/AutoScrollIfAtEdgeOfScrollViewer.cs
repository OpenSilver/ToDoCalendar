﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace ToDoCalendarControl.Helpers
{
    public class AutoScrollIfAtEdgeOfScrollViewer
    {
        private const int TimerIntervalInMilliseconds = 250; // in milliseconds
        private const double SpeedOfScrollViewerAutomaticScroll = 140; // in pixels per automatic scroll

        private Thumb _thumb;
        private Point _absolutePosition;
        private DispatcherTimer _dispatcherTimer;
        private bool _positionHasChanged;
        private Action _actionToScroll;
        private bool _detectionIsRunning;

        private enum ScrollDirection
        {
            Top, Bottom, Left, Right, None
        }

        private double _thresholdForScrollViewerAutomaticScroll = 30; // in pixels
        public double ThresholdForScrollViewerAutomaticScroll
        {
            get => _thresholdForScrollViewerAutomaticScroll;
            set => _thresholdForScrollViewerAutomaticScroll = value;
        }

        public void StartDetectionByPassingAThumb(Thumb thumb)
        {
            if (_detectionIsRunning)
                StopDetection();

            _thumb = thumb;
            _thumb.DragDelta -= Thumb_DragDelta;
            _thumb.DragDelta += Thumb_DragDelta;
            _thumb.Unloaded -= Thumb_Unloaded;
            _thumb.Unloaded += Thumb_Unloaded;
            StartDetection();
        }

        public void StartDetectionBySettingAbsolutePositionOnARegularBasis()
        {
            if (_detectionIsRunning)
                StopDetection();
            StartDetection();
        }

        public void SetAbsolutePosition(Point absolutePosition)
        {
            if (_absolutePosition != absolutePosition)
            {
                _positionHasChanged = true;
                _absolutePosition = absolutePosition;
            }
        }

        public void StopDetection()
        {
            if (_dispatcherTimer != null)
            {
                _dispatcherTimer.Stop();
                _dispatcherTimer = null;
            }
            if (_thumb != null)
            {
                _thumb.DragDelta -= Thumb_DragDelta;
                _thumb.Unloaded -= Thumb_Unloaded;
                _thumb = null;
            }
            _detectionIsRunning = false;
        }

        private void StartDetection()
        {
            _dispatcherTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 0, 0, TimerIntervalInMilliseconds)
            };
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Start();
            _detectionIsRunning = true;
        }

        private void Thumb_Unloaded(object sender, RoutedEventArgs e)
        {
            StopDetection();
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            // Calculate thumb position:
            var thumb = (Thumb)sender;
            var thumbActualWidth = double.IsNaN(thumb.ActualWidth) ? 0d : thumb.ActualWidth;
            var thumbActualHeight = double.IsNaN(thumb.ActualHeight) ? 0d : thumb.ActualHeight;
            GeneralTransform generalTransform = null;
            try
            {
                generalTransform = thumb.TransformToVisual(Application.Current.RootVisual);
            }
            catch (ArgumentException)
            {
            }
            var newAbsolutePosition = new Point();
            if (generalTransform != null)
            {
                newAbsolutePosition = generalTransform.Transform(new Point(thumbActualWidth / 2, thumbActualHeight / 2));
            }
            if (_absolutePosition != newAbsolutePosition)
            {
                _positionHasChanged = true;
                _absolutePosition = newAbsolutePosition;
            }
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (_positionHasChanged)
            {
                _positionHasChanged = false;
                if (DetermineActionToScrollIfAny(out Action actionToScroll))
                    _actionToScroll = actionToScroll;
                else
                    _actionToScroll = null;
            }

            _actionToScroll?.Invoke();
        }

        private bool DetermineActionToScrollIfAny(out Action actionToScroll)
        {
            bool scrollShouldTakePlace = false;
            actionToScroll = null;

            // First, look at the popups (Silverlight only, and useful only if the tree is on a popup such as a ChildWindow):
            foreach (var popup in VisualTreeHelper.GetOpenPopups()) //todo: verify that, in case of a popup, the "position" variable is correct (in fact, if the popup is not in position (0,0), the call to "VisualTreeHelper.FindElementsInHostCoordinates" may be wrong)
            {
                scrollShouldTakePlace = DetermineActionToScrollIfAny(_absolutePosition, popup, _thresholdForScrollViewerAutomaticScroll, out actionToScroll);
                if (scrollShouldTakePlace)
                    break;
            }

            // If not found in the popups, look at the rest of the application:
            if (!scrollShouldTakePlace)
            {
                scrollShouldTakePlace = DetermineActionToScrollIfAny(_absolutePosition, Application.Current.RootVisual, _thresholdForScrollViewerAutomaticScroll, out actionToScroll);
            }

            return scrollShouldTakePlace;
        }

        private static bool DetermineActionToScrollIfAny(Point mousePointerPosition, UIElement rootControl, double thresholdForScrollViewerAutomaticScroll, out Action actionToScroll)
        {
            actionToScroll = null;
            bool scrollShouldTakePlace = false;
            foreach (var scrollViewer in FindScrollViewersUnderPointer(mousePointerPosition, rootControl)) //todo: look also in the popups
            {
                // Check if the thumb is at the edge of the scrollviewer:
                if (IsPointerAtEdgeOfScrollViewer(mousePointerPosition, scrollViewer, thresholdForScrollViewerAutomaticScroll, out ScrollDirection scrollDirection))
                {
                    // Scroll the ScrollViewer accordingly, if possible:
                    switch (scrollDirection)
                    {
                        case ScrollDirection.Top:
                            if (scrollViewer.VerticalOffset > SpeedOfScrollViewerAutomaticScroll)
                            {
                                actionToScroll = () => scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - SpeedOfScrollViewerAutomaticScroll);
                                scrollShouldTakePlace = true;
                            }
                            else if (scrollViewer.VerticalOffset > 0)
                            {
                                actionToScroll = () => scrollViewer.ScrollToVerticalOffset(0);
                                scrollShouldTakePlace = true;
                            }
                            break;
                        case ScrollDirection.Bottom:
                            if (scrollViewer.VerticalOffset < scrollViewer.ScrollableHeight - SpeedOfScrollViewerAutomaticScroll)
                            {
                                actionToScroll = () => scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + SpeedOfScrollViewerAutomaticScroll);
                                scrollShouldTakePlace = true;
                            }
                            else if (scrollViewer.VerticalOffset < scrollViewer.ScrollableHeight)
                            {
                                actionToScroll = () => scrollViewer.ScrollToVerticalOffset(scrollViewer.ScrollableHeight);
                                scrollShouldTakePlace = true;
                            }
                            break;
                        case ScrollDirection.Left:
                            if (scrollViewer.HorizontalOffset > SpeedOfScrollViewerAutomaticScroll)
                            {
                                actionToScroll = () => scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - SpeedOfScrollViewerAutomaticScroll);
                                scrollShouldTakePlace = true;
                            }
                            else if (scrollViewer.HorizontalOffset > 0)
                            {
                                actionToScroll = () => scrollViewer.ScrollToHorizontalOffset(0);
                                scrollShouldTakePlace = true;
                            }
                            break;
                        case ScrollDirection.Right:
                            if (scrollViewer.HorizontalOffset < scrollViewer.ScrollableWidth - SpeedOfScrollViewerAutomaticScroll)
                            {
                                actionToScroll = () => scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + SpeedOfScrollViewerAutomaticScroll);
                                scrollShouldTakePlace = true;
                            }
                            else if (scrollViewer.HorizontalOffset < scrollViewer.ScrollableWidth)
                            {
                                actionToScroll = () => scrollViewer.ScrollToHorizontalOffset(scrollViewer.ScrollableWidth);
                                scrollShouldTakePlace = true;
                            }
                            break;
                        case ScrollDirection.None:
                            break;
                        default:
                            break;
                    }
                }

                if (scrollShouldTakePlace)
                    break;
            }

            return scrollShouldTakePlace;
        }

        private static bool IsPointerAtEdgeOfScrollViewer(Point mousePointerPosition, ScrollViewer scrollViewer, double thresholdForScrollViewerAutomaticScroll, out ScrollDirection scrollDirection)
        {
            // Check that the scrollViewer is initialized in the visual tree:
            if (!double.IsNaN(scrollViewer.ActualWidth) && !double.IsNaN(scrollViewer.ActualHeight))
            {
                // Get the absolute coordinates of the ScrollViewer:
                GeneralTransform gt = scrollViewer.TransformToVisual(Application.Current.RootVisual);
                Point scrollViewerTopLeftPosition = gt.Transform(new Point(0, 0));
                Point scrollViewerBottomRightPosition = gt.Transform(new Point(scrollViewer.ActualWidth, scrollViewer.ActualHeight));

                // Compare the coordinates of the scrollviewer with the position of the mouse pointer:
                if (mousePointerPosition.Y < scrollViewerBottomRightPosition.Y && mousePointerPosition.Y > scrollViewerBottomRightPosition.Y - thresholdForScrollViewerAutomaticScroll)
                {
                    scrollDirection = ScrollDirection.Bottom;
                    return true;
                }
                else if (mousePointerPosition.Y > scrollViewerTopLeftPosition.Y && mousePointerPosition.Y < scrollViewerTopLeftPosition.Y + thresholdForScrollViewerAutomaticScroll)
                {
                    scrollDirection = ScrollDirection.Top;
                    return true;
                }
                else if (mousePointerPosition.X < scrollViewerBottomRightPosition.X && mousePointerPosition.X > scrollViewerBottomRightPosition.X - thresholdForScrollViewerAutomaticScroll)
                {
                    scrollDirection = ScrollDirection.Right;
                    return true;
                }
                else if (mousePointerPosition.X > scrollViewerTopLeftPosition.X && mousePointerPosition.X < scrollViewerTopLeftPosition.X + thresholdForScrollViewerAutomaticScroll)
                {
                    scrollDirection = ScrollDirection.Left;
                    return true;
                }
            }
            scrollDirection = ScrollDirection.None;
            return false;
        }

        private static IEnumerable<ScrollViewer> FindScrollViewersUnderPointer(Point position, UIElement rootControl)
        {
            var list = VisualTreeHelper.FindElementsInHostCoordinates(position, rootControl);
            foreach (UIElement uiElement in list)
            {
                if (uiElement is ScrollViewer viewer)
                {
                    yield return viewer;
                }
            }
        }
    }
}
