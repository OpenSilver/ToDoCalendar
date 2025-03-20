using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
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
        private const int InitialDayCountBeforeScreen = 5;
        private const int InitialDayCountAfterScreen = 10;
        private const int NumberOfAdditionalDaysToLoadBefore = 30;
        private const int NumberOfAdditionalDaysToLoadAfter = 30;
        private const int NumberOfAdditionalDaysToLoadBeforeInLandscape = 100;
        private const int NumberOfAdditionalDaysToLoadAfterInLandscape = 150;
        private const double DayItemHeight = 16;
        private const double InitialTodayTopOffset = 150;
        private const int MinLandscapeWidth = 740;
        private const int HorizontalScrollMargin = 67;
        private const int MaxOptionsPopupWidth = 450;

        public const double LandscapeColumnWidth = 500;

        private readonly IKeyboardService _keyboardService;
        private readonly Controller _controller;

        private DateTime _firstDayOfCalendar;
        private DateTime _lastDayOfCalendar;

        private bool IsLandscapeMode => ActualWidth > MinLandscapeWidth;

        public MainControl()
        {
            InitializeComponent();

            // If we are not at Design Time:
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _keyboardService = ServiceLocator.Provider.GetService<IKeyboardService>();

                _controller = new Controller(new CalendarServiceSaver(ServiceLocator.Provider.GetRequiredService<ICalendarService>()));
                _controller.EditingModeStarted += Controller_EditingModeStarted;
                _controller.EditingModeStopped += Controller_EditingModeStopped;
                _controller.QuitEditingModeRequested += Controller_QuitEditingModeRequested;
                _controller.CalendarService.CalendarModified += OnCalendarModified;

                LayoutUpdated += OnLayoutUpdated;
            }

            // Register other events:
            ButtonsOuterContainer.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(ButtonsOuterContainer_MouseLeftButtonDown), true);

            if (_keyboardService != null)
            {
                _keyboardService.KeyboardStateChanged += OnKeyboardStateChanged;
            }

#if WINDOWS        
            OptionsPopup.AllowsTransparency = true;
#endif
        }

        private async void OnLayoutUpdated(object sender, EventArgs e)
        {
            LayoutUpdated -= OnLayoutUpdated;
            await RefreshAll();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo info)
        {
            base.OnRenderSizeChanged(info);

            var stateName = IsLandscapeMode ? "LandscapeState" : "DefaultState";
#if OPENSILVER
            var control = Parent as Control;
#elif WINDOWS
            var control = Parent as FrameworkElement;
#endif
            VisualStateManager.GoToState(control, stateName, false);
        }

        private static void ScrollIntoView(ScrollViewer viewer, FrameworkElement element, double verticalMargin)
        {
#if OPENSILVER
            if (element.GetBoundsRelativeTo(viewer) is not Rect itemRect)
                return;
#elif WINDOWS
            if (element.TransformToVisual(viewer) is not System.Windows.Media.GeneralTransform transform)
                return;
            var itemRect = transform.TransformBounds(new Rect(element.RenderSize));
#endif

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
            await Task.Delay(1);

            await LoadCalendarEvents(_firstDayOfCalendar, _lastDayOfCalendar);
            await Task.Delay(1);

            var todayItem = DaysContainer.Children.Cast<Border>()
                .First(x => x.Child is Panel panel && panel.Background == RenderingHelpers.BackgroundColorForToday);

            if (!IsLandscapeMode)
            {
                ScrollIntoView(MainScrollViewer, todayItem, InitialTodayTopOffset);
            }

            LoadPrevButtonTransformer.Visibility = Visibility.Visible;
            LoadNextButtonTransformer.Visibility = Visibility.Visible;
            ButtonsOuterContainer.Visibility = Visibility.Visible;

            await Task.Delay(1);
            LoadingIndicator.Visibility = Visibility.Collapsed;
        }

        private async void ButtonLoadMoreDaysBefore_Click(object sender, RoutedEventArgs e)
        {
            var firstDayToAdd = _firstDayOfCalendar.AddDays(IsLandscapeMode ? -NumberOfAdditionalDaysToLoadBeforeInLandscape : -NumberOfAdditionalDaysToLoadBefore);
            var lastDayToAdd = _firstDayOfCalendar.AddDays(-1);

            RenderingHelpers.AddDaysToContainer(DaysContainer, firstDayToAdd, lastDayToAdd, _controller, insertAtBeginningRatherThanAddToEnd: true);

            _firstDayOfCalendar = firstDayToAdd;

            await LoadCalendarEvents(_firstDayOfCalendar, lastDayToAdd);
        }

        private async void ButtonLoadMoreDaysAfter_Click(object sender, RoutedEventArgs e)
        {
            var firstDayToAdd = _lastDayOfCalendar.AddDays(1);
            var lastDayToAdd = _lastDayOfCalendar.AddDays(IsLandscapeMode ? NumberOfAdditionalDaysToLoadAfterInLandscape : NumberOfAdditionalDaysToLoadAfter);

            RenderingHelpers.AddDaysToContainer(DaysContainer, firstDayToAdd, lastDayToAdd, _controller);

            _lastDayOfCalendar = lastDayToAdd;

            await LoadCalendarEvents(firstDayToAdd, _lastDayOfCalendar);
        }

        private async void Controller_EditingModeStopped(object sender, EventArgs e)
        {
            EventOptionsControl.Visibility = Visibility.Collapsed;
            //AnimationHelper.HideWithAnimation(EventOptionsControl);

            EventOptionsControl.RemoveTypeToDoHint();

            if (!EventOptionsControl.EventModel.IsDeleted)
            {
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

            // Show again the button to add new events after a small delay to make sure the virtual keyboard is collapsed:
            await Task.Delay(1);
            ButtonsOuterContainer.Visibility = Visibility.Visible;
        }

        private void Controller_EditingModeStarted(object sender, EditingModeStartedEventArgs e)
        {
            var textBox = e.TextBox;
            EventOptionsControl.Controller = _controller;
            EventOptionsControl.EventModel = e.EventModel;
            EventOptionsControl.PreviousTitle = e.EventModel.Title;
            EventOptionsControl.DayModel = e.DayModel;
            EventOptionsControl.Day = e.Day;
            EventOptionsControl.TextBox = textBox;
            EventOptionsControl.MaxWidth = Math.Min(ActualWidth, MaxOptionsPopupWidth);
            EventOptionsControl.UpdateButtonsVisibility();
#if OPENSILVER
            AnimationHelper.ShowWithAnimation(EventOptionsControl);
#elif WINDOWS
            EventOptionsControl.Visibility = Visibility.Visible;
#endif
            OptionsPopup.PlacementTarget = textBox;
            OptionsPopup.IsOpen = true;

            // Hide the button to add new events:
            ButtonsOuterContainer.Visibility = Visibility.Collapsed;

            if (!IsLandscapeMode)
            {
#if OPENSILVER
                MainScrollViewer.ScrollIntoView(textBox, HorizontalScrollMargin, 0, TimeSpan.Zero);
#elif WINDOWS // todo
#endif
            }

            var isKeyboardAvailable = _keyboardService == null || _keyboardService.IsKeyboardVisible;
            EventOptionsControl.ShowTypeToDoHint(isKeyboardAvailable);
        }

        private void OnCalendarModified()
        {
            DisplayNotification(AppResources.Notification_Saved);
        }

        private void DisplayNotification(string text)
        {
            NotificationControl.Text = text;
            NotificationControl.Visibility = Visibility.Visible;
            NotificationControl.NotificationCompleted -= NotificationControl_NotificationCompleted;
            NotificationControl.NotificationCompleted += NotificationControl_NotificationCompleted;
            NotificationControl.Show();
        }

        private void NotificationControl_NotificationCompleted(object sender, EventArgs e)
        {
            NotificationControl.Visibility = Visibility.Collapsed;
        }

        private void Controller_QuitEditingModeRequested(object sender, EventArgs e)
        {
#if OPENSILVER
            // By removing the focus of the TextBox, we quit the editing mode:
            Focus();
#elif WINDOWS
            Controller_EditingModeStopped(sender, e);
#endif
        }

        private void DragAndDropSource_Click(object sender, EventArgs e)
        {
            ExplanationOverlay.Visibility = Visibility.Visible;
        }

        private void ExplanationOverlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ExplanationOverlay.Visibility = Visibility.Collapsed;
        }

        private void ButtonsOuterContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ExplanationOverlay.Visibility = Visibility.Collapsed;
        }

        private void OnMainScrollViewerLayoutUpdated(object sender, EventArgs e)
        {
            if (!IsLandscapeMode &&
                EventOptionsControl.Visibility == Visibility.Visible &&
                EventOptionsControl.TextBox is FrameworkElement element)
            {
#if OPENSILVER
                MainScrollViewer.ScrollIntoView(element, HorizontalScrollMargin, 0, TimeSpan.Zero);
#elif WINDOWS // todo
#endif
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

            if (isOpen)
            {
                Dispatcher.BeginInvoke(EventOptionsControl.UpdateTypeToDoHintIfVisible);
            }
        }
    }
}
