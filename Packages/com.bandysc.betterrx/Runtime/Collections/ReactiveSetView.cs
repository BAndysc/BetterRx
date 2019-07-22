using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace BetterRx.Collections
{
    public class ReactiveSetView<T> : IReadOnlyReactiveCollection<T>, IReadOnlyList<T>, System.IDisposable
    {
        private bool isDisposed;
        private List<T> backList = new List<T>();

        private System.IDisposable addDisposable;
        private System.IDisposable removeDisposable;
        private System.IDisposable clearDisposable;
        
        public ReactiveSetView(IObservableSet<T> source)
        {
            addDisposable = source.ObserveAdd().Subscribe(@event =>
            {
                backList.Add(@event.Value);
                collectionAdd?.OnNext(new CollectionAddEvent<T>(Count - 1, @event.Value));
            });
            
            removeDisposable = source.ObserveRemove().Subscribe(@event =>
            {
                backList.Remove(@event.Value);
                collectionRemove?.OnNext(new CollectionRemoveEvent<T>(Count - 1, @event.Value));
            });
            
            clearDisposable = source.ObserveReset().Subscribe(@event =>
            {
                collectionReset?.OnNext(Unit.Default);
            });
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            return backList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => backList.Count;

        public T this[int index] => backList[index];
        
        

        [NonSerialized]
        Subject<int> countChanged = null;
        public IObservable<int> ObserveCountChanged(bool notifyCurrentCount = false)
        {
            if (isDisposed) return Observable.Empty<int>();

            var subject = countChanged ?? (countChanged = new Subject<int>());
            if (notifyCurrentCount)
            {
                return subject.StartWith(() => this.Count);
            }
            else
            {
                return subject;
            }
        }

        [NonSerialized]
        Subject<Unit> collectionReset = null;
        public IObservable<Unit> ObserveReset()
        {
            if (isDisposed) return Observable.Empty<Unit>();
            return collectionReset ?? (collectionReset = new Subject<Unit>());
        }

        [NonSerialized]
        Subject<CollectionAddEvent<T>> collectionAdd = null;
        public IObservable<CollectionAddEvent<T>> ObserveAdd()
        {
            if (isDisposed) return Observable.Empty<CollectionAddEvent<T>>();
            return collectionAdd ?? (collectionAdd = new Subject<CollectionAddEvent<T>>());
        }

        [NonSerialized]
        Subject<CollectionRemoveEvent<T>> collectionRemove = null;
        public IObservable<CollectionRemoveEvent<T>> ObserveRemove()
        {
            if (isDisposed) return Observable.Empty<CollectionRemoveEvent<T>>();
            return collectionRemove ?? (collectionRemove = new Subject<CollectionRemoveEvent<T>>());
        }

        public IObservable<CollectionMoveEvent<T>> ObserveMove()
        {
            return Observable.Empty<CollectionMoveEvent<T>>();
        }

        public IObservable<CollectionReplaceEvent<T>> ObserveReplace()
        {
            return Observable.Empty<CollectionReplaceEvent<T>>();
        }

        public void Dispose()
        {
            addDisposable.Dispose();
            removeDisposable.Dispose();
            clearDisposable.Dispose();

            addDisposable = null;
            removeDisposable = null;
            clearDisposable = null;
        }
    }
}