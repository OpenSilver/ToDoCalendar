using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;


namespace ToDoCalendarControl
{
    static class WeakEventsHelpers
    {
        public static EventHandler<EVENTARGSTYPE> MakeWeakHandler<EVENTARGSTYPE>(Action<EVENTARGSTYPE> action, Action<EventHandler<EVENTARGSTYPE>> remove)
            where EVENTARGSTYPE : EventArgs
        {
            var reference = new WeakReference(action.Target);
#if SILVERLIGHT
            var method = action.Method;
#else
            var method = action.GetMethodInfo();
#endif
            EventHandler<EVENTARGSTYPE> handler = null;
            handler = delegate(object sender, EVENTARGSTYPE e)
            {
                var target = reference.Target;
                if (target != null)
                {
                    method.Invoke(target, new object[] { e });
                }
                else
                {
                    remove(handler);
                }
            };
            return handler;
        }


//        public static Controller.RefreshDayRequestedEventHandler MakeWeakHandlerForRefreshDayRequestedEvent(Action<RefreshDayRequestedEventArgs> action, Action<Controller.RefreshDayRequestedEventHandler> remove)
//        {
//            var reference = new WeakReference(action.Target);
//#if SILVERLIGHT
//            var method = action.Method;
//#else
//            var method = action.GetMethodInfo();
//#endif
//            Controller.RefreshDayRequestedEventHandler handler = null;
//            handler = delegate(object sender, RefreshDayRequestedEventArgs e)
//            {
//                var target = reference.Target;
//                if (target != null)
//                {
//                    method.Invoke(target, new object[] { e });
//                }
//                else
//                {
//                    remove(handler);
//                }
//            };
//            return handler;
//        }
    }
}
