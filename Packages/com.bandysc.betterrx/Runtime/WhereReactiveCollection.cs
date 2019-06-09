using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace BetterRx
{
    public static class StaticExtensions
    {
        public static IReadOnlyReactiveCollection<T> Where<T>(this IReadOnlyReactiveCollection<T> source,
            System.Func<T, bool> filter, IObservable<Unit> filterChanged)
        {
            return new WhereReactiveCollection<T>(source, filter, filterChanged);
        }

        public static IReadOnlyReactiveCollection<T> WhereMany<T>(this IReadOnlyReactiveCollection<T> source,
            System.Func<T, bool> filter, Func<T, IObservable<Unit>> filterChanged, IObservable<Unit> generalFilterChanged = null)
        {
            return new WhereManyReactiveCollection<T>(source, filter, filterChanged, generalFilterChanged);
        }
        
        public static IReadOnlyReactiveCollection<T> Sort<T>(this IReadOnlyReactiveCollection<T> source,
            Func<T, T, int> comparer, IObservable<Unit> observable)
        {
            return new SortedReactiveCollection<T>(source, comparer, observable);
        }
    }
    
    public class WhereReactiveCollection<T> : ReactiveCollection<T>
    {
        private readonly Func<T, bool> filter;
        private readonly IDisposable onAdd;
        private readonly IDisposable onReset;
        private readonly IDisposable onRemove;
        private readonly IDisposable onReplace;
        
        public WhereReactiveCollection(IReadOnlyReactiveCollection<T> source, Func<T, bool> filter, IObservable<Unit> filterChanged)
        {
            this.filter = filter;
            onAdd = source.ObserveAdd().Subscribe(@event =>
            {
                if (filter(@event.Value))
                    Add(@event.Value);
            });
            
            onRemove = source.ObserveRemove().Subscribe(@event =>
            {
                Remove(@event.Value);
            });

            onReset = source.ObserveReset().Subscribe(_ => ClearItems());

            onReset = source.ObserveReplace().Subscribe(@event =>
            {
                Remove(@event.OldValue);
                
                if (filter(@event.NewValue))
                    Add(@event.NewValue);
            });

            filterChanged.Subscribe(_ =>
            {
                foreach (var element in source)
                {
                    if (filter(element))
                    {
                        if (!Contains(element))
                            Add(element);
                    }
                    else
                    {
                        Remove(element);
                    }
                }
            });
            
        }

        protected override void Dispose(bool disposing)
        {
            onAdd.Dispose();
            onReset.Dispose();
            onRemove.Dispose();
            onReplace.Dispose();
            base.Dispose(disposing);
        }
    }
    
    public class WhereManyReactiveCollection<T> : ReactiveCollection<T>
    {
        private readonly Func<T, bool> filter;
        private readonly Func<T, IObservable<Unit>> filterChanged;
        private readonly IDisposable onAdd;
        private readonly IDisposable onReset;
        private readonly IDisposable onRemove;
        private readonly IDisposable onReplace;
        
        private Dictionary<T, IDisposable> observables = new Dictionary<T, IDisposable>();

        private IDisposable generalFilterChangedBinding;
        
        public WhereManyReactiveCollection(IReadOnlyReactiveCollection<T> source, Func<T, bool> filter, Func<T, IObservable<Unit>> filterChanged, IObservable<Unit> generalFilterChanged = null)
        {
            this.filter = filter;
            this.filterChanged = filterChanged;
            onAdd = source.ObserveAdd().Subscribe(@event =>
            {
                OnAdd(@event.Value);
            });
            
            onRemove = source.ObserveRemove().Subscribe(@event => { OnRemove(@event.Value); });

            onReset = source.ObserveReset().Subscribe(_ => ClearItems());

            onReset = source.ObserveReplace().Subscribe(@event =>
            {
                OnRemove(@event.OldValue);
                OnAdd(@event.NewValue);
            });

            if (generalFilterChanged != null)
            {
                generalFilterChangedBinding = generalFilterChanged.Subscribe(_ =>
                {
                    foreach (var element in source)
                    {
                        UpdateElement(element);
                    }
                });
            }
            
            foreach (var el in source)
                OnAdd(el);
        }

        private bool inOnAdd = false;
        
        private void OnAdd(T element)
        {
            if (inOnAdd)
                return;
            
            inOnAdd = true;
            var observable = filterChanged(element).Subscribe(_ => UpdateElement(element));
            observables.Add(element, observable);

            if (filter(element))
                Add(element);
            inOnAdd = false;
        }

        private void OnRemove(T element)
        {
            if (!observables.ContainsKey(element))
                Debug.Log("bug");
            observables[element].Dispose();
            observables.Remove(element);
            Remove(element);     
        }

        private void UpdateElement(T element)
        {
            if (!observables.ContainsKey(element))
                return;
            
            if (filter(element))
            {
                if (!Contains(element))
                    Add(element);
            }
            else
            {
                Remove(element);
            }
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var disp in observables.Values)
                disp.Dispose();
            
            observables.Clear();
            
            onAdd.Dispose();
            onReset.Dispose();
            onRemove.Dispose();
            onReplace.Dispose();
            
            if (generalFilterChangedBinding != null)
                generalFilterChangedBinding.Dispose();
            
            base.Dispose(disposing);
        }
    }
}