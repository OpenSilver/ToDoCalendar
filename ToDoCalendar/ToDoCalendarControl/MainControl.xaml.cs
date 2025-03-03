using MetroStyleApps;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ToDoCalendarControl.Helpers;
using ToDoCalendarControl.Resources;
using ToDoCalendarControl.Services;

namespace ToDoCalendarControl
{
    public partial class MainControl : UserControl
    {
        const int InitialDayCountBeforeScreen = 5;
        const int InitialDayCountAfterScreen = 10;
        const int NumberOfAdditionalDaysToLoadBefore = 30;
        const int NumberOfAdditionalDaysToLoadAfter = 30;
        const double DayItemHeight = 16;
        const double InitialTodayTopOffset = 150;
        const int MinLandscapeWidth = 1150;
        const int HorizontalScrollMargin = 67;
        const int MaxOptionsPopupWidth = 450;

        public const double LandscapeColumnWidth = 500;

        private readonly IKeyboardService _keyboardService = ServiceLocator.Provider.GetService<IKeyboardService>();

        Controller _controller;
        DateTime _firstDayOfCalendar;
        DateTime _lastDayOfCalendar;
        bool _isMainScrollViewerLocked;

        private bool IsLandscapeMode => ActualWidth > MinLandscapeWidth;

        public MainControl()
        {
            InitializeComponent();

            // Prepare the controller:
            _controller = new Controller(new CalendarServiceSaver(ServiceLocator.Provider.GetRequiredService<ICalendarService>()));
            _controller.EditingModeStarted += Controller_EditingModeStarted;
            _controller.EditingModeStopped += Controller_EditingModeStopped;
            _controller.QuitEditingModeRequested += Controller_QuitEditingModeRequested;
#if !OPENSILVER
            _controller.LockMainScrollViewerRequested += Controller_LockMainScrollViewerRequested;
            _controller.UnlockMainScrollViewerRequested += Controller_UnlockMainScrollViewerRequested;
#endif

            // If we are not at Design Time:
            bool isInDesignMode = System.ComponentModel.DesignerProperties.IsInDesignTool;
            if (!isInDesignMode)
            {
#if OPENSILVER && DEBUG
                // Mock data for testing:
                //_controller.Model = CreateMockData();
#endif
                _controller.CalendarService.CalendarModified += OnCalendarModified;
            }

            this.InvokeOnLayoutUpdated(async () => await RefreshAll());

            // Register other events:
            ButtonsOuterContainer.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ButtonsOuterContainer_MouseLeftButtonDown), true);

            if (_keyboardService != null)
            {
                _keyboardService.KeyboardStateChanged += OnKeyboardStateChanged;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo info)
        {
            base.OnRenderSizeChanged(info);

            var stateName = IsLandscapeMode ? "LandscapeState" : "DefaultState";
            VisualStateManager.GoToState(Parent as Control, stateName, false);
        }

        private static void ScrollIntoView(ScrollViewer viewer, FrameworkElement element, double verticalMargin)
        {
            if (element.GetBoundsRelativeTo(viewer) is not Rect itemRect)
                return;

            if (itemRect.Top - verticalMargin < 0 || itemRect.Bottom + verticalMargin > viewer.ViewportHeight)
            {
                double itemTop = itemRect.Top - verticalMargin;
                double verticalOffset = viewer.VerticalOffset + itemTop;
                viewer.ScrollToVerticalOffset(verticalOffset);
            }
        }

        private async Task LoadCalendarEvents(DateTime startDate, DateTime endDate)
        {
            var model = _controller.Model;

            try
            {
                endDate = endDate.AddDays(1); // workaround to load events from the last day
                var allEvents = await _controller.CalendarService.GetCalendarEvents(startDate, endDate);

                foreach (var dayItems in allEvents.GroupBy(x => x.DateTime.Date).OrderBy(x => x.Key))
                {
                    var date = dayItems.Key;
                    if (!model.Days.TryGetValue(date, out var dayModel))
                    {
                        model.Days[date] = dayModel = new DayModel();
                    }

                    foreach (var calendarEvent in dayItems)
                    {
                        if (dayModel.Events.Find(x => x.Id == calendarEvent.Id) == null)
                        {
                            dayModel.Events.Add(calendarEvent.ToEventModel());
                        }
                    }

                    _controller.RequestRefreshOfDay(date);
                    await Task.Delay(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show(ex.ToString(), "Cannot read calendar events");
            }
        }

        private async Task RefreshAll()
        {
            int dayCountBeforeCurrentDay = InitialDayCountBeforeScreen;
            int dayCountAfterCurrentDay;
            var maxItemsPerScreen = (int)Math.Ceiling(MainScrollViewer.ViewportHeight / DayItemHeight);

            if (IsLandscapeMode)
            {
                var maxColumns = (int)Math.Ceiling(MainScrollViewer.ViewportWidth / LandscapeColumnWidth);
                dayCountAfterCurrentDay = maxItemsPerScreen * maxColumns;
            }
            else
            {
                var numberOfCurrentDayOnScreen = (int)Math.Floor(InitialTodayTopOffset / DayItemHeight);
                dayCountBeforeCurrentDay += numberOfCurrentDayOnScreen;
                dayCountAfterCurrentDay = maxItemsPerScreen - numberOfCurrentDayOnScreen + InitialDayCountAfterScreen;
            }

            var today = DatesHelpers.GetTodayDateWithoutTime();
            _firstDayOfCalendar = today.AddDays(-dayCountBeforeCurrentDay);
            _lastDayOfCalendar = today.AddDays(dayCountAfterCurrentDay);

            RenderingHelpers.AddDaysToContainer(DaysContainer, _firstDayOfCalendar, _lastDayOfCalendar, _controller, renderEvents: false);

            // Set initial scroll offset:
            MainScrollViewer.ScrollToVerticalOffset(InitialTodayTopOffset);

            await LoadCalendarEvents(_firstDayOfCalendar, _lastDayOfCalendar);

            var todayItem = DaysContainer.Children.Cast<Border>()
                .First(x => x.Child is Panel panel && panel.Background == RenderingHelpers.BackgroundColorForToday);
            ScrollIntoView(MainScrollViewer, todayItem, InitialTodayTopOffset);
        }

        private async void ButtonLoadMoreDaysBefore_Click(object sender, RoutedEventArgs e)
        {
            var firstDayToAdd = _firstDayOfCalendar.AddDays(-NumberOfAdditionalDaysToLoadBefore);
            var lastDayToAdd = _firstDayOfCalendar.AddDays(-1);

            RenderingHelpers.AddDaysToContainer(DaysContainer, firstDayToAdd, lastDayToAdd, _controller, insertAtBeginningRatherThanAddToEnd: true);

            _firstDayOfCalendar = firstDayToAdd;

            await LoadCalendarEvents(_firstDayOfCalendar, lastDayToAdd);
        }

        private async void ButtonLoadMoreDaysAfter_Click(object sender, RoutedEventArgs e)
        {
            var firstDayToAdd = _lastDayOfCalendar.AddDays(1);
            var lastDayToAdd = _lastDayOfCalendar.AddDays(NumberOfAdditionalDaysToLoadAfter);

            RenderingHelpers.AddDaysToContainer(DaysContainer, firstDayToAdd, lastDayToAdd, _controller);

            _lastDayOfCalendar = lastDayToAdd;

            await LoadCalendarEvents(firstDayToAdd, _lastDayOfCalendar);
        }

        async void Controller_EditingModeStopped(object sender, EventArgs e)
        {
            EventOptionsControl.Visibility = Visibility.Collapsed;

            // Show again the button to add new events:
            ButtonsOuterContainer.Visibility = Visibility.Visible;

            try
            {
                // If the event has an empty title and was created recently (less than 3 minutes ago),
                // then delete it (because it was most likely created by mistake), otherwise save the new title:
                if (string.IsNullOrEmpty(EventOptionsControl.EventModel.Title)
                    && EventOptionsControl.EventModel.TemporaryCreationDate.HasValue
                    && EventOptionsControl.EventModel.TemporaryCreationDate.Value < DateTime.UtcNow
                    && (DateTime.UtcNow - EventOptionsControl.EventModel.TemporaryCreationDate.Value) < TimeSpan.FromMinutes(3))
                {
                    await _controller.DeleteEvent(EventOptionsControl.EventModel, EventOptionsControl.DayModel, EventOptionsControl.Day, false);
                }
                else
                {
                    if (EventOptionsControl.EventModel.Title != EventOptionsControl.PreviousTitle)
                    {
                        await _controller.CalendarService.UpdateCalendarEvent(new DeviceEvent(EventOptionsControl.EventModel, EventOptionsControl.Day));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Cannot update or delete the event");
            }
        }

        void Controller_EditingModeStarted(object sender, EditingModeStartedEventArgs e)
        {
            EventOptionsControl.Controller = _controller;
            EventOptionsControl.EventModel = e.EventModel;
            EventOptionsControl.PreviousTitle = e.EventModel.Title;
            EventOptionsControl.DayModel = e.DayModel;
            EventOptionsControl.Day = e.Day;
            EventOptionsControl.TextBox = e.TextBox;
            EventOptionsControl.MaxWidth = Math.Min(ActualWidth, MaxOptionsPopupWidth);
            EventOptionsControl.UpdateButtonsVisibility();
            EventOptionsControl.Visibility = Visibility.Visible;
            OptionsPopup.PlacementTarget = e.TextBox;
            OptionsPopup.IsOpen = true;

            // Hide the button to add new events:
            ButtonsOuterContainer.Visibility = Visibility.Collapsed;

            MainScrollViewer.ScrollIntoView(EventOptionsControl.TextBox, HorizontalScrollMargin, 0, TimeSpan.Zero);
        }

        void OnCalendarModified()
        {
            DisplayNotification(AppResources.Notification_Saved);
        }

        void DisplayNotification(string text)
        {
            NotificationControl.Text = text;
            NotificationControl.Visibility = System.Windows.Visibility.Visible;
            NotificationControl.NotificationCompleted -= NotificationControl_NotificationCompleted;
            NotificationControl.NotificationCompleted += NotificationControl_NotificationCompleted;
            NotificationControl.Show();
        }

        void NotificationControl_NotificationCompleted(object sender, EventArgs e)
        {
            NotificationControl.Visibility = System.Windows.Visibility.Collapsed;
        }

#if !OPENSILVER
        private void ButtonForOptions_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PopupForOptions.IsOpen = !PopupForOptions.IsOpen;
        }

        private void ButtonClosePopupForOptions_Click(object sender, RoutedEventArgs e)
        {
            PopupForOptions.IsOpen = false;
        }
#endif

        void Controller_QuitEditingModeRequested(object sender, EventArgs e)
        {
            // By removing the focus of the TextBox, we quit the editing mode:
            MetroHelpers.SetFocus(this);
        }

        void DragAndDropSource_Click(object sender, EventArgs e)
        {
            ExplanationOverlay.Visibility = System.Windows.Visibility.Visible;
            //MessageBox.Show("Drag-and-drop the + sign to the calendar.");
        }

        void ExplanationOverlay_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ExplanationOverlay.Visibility = System.Windows.Visibility.Collapsed;
        }

        void ButtonsOuterContainer_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ExplanationOverlay.Visibility = System.Windows.Visibility.Collapsed;
        }

        void ButtonBuyFullVersion_Click(object sender, EventArgs e)
        {
            TrialHelpers.BuyFullVersion();
        }

        private void OnMainScrollViewerLayoutUpdated(object sender, EventArgs e)
        {
            if (EventOptionsControl.Visibility == Visibility.Visible && EventOptionsControl.TextBox is FrameworkElement element)
            {
                MainScrollViewer.ScrollIntoView(element, HorizontalScrollMargin, 0, TimeSpan.Zero);
            }
        }

        private void OnMainScrollViewerMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (MainScrollViewer.ComputedHorizontalScrollBarVisibility != Visibility.Visible || e.Delta == 0)
                return;

            MainScrollViewer.ScrollToHorizontalOffset(MainScrollViewer.HorizontalOffset - e.Delta);
        }

        private void OnKeyboardStateChanged(bool isOpen)
        {
            // hide the editing popup if the virtual keyboard is closed 
            if (!isOpen && EventOptionsControl.Visibility == Visibility.Visible)
            {
                Dispatcher.BeginInvoke(Focus);
            }
        }

#if !OPENSILVER
        void Controller_LockMainScrollViewerRequested(object sender, EventArgs e)
        {
            _isMainScrollViewerLocked = true;
        }

        void Controller_UnlockMainScrollViewerRequested(object sender, EventArgs e)
        {
            _isMainScrollViewerLocked = false;
        }

        private void MainContainer_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            // This lets us temporary lock the ScrollViewer without affecting the current HorizontalOffset and VertialOffset.
            // NB: on WP8, the "ManipulationMode" must be set to "Control".
            // cf. http://social.msdn.microsoft.com/Forums/wpapps/en-US/c368fbeb-5cd0-4261-99a4-0610af2fbcea/temporary-lock-scrollviewer?forum=wpdevelop // This uses "ManipulationStarted" event, but it is too early for us because "_isMainScrollViewerLocked" has not been set yet. Next link applies better to our case.
            // cf. http://social.msdn.microsoft.com/Forums/wpapps/en-US/90e8607e-ae2f-4e5f-a60a-138826654518/wp8-prevent-scrollviewer-from-scrolling-at-desired-location?forum=wpdevelop

            if (_isMainScrollViewerLocked)
            {
                e.Handled = true;
                e.Complete();
            }
        }
#endif

#if OPENSILVER
        private static Model CreateMockData()
        {
            // Initialize the Model
            var model = new Model();

            // Create some dates for the events
            DateTime[] dates = new DateTime[]
            {
                new DateTime(2025, 2, 1),
                new DateTime(2025, 2, 2),
                new DateTime(2025, 2, 3),
                new DateTime(2025, 2, 4),
                new DateTime(2025, 2, 5),
                new DateTime(2025, 2, 6),
                new DateTime(2025, 2, 7),
                new DateTime(2025, 2, 8),
                new DateTime(2025, 2, 9),
                new DateTime(2025, 2, 10)
            };

            // Create some mock events
            for (int i = 0; i < 10; i++)
            {
                var eventModel = new EventModel
                {
                    Title = $"Event {i + 1}",
                    IsMarkedAsDone = (i % 2 == 0), // Mark every second event as done
                    EventType = (EventType)(i % 5) // Cycle through EventType enum values
                };

                var day = dates[i];

                if (!model.Days.ContainsKey(day))
                {
                    model.Days[day] = new DayModel();
                }

                model.Days[day].Events.Add(eventModel);
            }

            return model;
        }
#endif
    }
}
