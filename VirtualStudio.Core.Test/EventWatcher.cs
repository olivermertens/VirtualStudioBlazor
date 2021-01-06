using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace VirtualStudio.Core.Test
{
    internal class EventWatcher
    {
        public static bool FiresEvent<TSource>(TSource obj, string eventName, Action eventTriggerAction)
        {
            var watcher = new EventWatcher();
            EventInfo eventInfo = typeof(TSource).GetEvent(eventName);

            var handleMethod = typeof(EventWatcher).GetMethod(nameof(watcher.HandleEvent), BindingFlags.NonPublic | BindingFlags.Instance);
            Delegate del = Delegate.CreateDelegate(eventInfo.EventHandlerType, watcher, handleMethod);

            eventInfo.AddEventHandler(obj, del);

            eventTriggerAction.Invoke();
            return watcher.fired;
        }

        bool fired = false;
        private void HandleEvent(object sender, EventArgs args)
        {
            fired = true;
        }
    }
}
