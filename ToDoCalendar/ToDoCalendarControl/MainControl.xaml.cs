﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;
using MetroStyleApps;
using System.Windows.Media.Animation;
using System.Windows.Input;
using ToDoCalendarControl.Resources;

namespace ToDoCalendarControl
{
    public partial class MainControl : UserControl
    {
        const int AutoSaveIntervalInSeconds = 2;
        const int InitialDayCountBeforeCurrentDate = 40;
        const int InitialDayCountAfterCurrentDate = 140;
        const int NumberOfAdditionalDaysToLoadBefore = 60;
        const int NumberOfAdditionalDaysToLoadAfter = 200;

        const double InitialVerticalScrollOffset = 800;

        Controller _controller;
        AutoSaveHandler _autoSaveHandler;
        DateTime _firstDayOfCalendar;
        DateTime _lastDayOfCalendar;
        bool _isMainScrollViewerLocked;

        public MainControl()
        {
            InitializeComponent();

            // If we are in Trial mode, show the alert:
            TrialAlert.Visibility = (TrialHelpers.IsTrial() ? Visibility.Visible : Visibility.Collapsed);

            // Prepare the controller:
            _controller = new Controller();
            _controller.ChangesWereMade += Controller_ChangesWereMade;
            _controller.EditingModeStarted += Controller_EditingModeStarted;
            _controller.EditingModeStopped += Controller_EditingModeStopped;
            _controller.QuitEditingModeRequested += Controller_QuitEditingModeRequested;
            _controller.LockMainScrollViewerRequested += Controller_LockMainScrollViewerRequested;
            _controller.UnlockMainScrollViewerRequested += Controller_UnlockMainScrollViewerRequested;

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
                _autoSaveHandler.AutoSaveTookPlace += AutoSaveHandler_AutoSaveTookPlace;
                _autoSaveHandler.Start();
            }

            // Render the calendar:
            RefreshAll();

            // Register the "Loaded" event, which occurs after the controls have been added to the Visual Tree:
            this.Loaded += MainControl_Loaded;

            // Register other events:
            ButtonsOuterContainer.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ButtonsOuterContainer_MouseLeftButtonDown), true);
        }

        void MainControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Set initial scroll offset:
            Dispatcher.BeginInvoke(() =>
            {
                MainScrollViewer.ScrollToVerticalOffset(InitialVerticalScrollOffset);
            });
        }

        void RefreshAll()
        {
            DaysContainer.Children.Clear();

            var today = DatesHelpers.GetTodayDateWithoutTime();
            _firstDayOfCalendar = today.AddDays(-InitialDayCountBeforeCurrentDate);
            _lastDayOfCalendar = today.AddDays(InitialDayCountAfterCurrentDate);

            RenderingHelpers.AddDaysToContainer(DaysContainer, _firstDayOfCalendar, _lastDayOfCalendar, _controller);
        }

        private void ButtonLoadMoreDaysBefore_Click(object sender, RoutedEventArgs e)
        {
            var firstDayToAdd = _firstDayOfCalendar.AddDays(-NumberOfAdditionalDaysToLoadBefore);
            var lastDayToAdd = _firstDayOfCalendar.AddDays(-1);

            RenderingHelpers.AddDaysToContainer(DaysContainer, firstDayToAdd, lastDayToAdd, _controller, insertAtBeginningRatherThanAddToEnd: true);

            _firstDayOfCalendar = firstDayToAdd;
        }

        private void ButtonLoadMoreDaysAfter_Click(object sender, RoutedEventArgs e)
        {
            var firstDayToAdd = _lastDayOfCalendar.AddDays(1);
            var lastDayToAdd = _lastDayOfCalendar.AddDays(NumberOfAdditionalDaysToLoadAfter);

            RenderingHelpers.AddDaysToContainer(DaysContainer, firstDayToAdd, lastDayToAdd, _controller);

            _lastDayOfCalendar = lastDayToAdd;
        }

        void Controller_EditingModeStopped(object sender, EventArgs e)
        {
            EventOptionsControl.Visibility = Visibility.Collapsed;

            // Show again the button to add new events:
            ButtonsOuterContainer.Visibility = Visibility.Visible;
        }

        void Controller_EditingModeStarted(object sender, EditingModeStartedEventArgs e)
        {
            // Position and show the options just below the control for editing the event:
            var generalTransform = e.TextBox.TransformToVisual(MainContainer);
            Point coordinates = MetroHelpers.TransformPoint(generalTransform, new Point());
            Dispatcher.BeginInvoke(() =>
                {
                    // We do the positioning with the Dispatcher so that "ActualHeight" is not NaN.
                    EventOptionsControl.RenderTransform = new TranslateTransform()
                    {
                        Y = coordinates.Y - (!double.IsNaN(EventOptionsControl.ActualHeight) ? EventOptionsControl.ActualHeight : 0d) // + (!double.IsNaN(e.ControlThatShowsTheEvent.ActualHeight) ? e.ControlThatShowsTheEvent.ActualHeight : 0d)
                    };
                });
            EventOptionsControl.Controller = _controller;
            EventOptionsControl.EventModel = e.EventModel;
            EventOptionsControl.DayModel = e.DayModel;
            EventOptionsControl.Day = e.Day;
            EventOptionsControl.TextBox = e.TextBox;
            EventOptionsControl.UpdateButtonsVisibility();
            EventOptionsControl.Visibility = Visibility.Visible;

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

        private void ButtonSendBackupByEmail_Click(object sender, RoutedEventArgs e)
        {
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
        }

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

        private void ButtonStartImportingFromBackup_Click(object sender, RoutedEventArgs e)
        {
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
        }

        private void CheckBoxForDisablingAutoScreenOff_Click(object sender, RoutedEventArgs e)
        {
            bool isScreenAutoOffDisabled = CheckBoxForDisablingAutoScreenOff.IsChecked.HasValue ? CheckBoxForDisablingAutoScreenOff.IsChecked.Value : false;

            ScreenAutoOffHelpers.SaveScreenAutoOffSetting(isScreenAutoOffDisabled);
            ScreenAutoOffHelpers.ApplyScreenAutoOffSetting();
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
            // Bug fix to keep the display refreshing while the user is dragging the button to add an event:
            var storyboard = (Storyboard)this.Resources["BugFixToKeepDisplayRefreshingWhileDragging"];
            storyboard.Begin();
        }

        void DragAndDropSource_DragAndDropStopped(object sender, EventArgs e)
        {
            // Bug fix to keep the display refreshing while the user is dragging the button to add an event:
            var storyboard = (Storyboard)this.Resources["BugFixToKeepDisplayRefreshingWhileDragging"];
            storyboard.Stop();
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
    }
}
