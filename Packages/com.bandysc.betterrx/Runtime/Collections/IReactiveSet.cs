using System.Collections.Generic;
using UniRx;

namespace BetterRx.Collections
{
    public interface IObservableSet<T>
    {
        System.IObservable<SetAddEvent<T>> ObserveAdd();
        System.IObservable<SetRemoveEvent<T>> ObserveRemove();
        System.IObservable<Unit> ObserveReset();
    }
    
    public interface IReactiveSet<T> : IObservableSet<T>, ICollection<T>
    {
        new bool Add(T item);
    }

    public struct SetAddEvent<T> : System.IEquatable<SetAddEvent<T>>
    {
        public T Value { get; private set; }

        public SetAddEvent(T value) : this()
        {
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("Value:{1}", Value);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Value) << 2;
        }

        public bool Equals(SetAddEvent<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }
    }

    public struct SetRemoveEvent<T> : System.IEquatable<SetRemoveEvent<T>>
    {
        public T Value { get; private set; }

        public SetRemoveEvent(T value)
            : this()
        {
            Value = value;
        }

        public override string ToString()
        {
            return string.Format("Value:{1}", Value);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Value) << 2;
        }

        public bool Equals(SetRemoveEvent<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }
    }
}