using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace BetterRx.Collections
{
    public class FilterSet<T> : IObservableSet<T>
    {
        private readonly IObservableSet<T> super;
        private readonly System.Func<T, bool> predicate;

        public bool IsSubscribed => isSubscribed;
        
        public FilterSet(IObservableSet<T> super, System.Func<T, bool> predicate)
        {
            this.super = super;
            this.predicate = predicate;
        }

        private System.IDisposable superAdd;
        private System.IDisposable superRemove;
        private System.IDisposable superClear;
        private System.IDisposable lastDisposed;
        private System.IDisposable lastDisposed2;
        private System.IDisposable lastDisposed3;

        private bool isSubscribed;
        
        private HashSet<T> elements = new HashSet<T>();
        
        private void Subscribe()
        {
            isSubscribed = true;
            subjectAdd = new NotifySubject<SetAddEvent<T>>(observer =>
            {
                foreach (var el in elements)
                    observer.OnNext(new SetAddEvent<T>(el));
            });
            subjectRemove = new NotifySubject<SetRemoveEvent<T>>(null);
            subjectClear = new NotifySubject<Unit>(null);
            
            superAdd = super.ObserveAdd().Subscribe(@event =>
            {
                if (predicate(@event.Value))
                {
                    elements.Add(@event.Value);
                    subjectAdd.OnNext(@event);
                }
            });
            
            superRemove = super.ObserveRemove().Subscribe(@event =>
            {
                if (predicate(@event.Value))
                {
                    elements.Remove(@event.Value);
                    subjectRemove.OnNext(@event);
                }
            });
            
            superClear = super.ObserveReset().Subscribe(@event =>
            {
                elements.Clear();
                subjectClear.OnNext(@event);
            });

            lastDisposed = subjectAdd.LastDisposed.Subscribe(TryUnsubscribe);
            lastDisposed2 = subjectRemove.LastDisposed.Subscribe(TryUnsubscribe);
            lastDisposed3 = subjectClear.LastDisposed.Subscribe(TryUnsubscribe);
        }

        public void TryUnsubscribe(Unit _)
        {
            if (!subjectAdd.HasObservers &&
                !subjectRemove.HasObservers &&
                !subjectClear.HasObservers)
                Unsubscribe();
        }

        private void Unsubscribe()
        {
            elements.Clear();
            lastDisposed.Dispose();
            lastDisposed2.Dispose();
            lastDisposed3.Dispose();
            
            superAdd.Dispose();
            superRemove.Dispose();
            superClear.Dispose();

            isSubscribed = false;
        }

        private void EnsureSubscribed()
        {
            if (!isSubscribed)
                Subscribe();
        }

        private NotifySubject<SetAddEvent<T>> subjectAdd;
        public System.IObservable<SetAddEvent<T>> ObserveAdd()
        {
            EnsureSubscribed();
            return subjectAdd;
        }

        private NotifySubject<SetRemoveEvent<T>> subjectRemove;
        public System.IObservable<SetRemoveEvent<T>> ObserveRemove()
        {
            EnsureSubscribed();
            return subjectRemove;
        }

        private NotifySubject<Unit> subjectClear;
        public System.IObservable<Unit> ObserveReset()
        {
            EnsureSubscribed();
            return subjectClear;
        }
    }
}