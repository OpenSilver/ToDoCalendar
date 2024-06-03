#if !OPENSILVER
using Microsoft.Phone.Shell;
#endif
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;

namespace ToDoCalendarControl
{
    public static class ScreenAutoOffHelpers
    {
        const string KeyForIsScreenAutoOffSetting = "IsScreenAutoOffDisabled";

        public static void SaveScreenAutoOffSetting(bool isScreenAutoOffDisabled)
        {
            IsolatedStorageSettings.ApplicationSettings[KeyForIsScreenAutoOffSetting] = isScreenAutoOffDisabled;
        }

        public static bool GetScreenAutoOffSetting()
        {
            var isScreenAutoOffDisabled = (IsolatedStorageSettings.ApplicationSettings.Contains(KeyForIsScreenAutoOffSetting)
                ? (bool)IsolatedStorageSettings.ApplicationSettings[KeyForIsScreenAutoOffSetting]
                : false);

            return isScreenAutoOffDisabled;
        }

#if !OPENSILVER
        public static void ApplyScreenAutoOffSetting()
        {
            var isScreenAutoOffDisabled = GetScreenAutoOffSetting();
            PhoneApplicationService.Current.UserIdleDetectionMode = (isScreenAutoOffDisabled ? IdleDetectionMode.Disabled : IdleDetectionMode.Enabled);
        }
#endif
    }
}
