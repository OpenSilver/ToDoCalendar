using MetroStyleApps;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToDoCalendarControl.Resources;
using ToDoCalendarControl.Services;

namespace ToDoCalendarControl
{
    public partial class MainControl : UserControl
    {
        const int AutoSaveIntervalInSeconds = 2;
        const int InitialDayCountBeforeCurrentDate = 10;
        const int InitialDayCountAfterCurrentDate = 80;
        const int NumberOfAdditionalDaysToLoadBefore = 30;
        const int NumberOfAdditionalDaysToLoadAfter = 120;
        const int ItemHeight = 16;

        Controller _controller;
        AutoSaveHandler _autoSaveHandler;
        DateTime _firstDayOfCalendar;
        DateTime _lastDayOfCalendar;
        bool _isMainScrollViewerLocked;

        private readonly ICalendarService _calendarService = ServiceLocator.Provider?.GetRequiredService<ICalendarService>();

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
                // Reload the model (if a save file exists):
                Model model;
                if (LoadAndSaveHelpers.TryLoadModelFromFileSystem(out model))
                {
                    _controller.Model = model;
                }

                // Initialize the Auto-Save handler:
                _autoSaveHandler = new AutoSaveHandler(AutoSaveIntervalInSeconds, () => _controller.Model, () => _controller.ContainsUnsavedChanges);
#if !OPENSILVER
                _autoSaveHandler.AutoSaveTookPlace += AutoSaveHandler_AutoSaveTookPlace;
                _autoSaveHandler.Start();
#endif
            }

#if OPENSILVER && DEBUG
            // Mock data for testing:
            _controller.Model = CreateMockData();
#endif

            // Render the calendar:
            RefreshAll();

            // Register the "Loaded" event, which occurs after the controls have been added to the Visual Tree:
            Loaded += MainControl_Loaded;

            // Register other events:
            ButtonsOuterContainer.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ButtonsOuterContainer_MouseLeftButtonDown), true);
        }

        async void MainControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Set initial scroll offset:
            MainScrollViewer.ScrollToVerticalOffset(InitialDayCountBeforeCurrentDate * ItemHeight);

            await LoadCalendarEvents(_firstDayOfCalendar, _lastDayOfCalendar);
        }

        private async Task LoadCalendarEvents(DateTime startDate, DateTime endDate)
        {
            if (_calendarService == null)
                return;

            var model = _controller.Model;

            try
            {
                await foreach (var item in _calendarService.GetCalendarEvents(startDate, endDate))
                {
                    var date = item.DateTime.Date;
                    if (!model.Days.ContainsKey(date))
                    {
                        model.Days[date] = new DayModel();
                    }

                    model.Days[date].Events.Add(new EventModel { Title = item.Title });
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

            RenderingHelpers.AddDaysToContainer(DaysContainer, _firstDayOfCalendar, _lastDayOfCalendar, _controller);
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

        void Controller_EditingModeStopped(object sender, EventArgs e)
        {
            EventOptionsControl.Visibility = Visibility.Collapsed;

            // Show again the button to add new events:
            ButtonsOuterContainer.Visibility = Visibility.Visible;
        }

        void Controller_EditingModeStarted(object sender, EditingModeStartedEventArgs e)
        {
            EventOptionsControl.Controller = _controller;
            EventOptionsControl.EventModel = e.EventModel;
            EventOptionsControl.DayModel = e.DayModel;
            EventOptionsControl.Day = e.Day;
            EventOptionsControl.TextBox = e.TextBox;
            EventOptionsControl.UpdateButtonsVisibility();
            EventOptionsControl.Visibility = Visibility.Visible;
            OptionsPopup.PlacementTarget = e.TextBox;
            OptionsPopup.IsOpen = true;

            // Hide the button to add new events:
            ButtonsOuterContainer.Visibility = Visibility.Collapsed;
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

            // Update the status of the checkbox(es):
            if (PopupForOptions.IsOpen)
            {
                CheckBoxForDisablingAutoScreenOff.IsChecked = ScreenAutoOffHelpers.GetScreenAutoOffSetting();
            }
        }

        private void ButtonClosePopupForOptions_Click(object sender, RoutedEventArgs e)
        {
            PopupForOptions.IsOpen = false;
        }
#endif

        private void ButtonSendBackupByEmail_Click(object sender, RoutedEventArgs e)
        {
#if !OPENSILVER
            try
            {
                var emailSubject = string.Format("Backup of calendar ({0:0000}-{1:00}-{2:00})", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                var emailBody = "To restore this backup, copy/paste the following code into the \"Import from backup\" field:\r\n\r\n\r\n\r\n" + SerializationHelpers.Serialize(_controller.Model);

                var emailComposeTask = new Microsoft.Phone.Tasks.EmailComposeTask();
                emailComposeTask.Subject = emailSubject;
                emailComposeTask.Body = emailBody;
                emailComposeTask.To = "";
                emailComposeTask.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
#else
            MessageBox.Show("This feature is currently unavailable");
#endif
        }

#if !OPENSILVER
        private void ButtonImportFromBackup_Click(object sender, RoutedEventArgs e)
        {
            TextBoxForImportingFromBackup.Text = string.Empty;
            PopupForOptions.IsOpen = false;
            PopupForImportingFromBackup.IsOpen = true;
        }

        private void ButtonClosePopupForImportingFromBackup_Click(object sender, RoutedEventArgs e)
        {
            PopupForImportingFromBackup.IsOpen = false;
        }
#endif

        private void ButtonStartImportingFromBackup_Click(object sender, RoutedEventArgs e)
        {
#if !OPENSILVER
            try
            {
                // Attempt to load the model from the backup:
                var model = LoadAndSaveHelpers.LoadModelFromTextAndRaiseExceptionIfError(TextBoxForImportingFromBackup.Text);

                // Replace the current model:
                _controller.Model = model;
                _controller.RememberThatThereAreUnsavedChanges();

                // Refresh all:
                RefreshAll();

                // Close the popup:
                PopupForImportingFromBackup.IsOpen = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
#else
            MessageBox.Show("This feature is currently unavailable.");
#endif
        }

        private void CheckBoxForDisablingAutoScreenOff_Click(object sender, RoutedEventArgs e)
        {
#if !OPENSILVER
            bool isScreenAutoOffDisabled = CheckBoxForDisablingAutoScreenOff.IsChecked.HasValue ? CheckBoxForDisablingAutoScreenOff.IsChecked.Value : false;

            ScreenAutoOffHelpers.SaveScreenAutoOffSetting(isScreenAutoOffDisabled);
            ScreenAutoOffHelpers.ApplyScreenAutoOffSetting();
#endif
        }

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

        void DragAndDropSource_DragAndDropStarted(object sender, EventArgs e)
        {
#if !OPENSILVER
            // Bug fix to keep the display refreshing while the user is dragging the button to add an event:
            var storyboard = (Storyboard)this.Resources["BugFixToKeepDisplayRefreshingWhileDragging"];
            storyboard.Begin();
#endif
        }

        void DragAndDropSource_DragAndDropStopped(object sender, EventArgs e)
        {
#if !OPENSILVER
            // Bug fix to keep the display refreshing while the user is dragging the button to add an event:
            var storyboard = (Storyboard)this.Resources["BugFixToKeepDisplayRefreshingWhileDragging"];
            storyboard.Stop();
#endif
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
                    EventType = (EventType)(i % 4) // Cycle through EventType enum values
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
