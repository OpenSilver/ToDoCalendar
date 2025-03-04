using MetroStyleApps;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ToDoCalendarControl.Resources;

namespace ToDoCalendarControl
{
    static class RenderingHelpers
    {
        //----------------
        // SETTINGS
        //----------------

        // Rendering a day:
        const double Column1Width = 25d;
        const double Column2WidthIfNormalSpaceRequired = 30d;
        const double Column2WidthIfMoreSpaceRequired = 80d;
        const double SeparatorHeight = 1d;
        const double ColumnsSeparatorWidth = 2d;
        static readonly Brush SeparatorColor = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
        static readonly Brush SeparatorColorIfFirstDayOfWeek = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
        static readonly Brush ColumnsSeparatorColor = new SolidColorBrush(Color.FromArgb(255, 136, 136, 136));
        static readonly Brush BackgroundColorForWorkDays = new SolidColorBrush(Color.FromArgb(255, 250, 250, 250));
        static readonly Brush BackgroundColorForHolidays = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220));
        internal static readonly Brush BackgroundColorForToday = (Brush)Application.Current.Resources["PhoneAccentBrush"];
        static readonly Brush DayNumberColorForWorkDays = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
        static readonly Brush DayNumberColorForHolidays = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
        static readonly Brush DayNumberColorForToday = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255));
        static readonly Brush DayOfWeekColorForWorkDays = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
        static readonly Brush DayOfWeekColorForHolidays = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
        static readonly Brush DayOfWeekColorForToday = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255));

        // Rendering an event:
        const double EventFontSize = 12;
        const double EventFontSizeWhenEditing = 18;
        const double EventFontSizeWhenToday = 14;
        const double StrikethroughLineHeight = 5;
        const double StrikethroughLineHeightIfToday = 9;
        const double MaxWidthWhenEventIsMarkedAsDone = 80;
        const double EventOpacityWhenDone = 0.5;
        const double EventOpacityWhenLowPriority = 0.3;
        const double EventOpacityWhenLowPriorityIfToday = 0.6;
        static readonly Thickness StrikethroughMargin = new Thickness(-5, 2, -5, 0);
        static readonly Brush StrikethroughLineColor = new SolidColorBrush(Color.FromArgb(80, 0, 0, 0));
        static readonly Brush EventBackgroundColorDefault = (Brush)Application.Current.Resources["PhoneAccentBrush"]; //new SolidColorBrush(Color.FromArgb(255, 210, 210, 210));
        static readonly Brush EventBackgroundColorWhenHighPriority = new SolidColorBrush(Colors.Black);
        static readonly Brush EventBackgroundColorWhenDone = new SolidColorBrush(Color.FromArgb(255, 160, 160, 160));
        static readonly Brush EventBackgroundColorWhenInfo = new SolidColorBrush(Colors.Transparent);
        static readonly Brush EventTextBackgroundWhenNotEditing = new SolidColorBrush(Colors.Transparent);
        static readonly Brush EventTextBackgroundWhenEditingDefault = (Brush)Application.Current.Resources["PhoneAccentBrush"];
        static readonly Brush EventTextColor = new SolidColorBrush(Colors.White);
        static readonly Brush EventTextColorWhenEditing = new SolidColorBrush(Colors.White);
        static readonly Brush EventTextColorWhenInfoBeforeCurrentDate = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0));
        static readonly Brush EventTextColorWhenInfoAfterCurrentDate = (Brush)Application.Current.Resources["PhoneAccentBrush"];
        static readonly CornerRadius EventCornerRadius = new CornerRadius(10);
        static readonly Thickness EventMargin = new Thickness(0);
        static readonly Thickness EventTextBoxMarginWhenNotEditing = new Thickness(-6, 0, -6, -1);
        static readonly Thickness EventTextBoxMarginWhenEditing = new Thickness(-2, 5, -2, 4);

        // Rendering the month header:
        const double LeftMarginOfMonthName = 5d; // in pixels
        static readonly Brush MonthHeaderBackgroundColor = new SolidColorBrush(Color.FromArgb(255, 136, 136, 136));
        static readonly Brush MonthHeaderForegroundColor = new SolidColorBrush(Colors.White);

        private static Point _originPosition;

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

            var rootControl = new Border()
            {
                BorderThickness = new Thickness(0, 0, ColumnsSeparatorWidth, 0),
                BorderBrush = ColumnsSeparatorColor
            };

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
                MinHeight = 14,
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
            dayContainer.Children.Add(topSeparator);
            dayContainer.Children.Add(eventsContainer);
            mainContainer.Children.Add(dayContainer);
            mainContainer.Children.Add(dragAndDropTarget);
            rootControl.Child = mainContainer;

            return rootControl;
        }

        static async void DragAndDropTarget_SourceDropped(object sender, DragAndDropEventArgs e)
        {
            var customDragAndDropTarget = (CustomDragAndDropTarget)sender;

            // Check if the user has dragged the control for adding an event or he/she has dragged another event:
            if (e.Source.Name == "DRAGSOURCE_NewEvent")
            {
                var (newEventModel, dayModel) = await customDragAndDropTarget.Controller.AddEvent(customDragAndDropTarget.Day);

                // Make sure the event enters Edit Mode immediately after being created:
                customDragAndDropTarget.Dispatcher.BeginInvoke(async () =>
                {
                    await Task.Delay(1); // Workaround to ensure that the UI element for the new event has been loaded.
                    customDragAndDropTarget.Controller.EditEvent(newEventModel, dayModel, customDragAndDropTarget.Day);
                });
            }
            else
            {
                // If we arrived here, it means that the user has dragged another event.

                // Get the frameworkelement of the event:
                DragAndDropSource source = (DragAndDropSource)e.Source;

                // Get the information about the event being dragged:
                InformationAboutEventBeingDragged info = (InformationAboutEventBeingDragged)source.DataContext;

                // Move the event:
                await customDragAndDropTarget.Controller.MoveEvent(info.EventModel, info.DayModel, info.Day, customDragAndDropTarget.Day);
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
                mainContainer = new WrapPanel()
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
                    var eventControl = eventModel.IsReadOnly
                        ? RenderReadOnlyEvent(eventModel, isToday)
                        : RenderEvent(eventModel, dayModelIfAny, day, controller, isToday, shouldSetFocus);

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

        public static FrameworkElement RenderEvent(EventModel eventModel, DayModel dayModel, DateTime day, Controller controller, bool isToday, bool setFocus = false)
        {
            //----------------
            // CREATE ELEMENTS
            //----------------

            Brush functionToDetermineEventForeground() =>
                eventModel.EventType switch
                {
                    EventType.Info => DatesHelpers.GetDateWithoutTime(day) <= DatesHelpers.GetTodayDateWithoutTime() ? EventTextColorWhenInfoBeforeCurrentDate : EventTextColorWhenInfoAfterCurrentDate,
                    EventType.Unspecified => eventModel.CalendarColor.HasValue ? new SolidColorBrush(eventModel.CalendarColor.Value) : EventTextColor,
                    _ => EventTextColor
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
                Background = eventModel.EventType switch
                {
                    EventType.Info or EventType.Unspecified => EventBackgroundColorWhenInfo,
                    _ when eventModel.IsMarkedAsDone && !isToday => EventBackgroundColorWhenDone,
                    EventType.HighPriority => EventBackgroundColorWhenHighPriority,
                    _ => EventBackgroundColorDefault
                },
                Opacity = eventModel.EventType switch
                {
                    EventType.LowPriority => isToday ? EventOpacityWhenLowPriorityIfToday : EventOpacityWhenLowPriority,
                    //EventType.Unspecified => 0.8,
                    _ => eventModel.IsMarkedAsDone ? EventOpacityWhenDone : 1d
                },
                Padding = new Thickness(12, 0, 12, 0),
                Margin = EventMargin,
                Cursor = Cursors.Hand
            };

            var mainContainer = new Grid() { MinHeight = isToday ? 30 : 20 };
            mainContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

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
                Margin = EventTextBoxMarginWhenNotEditing,
                Padding = new Thickness(0),
                CaretBrush = EventTextColorWhenEditing,
                FontSize = (isToday ? EventFontSizeWhenToday : EventFontSize),
                FontWeight = FontWeights.Bold,
                MaxWidth = (eventModel.IsMarkedAsDone && !isToday ? MaxWidthWhenEventIsMarkedAsDone : double.PositiveInfinity),
                TextWrapping = TextWrapping.NoWrap,
                Height = double.NaN,
                IsHitTestVisible = false
            };

            var borderToStartEditingOnMouseUpRatherThanMouseDown = new Border()
            {
                Margin = new Thickness(-10, 0, -10, 0),
                Background = new SolidColorBrush(Colors.Transparent)
            };

            borderToStartEditingOnMouseUpRatherThanMouseDown.MouseLeftButtonDown += (s, e) => _originPosition = e.GetPosition(null);

            borderToStartEditingOnMouseUpRatherThanMouseDown.MouseLeftButtonUp += (s, e) => // Note: we use this control so as to put focus on the TextBox only on MouseUp (instead of MouseDown) so that the user can "Hold" to drag and drop instead of editing.
            {
                if (DragAndDropSource.IsSameSpot(_originPosition, e.GetPosition(null)))
                {
                    MetroHelpers.SetFocus(eventTitle);
                }
            };

            eventTitle.GotFocus += (s, e) =>
            {
                eventTitle.FontSize = EventFontSizeWhenEditing;
                eventTitle.Background = EventTextBackgroundWhenEditingDefault;
                eventTitle.Foreground = EventTextColorWhenEditing;
                eventTitle.Margin = EventTextBoxMarginWhenEditing;
                eventTitle.TextWrapping = TextWrapping.Wrap;
                eventTitle.MinWidth = 2000; // to prevent flickering on every text change, because the StandAloneWrapPanel is recalculating sizes
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
                eventTitle.Margin = EventTextBoxMarginWhenNotEditing;
                eventTitle.TextWrapping = TextWrapping.NoWrap;
                eventTitle.MinWidth = 0;
                eventTitle.MaxWidth = (eventModel.IsMarkedAsDone && !isToday ? MaxWidthWhenEventIsMarkedAsDone : double.PositiveInfinity);
                eventTitle.IsHitTestVisible = false;
                borderToStartEditingOnMouseUpRatherThanMouseDown.Visibility = Visibility.Visible;
                controller.SignalThatEditingModeStopped();
            };

            eventTitle.TextChanged += (s, e) =>
            {
                // Sync the model:
                eventModel.Title = ((TextBox)s).Text;
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

            if (eventModel.IsMarkedAsDone)
            {
                var lineToStrikethrough = new Rectangle
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Height = (isToday ? StrikethroughLineHeightIfToday : StrikethroughLineHeight),
                    Fill = StrikethroughLineColor,
                    Margin = StrikethroughMargin,
                };
                mainContainer.Children.Add(lineToStrikethrough);
            }

            mainContainer.Children.Add(borderToStartEditingOnMouseUpRatherThanMouseDown);
            mainBorder.Child = mainContainer;
            dragAndDropSource.Content = mainBorder;

            return dragAndDropSource;
        }

        private static FrameworkElement RenderReadOnlyEvent(EventModel eventModel, bool isToday)
        {
            var eventTitle = new TextBlock()
            {
                Text = eventModel.Title,
                Foreground = eventModel.CalendarColor.HasValue ? new SolidColorBrush(eventModel.CalendarColor.Value) : EventTextColor,
                Margin = new Thickness(12, 0, 12, 0),
                Padding = new Thickness(0),
                FontSize = isToday ? EventFontSizeWhenToday : EventFontSize,
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center
            };

            return eventTitle;
        }

        public static FrameworkElement RenderMonthHeader(DateTime firstDayOfMonth)
        {

            //----------------
            // CREATE ELEMENTS
            //----------------

            var outerContainer = new Border()
            {
                Height = 16,
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
                FontWeight = FontWeights.Bold,
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
