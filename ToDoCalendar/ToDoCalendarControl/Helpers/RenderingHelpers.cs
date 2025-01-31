using MetroStyleApps;
using System;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ToDoCalendarControl
{
    static class RenderingHelpers
    {
        //----------------
        // SETTINGS
        //----------------

        // Rendering a day:
        const double Column1Width = 35d;
        const double Column2WidthIfNormalSpaceRequired = 30d;
        const double Column2WidthIfMoreSpaceRequired = 80d;
        const double SeparatorHeight = 1d;
        static readonly Brush SeparatorColor = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
        static readonly Brush SeparatorColorIfFirstDayOfWeek = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
        static readonly Brush BackgroundColorForWorkDays = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
        static readonly Brush BackgroundColorForHolidays = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220));
        static readonly Brush BackgroundColorForToday = (Brush)Application.Current.Resources["PhoneAccentBrush"];
        static readonly Brush DayNumberColorForWorkDays = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
        static readonly Brush DayNumberColorForHolidays = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
        static readonly Brush DayNumberColorForToday = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255));
        static readonly Brush DayOfWeekColorForWorkDays = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
        static readonly Brush DayOfWeekColorForHolidays = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
        static readonly Brush DayOfWeekColorForToday = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255));

        // Rendering an event:
        const double EventFontSize = 18;
        const double EventFontSizeWhenEditing = 36;
        const double EventFontSizeWhenToday = 30;
        const double StrikethroughLineHeight = 5;
        const double StrikethroughLineHeightIfToday = 9;
        const double MaxWidthWhenEventIsMarkedAsDone = 80;
        const double EventOpacityWhenDone = 0.5;
        const double EventOpacityWhenLowPriority = 0.3;
        const double EventOpacityWhenLowPriorityIfToday = 0.6;
        static readonly Thickness StrikethroughMargin = new Thickness(3, 2, 3, 0);
        static readonly Brush StrikethroughLineColor = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0));
        static readonly Brush EventBackgroundColor = (Brush)Application.Current.Resources["PhoneAccentBrush"]; //new SolidColorBrush(Color.FromArgb(255, 210, 210, 210));
        static readonly Brush EventBackgroundColorWhenHighPriority = new SolidColorBrush(Colors.Black);
        static readonly Brush EventBackgroundColorWhenDone = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
        static readonly Brush EventBackgroundColorWhenInfo = new SolidColorBrush(Colors.Transparent);
        static readonly Brush EventTextBackgroundWhenNotEditing = new SolidColorBrush(Colors.Transparent);
        static readonly Brush EventTextBackgroundWhenEditing = new SolidColorBrush(Colors.White);
        static readonly Brush EventTextColor = new SolidColorBrush(Colors.White);
        static readonly Brush EventTextColorWhenEditing = new SolidColorBrush(Colors.Black);
        static readonly Brush EventTextColorWhenInfoBeforeCurrentDate = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0));
        static readonly Brush EventTextColorWhenInfoAfterCurrentDate = (Brush)Application.Current.Resources["PhoneAccentBrush"];
        static readonly CornerRadius EventCornerRadius = new CornerRadius(10);
        static readonly Thickness EventMargin = new Thickness(3);
        static readonly Thickness EventTextBoxMargin = new Thickness(-6, -12, -6, -12);

        // Rendering the month header:
        const double LeftMarginOfMonthName = 10d; // in pixels
        static readonly Brush MonthHeaderBackgroundColor = new SolidColorBrush(Color.FromArgb(255, 190, 190, 190));
        static readonly Brush MonthHeaderForegroundColor = new SolidColorBrush(Colors.White);


        //----------------
        // METHODS
        //----------------

        public static void AddDaysToContainer(Panel container, DateTime firstDay, DateTime lastDay, Controller controller, bool renderEvents = true, bool insertAtBeginningRatherThanAddToEnd = false)
        {
            //----------------
            // ADD EACH DAY
            //----------------

            int dayCount = (int)((lastDay - firstDay).TotalDays) + 1;
            int numberOfHeadersRendered = 0;
            for (int i = 0; i < dayCount; i++)
            {
                var day = firstDay.AddDays(i);

                // If it is the first day of the month, display the name of the month:
                if (day.Day == 1)
                {
                    if (insertAtBeginningRatherThanAddToEnd)
                        container.Children.Insert(i + numberOfHeadersRendered, RenderMonthHeader(day));
                    else
                        container.Children.Add(RenderMonthHeader(day));
                    ++numberOfHeadersRendered;
                }

                // Display the day:
                if (insertAtBeginningRatherThanAddToEnd)
                    container.Children.Insert(i + numberOfHeadersRendered, RenderDay(day, controller, renderEvents));
                else
                    container.Children.Add(RenderDay(day, controller, renderEvents));
            }
        }

        public static FrameworkElement RenderDay(DateTime day, Controller controller, bool renderEvents)
        {
            var isWorkDay = DatesHelpers.IsWorkDay(day);
            var isToday = (day == DatesHelpers.GetTodayDateWithoutTime());
            var isFirstDayOfWeek = DatesHelpers.IsFirstDayOfWeek(day);

            //----------------
            // CREATE ELEMENTS
            //----------------

            var rootControl = new Grid();

            //if (isToday)
            //{
            //    var backgroundOverlayIfDateIsToday = new Rectangle()
            //    {
            //        Fill = BackgroundColorForToday,
            //        Opacity = OpacityOfBackgroundOverlayToShowWhenDateIsToday
            //    };
            //    rootControl.Children.Add(backgroundOverlayIfDateIsToday);
            //}

            var mainContainer = new StackPanel()
            {
                Background = (isToday ? BackgroundColorForToday : (isWorkDay ? BackgroundColorForWorkDays : BackgroundColorForHolidays)),
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var dayContainer = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var dayNumber = new TextBlock()
            {
                Text = day.Day.ToString(),
                Width = Column1Width,
                Foreground = (isToday ? DayNumberColorForToday : (isWorkDay ? DayNumberColorForWorkDays : DayNumberColorForHolidays)),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                TextAlignment = System.Windows.TextAlignment.Right,
                Margin = new Thickness(0, 0, 0, 0)
            };

            var column2Width = (DatesHelpers.DoDaysOfWeekRequireMoreSpace() ? Column2WidthIfMoreSpaceRequired : Column2WidthIfNormalSpaceRequired);

            var dayOfWeek = new TextBlock()
            {
                Text = DatesHelpers.GetLetterFromDayOfWeek(day.DayOfWeek),
                Width = column2Width,
                Foreground = (isToday ? DayOfWeekColorForToday : (isWorkDay ? DayOfWeekColorForWorkDays : DayOfWeekColorForHolidays)),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                TextAlignment = System.Windows.TextAlignment.Center,
                Margin = new Thickness(Column1Width, 0, 0, 0)
            };

            var eventsContainer = new Border()
            {
                MinHeight = 30,
                Margin = new Thickness(Column1Width + column2Width, 1, 1, 1)
            };

            var topSeparator = new Rectangle()
            {
                Height = SeparatorHeight,
                Fill = (isFirstDayOfWeek ? SeparatorColorIfFirstDayOfWeek : SeparatorColor),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top
            };

            var dragAndDropTarget = new CustomDragAndDropTarget()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 0,
                IsHitTestVisible = false,
                Day = day,
                Controller = controller
            };


            //----------------
            // REGISTER THE CONTROLS EVENTS AND REFRESH THE DAY EVENTS:
            //----------------

            dragAndDropTarget.SourceDropped += DragAndDropTarget_SourceDropped;

            //// COMMENTED BECAUSE IT DIDN'T WORK ON WINDOWS PHONE 7.1 (though it worked on Win Phone 8) (it seems to be due to the fact that weak references are not supported in WP7)
            //Action<RefreshDayRequestedEventArgs> actionToRefreshDay = (e) =>
            //    {
            //        if (e.Day == day)
            //        {
            //            eventsContainer.Child = RenderDayEvents(day, controller, e.SetFocusToLastEventOfTheDay);
            //        }
            //    };
            var refreshDayRequestedHandler = new RefreshDayRequestedHandler(eventsContainer, day, controller);

            //todo: test that the following "weak event" is indeed "weak" and does not leak:
            //controller.RefreshDayRequested += WeakEventsHelpers.MakeWeakHandlerForRefreshDayRequestedEvent(actionToRefreshDay, h => controller.RefreshDayRequested -= h);
            controller.RefreshDayRequested += WeakEventsHelpers.MakeWeakHandler<RefreshDayRequestedEventArgs>(refreshDayRequestedHandler.Controller_RefreshDayRequested, h => controller.RefreshDayRequested -= h);
            //controller.RefreshDayRequested += WeakEventsHelpers.MakeWeakHandlerForRefreshDayRequestedEvent(refreshDayRequestedHandler.Controller_RefreshDayRequested, h => controller.RefreshDayRequested -= h);

            // Refresh the day events once:
            if (renderEvents)
            {
                //actionToRefreshDay(new RefreshDayRequestedEventArgs(day));
                refreshDayRequestedHandler.Controller_RefreshDayRequested(new RefreshDayRequestedEventArgs(day));
            }

            //----------------
            // ADD ELEMENTS TO OUTER CONTAINER
            //----------------

            dayContainer.Children.Add(dayNumber);
            dayContainer.Children.Add(dayOfWeek);
            dayContainer.Children.Add(eventsContainer);
            dayContainer.Children.Add(topSeparator);
            mainContainer.Children.Add(dayContainer);
            mainContainer.Children.Add(dragAndDropTarget);
            rootControl.Children.Add(mainContainer);

            return rootControl;
        }

        static void DragAndDropTarget_SourceDropped(object sender, DragAndDropEventArgs e)
        {
            var customDragAndDropTarget = (CustomDragAndDropTarget)sender;

            // Check if the user has dragged the control for adding an event or he/she has dragged another event:
            if (e.Source.Name == "DRAGSOURCE_NewEvent")
            {
                customDragAndDropTarget.Controller.AddEvent(customDragAndDropTarget.Day);
            }
            else
            {
                // If we arrived here, it means that the user has dragged another event.

                // Get the frameworkelement of the event:
                DragAndDropSource source = (DragAndDropSource)e.Source;

                // Get the information about the event being dragged:
                InformationAboutEventBeingDragged info = (InformationAboutEventBeingDragged)source.DataContext;

                // Move the event:
                customDragAndDropTarget.Controller.MoveEvent(info.EventModel, info.DayModel, info.Day, customDragAndDropTarget.Day);
            }
        }

        public static FrameworkElement RenderDayEvents(DateTime day, Controller controller, bool setFocusToLastEventOfTheDay = false)
        {
            DayModel dayModelIfAny = controller.Model.Days.ContainsKey(day) ? controller.Model.Days[day] : null;
            var isToday = (day == DatesHelpers.GetTodayDateWithoutTime());

            //----------------
            // CREATE ELEMENTS
            //----------------

            var rootControl = new Border();
            Panel mainContainer;
            if (isToday)
            {
                mainContainer = new StackPanel()
                {
                    Orientation = Orientation.Vertical
                };
            }
            else
            {
                mainContainer = new StandAloneWrapPanel()
                {
                    Orientation = Orientation.Horizontal
                };
            }

            if (dayModelIfAny != null)
            {
                int index = 0;
                int count = dayModelIfAny.Events.Count;
                foreach (EventModel eventModel in dayModelIfAny.Events)
                {
                    bool shouldSetFocus = setFocusToLastEventOfTheDay && index == (count - 1);
                    var eventControl = RenderEvent(eventModel, dayModelIfAny, day, controller, shouldSetFocus);
                    mainContainer.Children.Add(eventControl);
                    index++;
                }
            }

            //----------------
            // ADD ELEMENTS TO OUTER CONTAINER
            //----------------

            rootControl.Child = mainContainer;


            return rootControl;
        }

        public static FrameworkElement RenderEvent(EventModel eventModel, DayModel dayModel, DateTime day, Controller controller, bool setFocus = false)
        {
            var isToday = (day == DatesHelpers.GetTodayDateWithoutTime());


            //----------------
            // CREATE ELEMENTS
            //----------------

            Func<Brush> functionToDetermineEventForeground = () =>
                {
                    return (eventModel.EventType == EventType.Info ? (DatesHelpers.GetDateWithoutTime(day) <= DatesHelpers.GetTodayDateWithoutTime() ? EventTextColorWhenInfoBeforeCurrentDate : EventTextColorWhenInfoAfterCurrentDate) : EventTextColor);
                };

            var dragAndDropSource = new DragAndDropSource()
            {
                HoldToStartDrag = true,
                EnlargeSourceDuringDrag = true,
                DataContext = new InformationAboutEventBeingDragged(eventModel, dayModel, day)
            };

            var mainBorder = new Border()
            {
                CornerRadius = EventCornerRadius,
                Background = (eventModel.EventType == EventType.Info ? EventBackgroundColorWhenInfo : (eventModel.IsMarkedAsDone && !isToday ? EventBackgroundColorWhenDone : (eventModel.EventType == EventType.HighPriority ? EventBackgroundColorWhenHighPriority : EventBackgroundColor))),
                Opacity = (eventModel.EventType == EventType.LowPriority ? (isToday ? EventOpacityWhenLowPriorityIfToday : EventOpacityWhenLowPriority) : (eventModel.IsMarkedAsDone ? EventOpacityWhenDone : 1d)),
                Padding = new Thickness(15, 2, 15, 2),
                Margin = EventMargin
            };

            var mainContainer = new Grid() { Height = 20 };

            var lineToStrikethrough = new Rectangle()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = (isToday ? StrikethroughLineHeightIfToday : StrikethroughLineHeight),
                Fill = StrikethroughLineColor,
                Margin = StrikethroughMargin,
                Visibility = (eventModel.IsMarkedAsDone ? Visibility.Visible : Visibility.Collapsed),
            };

            var eventTitle = new TextBox()
            {
                Text = eventModel.Title,
                Background = EventTextBackgroundWhenNotEditing,
                Foreground = functionToDetermineEventForeground(),
#if !OPENSILVER
                InputScope = new InputScope() { Names = { new InputScopeName() { NameValue = InputScopeNameValue.Text } } }, // This will enable spell check and word auto-completion.
#else
                IsSpellCheckEnabled = true,
#endif
                BorderThickness = new Thickness(0),
                Margin = EventTextBoxMargin,
                Padding = new Thickness(0),
                FontSize = (isToday ? EventFontSizeWhenToday : EventFontSize),
                MaxWidth = (eventModel.IsMarkedAsDone && !isToday ? MaxWidthWhenEventIsMarkedAsDone : double.PositiveInfinity),
                TextWrapping = TextWrapping.NoWrap,
                Height = double.NaN,
                IsHitTestVisible = false
            };

            var borderToStartEditingOnMouseUpRatherThanMouseDown = new Border()
            {
                Background = new SolidColorBrush(Colors.Transparent)
            };

            borderToStartEditingOnMouseUpRatherThanMouseDown.MouseLeftButtonUp += (s, e) => // Note: we use this control so as to put focus on the TextBox only on MouseUp (instead of MouseDown) so that the user can "Hold" to drag and drop instead of editing.
                {
                    MetroHelpers.DispatcherRunAsync(() =>
                        {
                            MetroHelpers.SetFocus(eventTitle);
                        });
                };

            eventTitle.GotFocus += (s, e) =>
            {
                eventTitle.FontSize = EventFontSizeWhenEditing;
                eventTitle.Background = EventTextBackgroundWhenEditing;
                eventTitle.Foreground = EventTextColorWhenEditing;
                eventTitle.TextWrapping = TextWrapping.Wrap;
                eventTitle.MaxWidth = double.PositiveInfinity;
                eventTitle.IsHitTestVisible = true;
                borderToStartEditingOnMouseUpRatherThanMouseDown.Visibility = Visibility.Collapsed;
                controller.SignalThatEditingModeStarted(new EditingModeStartedEventArgs(eventTitle, eventModel, dayModel, day));
            };

            eventTitle.LostFocus += (s, e) =>
            {
                eventTitle.FontSize = (isToday ? EventFontSizeWhenToday : EventFontSize);
                eventTitle.Background = EventTextBackgroundWhenNotEditing;
                eventTitle.Foreground = functionToDetermineEventForeground();
                eventTitle.TextWrapping = TextWrapping.NoWrap;
                eventTitle.MaxWidth = (eventModel.IsMarkedAsDone && !isToday ? MaxWidthWhenEventIsMarkedAsDone : double.PositiveInfinity);
                eventTitle.IsHitTestVisible = false;
                borderToStartEditingOnMouseUpRatherThanMouseDown.Visibility = Visibility.Visible;
                controller.SignalThatEditingModeStopped();
            };

            eventTitle.TextChanged += (s, e) =>
                {
                    // Sync the model:
                    eventModel.Title = ((TextBox)s).Text;

                    // Remember that we need to save the changes:
                    controller.RememberThatThereAreUnsavedChanges();
                };

            eventTitle.KeyDown += (object sender, KeyEventArgs e) =>
                {
                    if (e.Key == Key.Enter)
                        controller.QuitEditingMode();
                };

            dragAndDropSource.DragAndDropStarted += (object sender, EventArgs e) =>
                {
                    controller.QuitEditingMode();
                    controller.LockMainScrollViewer();
                };

            dragAndDropSource.DragAndDropStopped += (object sender, EventArgs e) =>
            {
                controller.UnlockMainScrollViewer();
            };

            //// COMMENTED BECAUSE IT DIDN'T WORK ON WINDOWS PHONE 7.1 (though it worked on Win Phone 8) (it seems to be due to the fact that weak references are not supported in WP7)
            //Action<EditEventRequestedEventArgs> actionToEditEvent = (e) =>
            //    {
            //        if (e.EventModel == eventModel)
            //            MetroHelpers.SetFocus(eventTitle);
            //    };
            var editEventRequestedHandler = new EditEventRequestedRequestedHandler(eventTitle, eventModel);

            //todo: test that the following "weak event" is indeed "weak" and does not leak:
            //controller.EditEventRequested += WeakEventsHelpers.MakeWeakHandler<EditEventRequestedEventArgs>(actionToEditEvent, h => controller.EditEventRequested -= h);
            controller.EditEventRequested += WeakEventsHelpers.MakeWeakHandler<EditEventRequestedEventArgs>(editEventRequestedHandler.Controller_EditEventRequested, h => controller.EditEventRequested -= h);


            if (setFocus)
            {
                // We use a dispatcher because, in order to set the focus, the control must be in the visual tree:
                eventTitle.Dispatcher.BeginInvoke(() =>
                    {
                        MetroHelpers.SetFocus(eventTitle);
                    });
            }

            //----------------
            // ADD ELEMENTS TO OUTER CONTAINER
            //----------------

            mainContainer.Children.Add(eventTitle);
            mainContainer.Children.Add(lineToStrikethrough);
            mainContainer.Children.Add(borderToStartEditingOnMouseUpRatherThanMouseDown);
            mainBorder.Child = mainContainer;
            dragAndDropSource.Content = mainBorder;

            return dragAndDropSource;
        }

        public static FrameworkElement RenderMonthHeader(DateTime firstDayOfMonth)
        {

            //----------------
            // CREATE ELEMENTS
            //----------------

            var outerContainer = new Border()
            {
                Height = 32,
                Background = MonthHeaderBackgroundColor
            };

            var monthName = new TextBlock()
            {
                Text = DatesHelpers.GetMonthHeaderText(firstDayOfMonth),
                Foreground = MonthHeaderForegroundColor,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = System.Windows.TextAlignment.Left,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(LeftMarginOfMonthName, 0, 0, 0)
            };

            //----------------
            // ADD ELEMENTS TO OUTER CONTAINER
            //----------------

            outerContainer.Child = monthName;

            return outerContainer;
        }

        public class RefreshDayRequestedHandler
        {
            Border _eventsContainer;
            DateTime _day;
            Controller _controller;
            public RefreshDayRequestedHandler(Border eventsContainer, DateTime day, Controller controller)
            {
                _eventsContainer = eventsContainer;
                _day = day;
                _controller = controller;
                _eventsContainer.Unloaded += _eventsContainer_Unloaded;
            }

            void _eventsContainer_Unloaded(object sender, RoutedEventArgs e)
            {
                // This handler is here only to make it so that the lifespan of this class (in terms of garbage collection) is the same as that of the EventsContainer instance. (this is achieved by having a two-directional reference: registering any other event would have worked too)
            }

            public void Controller_RefreshDayRequested(RefreshDayRequestedEventArgs e)
            {
                if (e.Day == _day)
                {
                    _eventsContainer.Child = RenderDayEvents(_day, _controller, e.SetFocusToLastEventOfTheDay);
                }
            }
        }

        public class EditEventRequestedRequestedHandler
        {
            TextBox _eventTitle;
            EventModel _eventModel;
            public EditEventRequestedRequestedHandler(TextBox eventTitle, EventModel eventModel)
            {
                _eventTitle = eventTitle;
                _eventModel = eventModel;
                _eventTitle.Unloaded += _eventTitle_Unloaded;
            }

            void _eventTitle_Unloaded(object sender, RoutedEventArgs e)
            {
                // This handler is here only to make it so that the lifespan of this class (in terms of garbage collection) is the same as that of the EventTitle instance. (this is achieved by having a two-directional reference: registering any other event would have worked too)
            }

            public void Controller_EditEventRequested(EditEventRequestedEventArgs e)
            {
                if (e.EventModel == _eventModel)
                    MetroHelpers.SetFocus(_eventTitle);
            }
        }

        public class InformationAboutEventBeingDragged
        {
            public readonly EventModel EventModel;
            public readonly DayModel DayModel;
            public readonly DateTime Day;

            public InformationAboutEventBeingDragged(EventModel eventModel, DayModel dayModel, DateTime day)
            {
                EventModel = eventModel;
                DayModel = dayModel;
                Day = day;
            }
        }
    }
}
