using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToDoCalendarControl
{
    public static class TrialHelpers
    {
        public static bool IsTrial_CachedValue;

        public static void UpdateIsTrialCache()
        {
            IsTrial_CachedValue = IsTrial();
        }

        public static bool IsTrial()
        {
#if TRIAL
            // return true if debugging with trial enabled.
            return true;
#else
            var license = new Microsoft.Phone.Marketplace.LicenseInformation();
            return license.IsTrial();
#endif
        }

        public static void BuyFullVersion()
        {
            var marketplaceDetailTask = new MarketplaceDetailTask();
            marketplaceDetailTask.Show();
        }
    }
}
