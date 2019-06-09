using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BetterRx
{
    public class CollectionBinder<T> : IDisposable where T : class
    {
        private readonly IReadOnlyReactiveCollection<T> source;
        private readonly BaseListItem<T> prefab;
        private readonly Transform parent;
        private readonly IDisposable onAdd;
        private readonly IDisposable onMove;
        private readonly IDisposable onReset;
        private readonly IDisposable onRemove;
        private readonly IDisposable onReplace;
        
        private List<BaseListItem<T>> views = new List<BaseListItem<T>>();
        
        public CollectionBinder(IReadOnlyReactiveCollection<T> source, BaseListItem<T> prefab, Transform parent)
        {
            this.source = source;
            this.prefab = prefab;
            this.parent = parent;
            prefab.gameObject.SetActive(false);
            
            onAdd = source.ObserveAdd().Subscribe(@event => UpdateCollection());
            onMove = source.ObserveMove().Subscribe(@event => UpdateCollection());
            onReset = source.ObserveReset().Subscribe(@event => UpdateCollection());
            onRemove = source.ObserveRemove().Subscribe(@event => UpdateCollection());
            onReplace = source.ObserveReplace().Subscribe(@event => UpdateCollection());
            
            UpdateCollection();
        }

        private void UpdateCollection()
        {
            while (views.Count < source.Count)
            {
                var view = Object.Instantiate(prefab, parent);
                view.gameObject.SetActive(true);
                views.Add(view);
            }

            for (int i = source.Count; i < views.Count; ++i)
            {
                views[i].Unbind();
                views[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < source.Count; ++i)
            {
                views[i].Rebind(source[i]);
                views[i].gameObject.SetActive(true);
            }
        }

        public void Dispose()
        {
            onAdd.Dispose();
            onMove.Dispose();
            onReset.Dispose();
            onRemove.Dispose();
            onReplace.Dispose();

            foreach (var view in views)
                Object.Destroy(view.gameObject);
                
            views.Clear();
        }
    }
}