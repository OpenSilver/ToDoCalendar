﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ToDoCalendarControl.Resources;

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
            get => _eventModel;
            set
            {
                _eventModel = value;
                ApplyEventTypeToRadioButtons();
            }
        }

        public string PreviousTitle { get; set; }

        public EventOptions()
        {
            InitializeComponent();

            ButtonDelete.Click += ButtonDelete_Click;
            ButtonMarkAsDone.Click += ButtonMarkAsDone_Click;
            ButtonMarkAsNotDone.Click += ButtonMarkAsNotDone_Click;
            ButtonOK.Click += ButtonOK_Click;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Controller?.QuitEditingMode();
        }

        private async void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DayModel != null && EventModel != null && Controller != null && Day != default(DateTime))
            {
                await Controller.DeleteEvent(EventModel, DayModel, Day);
            }
        }

        private async void ButtonMarkAsNotDone_Click(object sender, RoutedEventArgs e)
        {
            await MarkAsDoneOrUndone(false);
        }

        private async void ButtonMarkAsDone_Click(object sender, RoutedEventArgs e)
        {
            await MarkAsDoneOrUndone(true);
        }

        private async Task MarkAsDoneOrUndone(bool isDone)
        {
            if (EventModel != null && Controller != null)
            {
                EventModel.IsMarkedAsDone = isDone;
                Controller.RequestRefreshOfDay(Day);

                await Controller.CalendarService.UpdateCalendarEvent(new DeviceEvent(EventModel, Day));
            }
            UpdateButtonsVisibility();
        }

        public void UpdateButtonsVisibility()
        {
            ButtonMarkAsDone.Visibility = (EventModel != null && !string.IsNullOrEmpty(PreviousTitle) && !EventModel.IsMarkedAsDone ? Visibility.Visible : Visibility.Collapsed);
            ButtonMarkAsNotDone.Visibility = (EventModel != null && !string.IsNullOrEmpty(PreviousTitle) && EventModel.IsMarkedAsDone ? Visibility.Visible : Visibility.Collapsed);
        }

        private void ApplyEventTypeToRadioButtons()
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

        private async Task ApplyRadioButtonsToEventType()
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

                await Controller.CalendarService.UpdateCalendarEvent(new DeviceEvent(EventModel, Day));
            }
        }

        private async void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            await ApplyRadioButtonsToEventType();
            if (Controller != null)
            {
                Controller.RequestRefreshOfDay(Day);

                // Make sure the textbox remains focused:
                Controller.EditEvent(EventModel, DayModel, Day);
            }
        }

        internal void ShowTypeToDoHint(bool isKeyboardAvailable)
        {
            if (string.IsNullOrEmpty(PreviousTitle) && TextBox?.Parent is Panel panel)
            {
                var hint = new TextBlock
                {
                    Text = isKeyboardAvailable ? AppResources.TypeToDoHint : AppResources.ClickToTypeToDoHint,
                    IsHitTestVisible = false,
                    VerticalAlignment = VerticalAlignment.Center,
                    Opacity = 0.7,
                };
                panel.Children.Add(hint);
                TextBox.TextChanged += OnTextChanged;
            }
        }

        internal void UpdateTypeToDoHintIfVisible()
        {
            if (TextBox?.Parent is Panel panel &&
                panel.Children.LastOrDefault() is TextBlock hint)
            {
                hint.Text = AppResources.TypeToDoHint;
            }
        }

        internal void RemoveTypeToDoHint()
        {
            if (TextBox?.Parent is Panel panel &&
                panel.Children.LastOrDefault() is TextBlock hint)
            {
                TextBox.TextChanged -= OnTextChanged;
                panel.Children.Remove(hint);
            }
        }

        private void OnTextChanged(object s, TextChangedEventArgs e) => RemoveTypeToDoHint();
    }
}
