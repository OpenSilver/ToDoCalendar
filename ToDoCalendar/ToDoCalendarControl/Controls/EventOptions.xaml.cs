using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ToDoCalendarControl.Services;

namespace ToDoCalendarControl
{
    public partial class EventOptions : UserControl
    {
        public Controller Controller { get; set; }
        public DayModel DayModel { get; set; }
        public DateTime Day { get; set; }
        public TextBox TextBox { get; set; }

        private EventModel _eventModel;
        public EventModel EventModel
        {
            get { return _eventModel; }
            set { _eventModel = value; ApplyEventTypeToRadioButtons(); }
        }

        public string PreviousTitle { get; set; }

        public EventOptions()
        {
            InitializeComponent();

            ButtonDelete.Click += ButtonDelete_Click;
            ButtonMarkAsDone.Click += ButtonMarkAsDone_Click;
            ButtonMarkAsNotDone.Click += ButtonMarkAsNotDone_Click;
            ButtonOK.Click += ButtonOK_Click;

            Application.Current.MainWindow.SizeChanged += OnMainWindowSizeChanged;
        }

        private void OnMainWindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            MaxWidth = Math.Min(e.Size.Width, 450);
        }

        void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (Controller != null)
                Controller.QuitEditingMode();
        }

        async void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DayModel != null && EventModel != null && Controller != null && Day != default(DateTime))
            {
                await Controller.DeleteEvent(EventModel, DayModel, Day);
            }
        }

        async void ButtonMarkAsNotDone_Click(object sender, RoutedEventArgs e)
        {
            await MarkAsDoneOrUndone(false);
        }

        async void ButtonMarkAsDone_Click(object sender, RoutedEventArgs e)
        {
            await MarkAsDoneOrUndone(true);
        }

        async Task MarkAsDoneOrUndone(bool isDone)
        {
            if (EventModel != null && Controller != null)
            {
                EventModel.IsMarkedAsDone = isDone;
                Controller.RequestRefreshOfDay(Day);
                Controller.RememberThatThereAreUnsavedChanges();

                if (Controller.CalendarService is ICalendarService calendarService)
                {
                    await calendarService.UpdateCalendarEvent(new DeviceEvent(EventModel, Day));
                }
            }
            UpdateButtonsVisibility();
        }

        public void UpdateButtonsVisibility()
        {
            ButtonMarkAsDone.Visibility = (EventModel != null && !string.IsNullOrEmpty(PreviousTitle) && !EventModel.IsMarkedAsDone ? Visibility.Visible : Visibility.Collapsed);
            ButtonMarkAsNotDone.Visibility = (EventModel != null && !string.IsNullOrEmpty(PreviousTitle) && EventModel.IsMarkedAsDone ? Visibility.Visible : Visibility.Collapsed);
        }

        void ApplyEventTypeToRadioButtons()
        {
            if (EventModel != null)
            {
                var eventType = EventModel.EventType;
                RadioButtonNormal.IsChecked = (eventType == EventType.Normal);
                RadioButtonHighPriority.IsChecked = (eventType == EventType.HighPriority);
                RadioButtonLowPriority.IsChecked = (eventType == EventType.LowPriority);
                RadioButtonInfo.IsChecked = (eventType == EventType.Info);
            }
        }

        async Task ApplyRadioButtonsToEventType()
        {
            var newEventType = EventType.Normal;
            if (RadioButtonNormal.IsChecked == true)
                newEventType = EventType.Normal;
            else if (RadioButtonHighPriority.IsChecked == true)
                newEventType = EventType.HighPriority;
            else if (RadioButtonLowPriority.IsChecked == true)
                newEventType = EventType.LowPriority;
            else if (RadioButtonInfo.IsChecked == true)
                newEventType = EventType.Info;

            if (EventModel != null)
            {
                EventModel.EventType = newEventType;

                if (Controller.CalendarService is ICalendarService calendarService)
                {
                    await calendarService.UpdateCalendarEvent(new DeviceEvent(EventModel, Day));
                }
            }
        }

        private async void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            await ApplyRadioButtonsToEventType();
            if (Controller != null)
            {
                Controller.RequestRefreshOfDay(Day);
                Controller.RememberThatThereAreUnsavedChanges();
            }

            // Make sure the textbox remains focused:
            Controller.EditEvent(EventModel, DayModel, Day);
        }
    }
}
