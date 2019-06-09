using System;
using System.Linq;
using UniRx;

namespace BetterRx
{
    public class SortedReactiveCollection<T> : ReactiveCollection<T>
    {
        private readonly Func<T, T, int> comparer;

        public SortedReactiveCollection(IReadOnlyReactiveCollection<T> source, System.Func<T, T, int> comparer, IObservable<Unit> observableToReact)
        {
            this.comparer = comparer;
            source.ObserveAdd().Subscribe(@event => { AddSorted(@event.Value); });
            source.ObserveRemove().Subscribe(@event => { Remove(@event.Value); });

            foreach (var el in source)
                AddSorted(el);
        }

        private void AddSorted(T el)
        {
            int i = 0;
            while (i < Count && comparer(this[i], el) == -1)
                i++;
            Insert(i, el);
        }
    }
}