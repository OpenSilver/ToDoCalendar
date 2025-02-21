using MetroStyleApps;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ToDoCalendarControl.Resources;
using ToDoCalendarControl.Services;

namespace ToDoCalendarControl
{
    public partial class MainControl : UserControl
    {
        const int AutoSaveIntervalInSeconds = 2;
        const int InitialDayCountBeforeCurrentDate = 20;
        const int InitialDayCountAfterCurrentDate = 150;
        const int NumberOfAdditionalDaysToLoadBefore = 100;
        const int NumberOfAdditionalDaysToLoadAfter = 150;
        const int ItemHeight = 14;
        const int InitialTodayTopOffset = 150;
        const int MinLandscapeWidth = 1150;

        Controller _controller;
        AutoSaveHandler _autoSaveHandler;
        DateTime _firstDayOfCalendar;
        DateTime _lastDayOfCalendar;
        bool _isMainScrollViewerLocked;

        public MainControl()
        {
            InitializeComponent();

            // Prepare the controller:
            _controller = new Controller();
            _controller.ChangesWereMade += Controller_ChangesWereMade;
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

                // Initialize the Auto-Save handler:
                _autoSaveHandler = new AutoSaveHandler(AutoSaveIntervalInSeconds, () => _controller.Model, () => _controller.ContainsUnsavedChanges);
                _autoSaveHandler.AutoSaveTookPlace += AutoSaveHandler_AutoSaveTookPlace;
                _autoSaveHandler.Start();
            }

            // Render the calendar:
            RefreshAll();

            // Register the "Loaded" event, which occurs after the controls have been added to the Visual Tree:
            Loaded += MainControl_Loaded;

            // Register other events:
            ButtonsOuterContainer.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ButtonsOuterContainer_MouseLeftButtonDown), true);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo info)
        {
            base.OnRenderSizeChanged(info);

            var stateName = ActualWidth > MinLandscapeWidth ? "LandscapeState" : "DefaultState";
            VisualStateManager.GoToState(Parent as Control, stateName, false);
        }

        async void MainControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Set initial scroll offset:
            MainScrollViewer.ScrollToVerticalOffset(InitialDayCountBeforeCurrentDate * ItemHeight);

            await LoadCalendarEvents(_firstDayOfCalendar, _lastDayOfCalendar);

            var todayItem = DaysContainer.Children.Cast<Border>()
                .First(x => x.Child is Panel panel && panel.Background == RenderingHelpers.BackgroundColorForToday);
            ScrollIntoView(MainScrollViewer, todayItem, InitialTodayTopOffset);
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
            if (_controller.CalendarService is not ICalendarService calendarService)
                return;

            var model = _controller.Model;

            try
            {
                endDate = endDate.AddDays(1); // workaround to load events from the last day
                var allEvents = await calendarService.GetCalendarEvents(startDate, endDate);

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

        void RefreshAll()
        {
            DaysContainer.Children.Clear();

            var today = DatesHelpers.GetTodayDateWithoutTime();
            _firstDayOfCalendar = today.AddDays(-InitialDayCountBeforeCurrentDate);
            _lastDayOfCalendar = today.AddDays(InitialDayCountAfterCurrentDate);

            RenderingHelpers.AddDaysToContainer(DaysContainer, _firstDayOfCalendar, _lastDayOfCalendar, _controller, renderEvents: false);
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
                // If the event has an empty title and was created recently (less than 3 minues ago),
                // then delete it (because it was most likely created by mistake), otherwise save the new title:
                if (string.IsNullOrEmpty(EventOptionsControl.EventModel.Title)
                    && (EventOptionsControl.EventModel.TemporaryCreationDate.HasValue
                    && EventOptionsControl.EventModel.TemporaryCreationDate.Value < DateTime.UtcNow
                    && (DateTime.UtcNow - EventOptionsControl.EventModel.TemporaryCreationDate.Value) < TimeSpan.FromMinutes(3)))
                {
                    await _controller.DeleteEvent(EventOptionsControl.EventModel, EventOptionsControl.DayModel, EventOptionsControl.Day);
                }
                else
                {
                    if (EventOptionsControl.EventModel.Title != EventOptionsControl.PreviousTitle &&
                        _controller.CalendarService is ICalendarService calendarService)
                    {
                        await calendarService.UpdateCalendarEvent(new DeviceEvent(EventOptionsControl.EventModel, EventOptionsControl.Day));
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
            EventOptionsControl.UpdateButtonsVisibility();
            EventOptionsControl.Visibility = Visibility.Visible;
            OptionsPopup.PlacementTarget = e.TextBox;
            OptionsPopup.IsOpen = true;

            // Hide the button to add new events:
            ButtonsOuterContainer.Visibility = Visibility.Collapsed;

            MainScrollViewer.ScrollIntoView(EventOptionsControl.TextBox);
        }

        void Controller_ChangesWereMade(object sender, EventArgs e)
        {
            // When changes are made, reset the timer of the AutoSave (this ensures that we don't AutoSave while the user is typing some text):
            _autoSaveHandler.PostponeAutoSave();
        }

        void AutoSaveHandler_AutoSaveTookPlace(object sender, EventArgs e)
        {
            DisplayNotification(AppResources.Notification_Saved);
            _controller.RememberThatChangesWereSaved();
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
                MainScrollViewer.ScrollIntoView(element);
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
