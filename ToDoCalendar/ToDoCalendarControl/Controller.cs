using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using ToDoCalendarControl.Services;

namespace ToDoCalendarControl
{
    public class Controller
    {
        bool _containsUnsavedChanges = false;

        public Model Model { get; set; }

        public event EventHandler<RefreshDayRequestedEventArgs> RefreshDayRequested;
        public event EventHandler<EditEventRequestedEventArgs> EditEventRequested;
        public event EventHandler<EditingModeStartedEventArgs> EditingModeStarted;
        public event EventHandler EditingModeStopped;
        public event EventHandler ChangesWereMade;
        public event EventHandler QuitEditingModeRequested;
        public event EventHandler LockMainScrollViewerRequested;
        public event EventHandler UnlockMainScrollViewerRequested;

        public ICalendarService CalendarService { get; } = ServiceLocator.Provider?.GetRequiredService<ICalendarService>();

        public Controller()
        {
            Model = new Model();
        }

        public void RememberThatThereAreUnsavedChanges()
        {
            _containsUnsavedChanges = true;

            if (ChangesWereMade != null)
                ChangesWereMade(this, new EventArgs());
        }

        public void RememberThatChangesWereSaved()
        {
            _containsUnsavedChanges = false;
        }

        public bool ContainsUnsavedChanges
        {
            get
            {
                return _containsUnsavedChanges;
            }
        }

        public async Task AddEvent(DateTime day)
        {
            // Create or reuse the model for the day:
            DayModel dayModel;
            if (Model.Days.ContainsKey(day))
                dayModel = Model.Days[day];
            else
            {
                dayModel = new DayModel();
                Model.Days.Add(day, dayModel);
            }

            // Create the model for the event:
            var newEventModel = new EventModel { EventType = EventType.Normal };
            var deviceEvent = new DeviceEvent(newEventModel, day);
            newEventModel.Id = await CalendarService?.CreateCalendarEvent(deviceEvent);
            newEventModel.CalendarColor = deviceEvent.CalendarColor;

            dayModel.Events.Add(newEventModel);

            // Refresh the day:
            RequestRefreshOfDay(day, true);

            // Remember that there are unsaved changes:
            RememberThatThereAreUnsavedChanges();
        }

        public async Task DeleteEvent(EventModel eventModel, DayModel dayModel, DateTime day)
        {
            await CalendarService?.DeleteCalendarEvent(eventModel.Id);

            // Delete the event from the model:
            dayModel.Events.Remove(eventModel);

            // Clean up the model (delete a day if it has no more events):
            if (dayModel.CanBeCleanedUp)
            {
                if (Model.Days.ContainsKey(day))
                    Model.Days.Remove(day);
            }

            // Refresh the day:
            RequestRefreshOfDay(day);

            // Remember that there are unsaved changes:
            RememberThatThereAreUnsavedChanges();
        }

        public async Task MoveEvent(EventModel eventModel, DayModel previousDayModel, DateTime previousDay, DateTime newDay)
        {
            if (previousDay != newDay)
            {
                // Create or reuse the model for the destination day:
                DayModel dayModel;
                if (Model.Days.ContainsKey(newDay))
                    dayModel = Model.Days[newDay];
                else
                {
                    dayModel = new DayModel();
                    Model.Days.Add(newDay, dayModel);
                }

                // Add the event model to the destination daymodel:
                dayModel.Events.Add(eventModel); //Note: the event model is supposed to NOT be already on the list because we checked "previousDay != newDay" above.

                // Delete the event from the previous daymodel:
                previousDayModel.Events.Remove(eventModel);

                // Clean up the model (delete a day if it has no more events):
                if (previousDayModel.CanBeCleanedUp)
                {
                    if (Model.Days.ContainsKey(previousDay))
                        Model.Days.Remove(previousDay);
                }

                // Refresh the days concerned by the change:
                RequestRefreshOfDay(newDay);
                RequestRefreshOfDay(previousDay);

                // Remember that there are unsaved changes:
                RememberThatThereAreUnsavedChanges();

                await CalendarService?.UpdateCalendarEvent(new DeviceEvent(eventModel, newDay));
            }
        }

        public void EditEvent(EventModel eventModel, DayModel dayModel, DateTime day)
        {
            if (EditEventRequested != null)
                EditEventRequested(this, new EditEventRequestedEventArgs(eventModel, dayModel, day));
        }

        public void RequestRefreshOfDay(DateTime day, bool setFocusToLastEventOfTheDay = false)
        {
            if (RefreshDayRequested != null)
                RefreshDayRequested(this, new RefreshDayRequestedEventArgs(day, setFocusToLastEventOfTheDay: setFocusToLastEventOfTheDay));
        }

        public void SignalThatEditingModeStarted(EditingModeStartedEventArgs args)
        {
            if (EditingModeStarted != null)
                EditingModeStarted(this, args);
        }

        public void SignalThatEditingModeStopped()
        {
            if (EditingModeStopped != null)
                EditingModeStopped(this, new EventArgs());
        }

        public void QuitEditingMode()
        {
            if (QuitEditingModeRequested != null)
                QuitEditingModeRequested(this, new EventArgs());
        }

        public void LockMainScrollViewer()
        {
            if (LockMainScrollViewerRequested != null)
                LockMainScrollViewerRequested(this, new EventArgs());
        }

        public void UnlockMainScrollViewer()
        {
            if (UnlockMainScrollViewerRequested != null)
                UnlockMainScrollViewerRequested(this, new EventArgs());
        }
    }
}
