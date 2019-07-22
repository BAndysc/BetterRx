using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace BetterRx.Collections
{
    public class ReactiveSet<T> : IReactiveSet<T>
    {
        private bool isDisposed;
        private HashSet<T> elements = new HashSet<T>();
        
        public IEnumerator<T> GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Add(T item)
        {
            if (elements.Add(item))
            {
                setAdd?.OnNext(new SetAddEvent<T>(item));
                return true;
            }

            return false;
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void Clear()
        {
            elements.Clear();
            setClear?.OnNext(Unit.Default);
        }

        public bool Contains(T item)
        {
            return elements.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            elements.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (elements.Remove(item))
            {
                setRemove?.OnNext(new SetRemoveEvent<T>(item));
                return true;
            }

            return false;
        }

        public int Count => elements.Count;
        public bool IsReadOnly => false;

        NotifySubject<SetAddEvent<T>> setAdd = null;
        public IObservable<SetAddEvent<T>> ObserveAdd()
        {
            if (isDisposed) return Observable.Empty<SetAddEvent<T>>();
            return setAdd ?? (setAdd = new NotifySubject<SetAddEvent<T>>(observer =>
            {
                foreach (var el in this)
                    observer.OnNext(new SetAddEvent<T>(el));
            }));
        }
        
        Subject<SetRemoveEvent<T>> setRemove = null;
        public IObservable<SetRemoveEvent<T>> ObserveRemove ()
        {
            if (isDisposed) return Observable.Empty<SetRemoveEvent<T>>();
            return setRemove ?? (setRemove = new Subject<SetRemoveEvent<T>>());
        }
        
        Subject<Unit> setClear = null;
        public IObservable<Unit> ObserveReset ()
        {
            if (isDisposed) return Observable.Empty<Unit>();
            return setClear ?? (setClear = new Subject<Unit>());
        }
    }
}