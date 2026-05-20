using System;
using System.Collections.Generic;

namespace Common
{
    public interface IEventListner<T>
    {
        public void AddEvent(T type, Action<object> callback);
        public void RemoveEvent(T type, Action<object> callback);
        public void SendEvent(T type, object value);
    }


    /// <summary>
    /// Scene에 종속 된 EventListner
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="C"></typeparam>
    public class MonoEventListner<T, C> : MonoSceneSingleton<C>, IEventListner<T>
        where C : MonoEventListner<T, C>
    {
        
        protected Dictionary<T, Action<object>> EventCallBacks = new ();
        public void AddEvent(T type, Action<object> callback)
        {
            if (EventCallBacks.ContainsKey(type) == false)
            {
                EventCallBacks.Add(type, callback);
                return;
            }

            EventCallBacks[type] += callback;
        }

        public void RemoveEvent(T type, Action<object> callback)
        {
            if (EventCallBacks.ContainsKey(type) && EventCallBacks[type] != null)
                EventCallBacks[type] -= callback;
        }

        public void SendEvent(T type, object value)
        {
            if (EventCallBacks.ContainsKey(type) && EventCallBacks[type] != null)
                EventCallBacks[type](value);
        }
        
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventCallBacks.Clear();
        }
    }
}