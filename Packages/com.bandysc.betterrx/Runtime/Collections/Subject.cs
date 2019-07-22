using System;
using UniRx;
using UniRx.InternalUtil;

namespace BetterRx.Collections
{
    public sealed class NotifySubject<T> : ISubject<T>, IDisposable
    {
        private readonly Action<IObserver<T>> onSubscribe;
        bool isStopped;
        bool isDisposed;
        Exception lastError;
        IObserver<T> outObserver = EmptyObserver<T>.Instance;

        private Subject<Unit> lastDisposed = new Subject<Unit>();
        public IObservable<Unit> LastDisposed => lastDisposed;

        public bool HasObservers => !(outObserver is EmptyObserver<T>) && !isStopped && !isDisposed;

        public NotifySubject(System.Action<IObserver<T>> onSubscribe)
        {
            this.onSubscribe = onSubscribe;
        }
        
        public void OnCompleted()
        {
            IObserver<T> old;
            ThrowIfDisposed();
            if (isStopped) return;

            old = outObserver;
            outObserver = EmptyObserver<T>.Instance;
            isStopped = true;
            old.OnCompleted();
        }

        public void OnError(Exception error)
        {
            if (error == null) throw new ArgumentNullException("error");

            IObserver<T> old;
            ThrowIfDisposed();
            if (isStopped) return;

            old = outObserver;
            outObserver = EmptyObserver<T>.Instance;
            isStopped = true;
            lastError = error;
            old.OnError(error);
        }

        public void OnNext(T value)
        {
            outObserver.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null) throw new ArgumentNullException("observer");

            var ex = default(Exception);

            ThrowIfDisposed();
            if (!isStopped)
            {
                var listObserver = outObserver as NotifyListObserver<T>;
                if (listObserver != null)
                {
                    outObserver = listObserver.Add(observer);
                }
                else
                {
                    var current = outObserver;
                    if (current is EmptyObserver<T>)
                    {
                        outObserver = observer;
                    }
                    else
                    {
                        outObserver = new NotifyListObserver<T>(new ImmutableList<IObserver<T>>(new[] { current, observer }));
                    }
                }

                onSubscribe?.Invoke(observer);
                
                return new Subscription(this, observer);
            }

            ex = lastError;

            
            if (ex != null)
            {
                observer.OnError(ex);
            }
            else
            {
                observer.OnCompleted();
            }

            return Disposable.Empty;
        }

        public void Dispose()
        {
            isDisposed = true;
            outObserver = DisposedObserver<T>.Instance;
        }

        void ThrowIfDisposed()
        {
            if (isDisposed) throw new ObjectDisposedException("");
        }

        class Subscription : IDisposable
        {
            NotifySubject<T> parent;
            IObserver<T> unsubscribeTarget;

            public Subscription(NotifySubject<T> parent, IObserver<T> unsubscribeTarget)
            {
                this.parent = parent;
                this.unsubscribeTarget = unsubscribeTarget;
            }

            public void Dispose()
            {
                if (parent != null)
                {
                    var listObserver = parent.outObserver as NotifyListObserver<T>;
                    if (listObserver != null)
                    {
                        parent.outObserver = listObserver.Remove(unsubscribeTarget);
                    }
                    else
                    {
                        parent.outObserver = EmptyObserver<T>.Instance;
                        parent.lastDisposed.OnNext(Unit.Default);
                    }

                    unsubscribeTarget = null;
                    parent = null;
                }
            }
        }
    }
    
    public class NotifyListObserver<T> : IObserver<T>
    {
        private readonly ImmutableList<IObserver<T>> _observers;

        public NotifyListObserver(ImmutableList<IObserver<T>> observers)
        {
            _observers = observers;
        }

        public void OnCompleted()
        {
            var targetObservers = _observers.Data;
            for (int i = 0; i < targetObservers.Length; i++)
            {
                targetObservers[i].OnCompleted();
            }
        }

        public void OnError(Exception error)
        {
            var targetObservers = _observers.Data;
            for (int i = 0; i < targetObservers.Length; i++)
            {
                targetObservers[i].OnError(error);
            }
        }

        public void OnNext(T value)
        {
            var targetObservers = _observers.Data;
            for (int i = 0; i < targetObservers.Length; i++)
            {
                targetObservers[i].OnNext(value);
            }
        }

        internal IObserver<T> Add(IObserver<T> observer)
        {
            return new ListObserver<T>(_observers.Add(observer));
        }

        internal IObserver<T> Remove(IObserver<T> observer)
        {
            var i = Array.IndexOf(_observers.Data, observer);
            if (i < 0)
                return this;

            if (_observers.Data.Length == 2)
            {
                return _observers.Data[1 - i];
            }
            else
            {
                return new ListObserver<T>(_observers.Remove(observer));
            }
        }
    }
}