using System;
using BetterRx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using ViewModels;

namespace Views
{
    public class CollectibleView : BaseListItem<CollectibleViewModel>
    {
        [SerializeField] private TagView tagView;
        
        [SerializeField] private Transform tagParent;
        
        [SerializeField] private Text titleText;

        [SerializeField] private Toggle toggle;
        
        [SerializeField] private Button addRandomTagButton;
        
        private IDisposable bindingCollection;
        
        protected override void OnBind(CollectibleViewModel t)
        {
            AddBinding(t.Title.Subscribe(v => titleText.text = v));
            AddBinding(toggle.BindChecked(t.IsSelected));
            AddBinding(t.AddRandomTag.BindTo(addRandomTagButton));
            bindingCollection = new CollectionBinder<TagViewModel>(t.Tags, tagView, tagParent);
        }

        protected override void OnUnbind()
        {
            bindingCollection.Dispose();
        }
    }
    
}
