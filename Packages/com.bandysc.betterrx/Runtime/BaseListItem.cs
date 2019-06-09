using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BetterRx
{
    public abstract class BaseListItem<T> : MonoBehaviour where T : class
    {
        private List<IDisposable> bindings = new List<IDisposable>();
        
        protected T CurrentlyBound { get; private set; }

        public bool IsBound => CurrentlyBound != null;
    
        public void Bind(T t)
        {
            if (IsBound)
                throw new Exception("Trying to bind bound view");

            CurrentlyBound = t;
            OnBind(t);
        }

        public void Unbind()
        {
            if (IsBound)
                OnUnbind();
            CurrentlyBound = null;
            foreach (var binding in bindings)
                binding.Dispose();
            bindings.Clear();
        }

        public void Rebind(T t)
        {
            if (IsBound)
            {
                if (CurrentlyBound == t)
                    return;

                Unbind();
            }

            Bind(t);
        }

        protected void AddBinding(IDisposable binding)
        {
            bindings.Add(binding);
        }
        
        protected abstract void OnBind(T t);
        protected abstract void OnUnbind();
    }
}