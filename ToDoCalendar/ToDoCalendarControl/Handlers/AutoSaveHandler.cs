using System;

using System.Windows.Threading;

namespace ToDoCalendarControl
{
    class AutoSaveHandler
    {
        public const string FileNameForAutoSave = "AutoSave.xml";

        Func<object> _objectToSaveProvider;
        Func<bool> _predicateToKnowIfObjectContainsUnsavedChanges;
        DispatcherTimer _timerForSavingEveryFewSeconds;
        bool _isAutoSaving;
        bool _isTimerStarted;

        public event EventHandler AutoSaveTookPlace;

        public AutoSaveHandler(int autoSaveIntervalInSeconds, Func<object> objectToSaveProvider, Func<bool> predicateToKnowIfObjectContainsUnsavedChanges)
        {
            _objectToSaveProvider = objectToSaveProvider;
            _predicateToKnowIfObjectContainsUnsavedChanges = predicateToKnowIfObjectContainsUnsavedChanges;
            _timerForSavingEveryFewSeconds = new DispatcherTimer() { Interval = new TimeSpan(0, 0, autoSaveIntervalInSeconds) };
            _timerForSavingEveryFewSeconds.Tick += TimerForSavingEveryFewSeconds_Tick;
        }

        public void Start()
        {
            _timerForSavingEveryFewSeconds.Start();
            _isTimerStarted = true;
        }

        public void Stop()
        {
            _timerForSavingEveryFewSeconds.Stop();
            _isTimerStarted = false;
        }

        public void PostponeAutoSave()
        {
            // This method lets you reset the timer so that the remaining timer interval is reinitialized.
            if (_isTimerStarted)
            {
                _timerForSavingEveryFewSeconds.Stop();
                _timerForSavingEveryFewSeconds.Start();
            }
        }

        void TimerForSavingEveryFewSeconds_Tick(object sender, object e)
        {
            Save(_objectToSaveProvider());
        }

        void Save(object objectToSave)
        {
            if (!_isAutoSaving && !TrialHelpers.IsTrial_CachedValue)
            {
                _isAutoSaving = true;

                bool areThereUnsavedChanges = _predicateToKnowIfObjectContainsUnsavedChanges();

                if (areThereUnsavedChanges)
                {
                    // Serialize the object to save:
                    var objectAsString = SerializationHelpers.Serialize(objectToSave);

                    // Save the object to disk:
                    FileSystemHelpers.WriteTextToFile(FileNameForAutoSave, objectAsString);

                    // Raise event to signal that the auto-save took place:
                    if (AutoSaveTookPlace != null)
                        AutoSaveTookPlace(this, new EventArgs());
                }

                _isAutoSaving = false;
            }
        }
    }
}
