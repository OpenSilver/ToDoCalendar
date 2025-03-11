using System;

namespace ToDoCalendarControl
{
    internal static class WeakEventsHelpers
    {
        public static EventHandler<EVENTARGSTYPE> MakeWeakHandler<EVENTARGSTYPE>(Action<EVENTARGSTYPE> action, Action<EventHandler<EVENTARGSTYPE>> remove)
            where EVENTARGSTYPE : EventArgs
        {
            var reference = new WeakReference(action.Target);
            var method = action.Method;

            void handler(object sender, EVENTARGSTYPE e)
            {
                var target = reference.Target;
                if (target != null)
                {
                    method.Invoke(target, [e]);
                }
                else
                {
                    remove(handler);
                }
            }

            return handler;
        }
    }
}
