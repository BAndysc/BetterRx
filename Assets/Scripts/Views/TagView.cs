using System;
using BetterRx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using ViewModels;

namespace Views
{
    [Serializable]
    public class TagView : BaseListItem<TagViewModel>
    {
        [SerializeField] private RawImage background;
        [SerializeField] private Text text;
        [SerializeField] private Button button;


        protected override void OnBind(TagViewModel t)
        {
            background.color = t.Color;
            text.text = t.Name;
            AddBinding(t.RemoveTag.BindTo(button));
        }

        protected override void OnUnbind()
        {
            
        }
    }
}