
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BetterRx;
using Models;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ViewModels
{
    public class CollectiblesViewModel
    {
        public IReadOnlyReactiveCollection<CollectibleViewModel> AllItems { get; }

        public IReadOnlyReactiveCollection<CollectibleViewModel> Items { get; }

        public IReadOnlyReactiveCollection<CollectibleViewModel> SelectedItems { get; }

        public IReactiveProperty<string> SearchText { get; }
        
        public CollectiblesViewModel(CollectiblesList list, TagsList tags)
        {
            SearchText = new ReactiveProperty<string>("");
            
            AllItems = list.Collectibles.Select(item => new CollectibleViewModel(item, SearchText, tags.Tags)).ToReactiveCollection();

            Func<CollectibleViewModel, CollectibleViewModel, int> sortByTitle = (a, b) => String.Compare(a.OriginalTitle, b.OriginalTitle);

            Func<CollectibleViewModel, bool> filter = vm =>
            {
                if (string.IsNullOrEmpty(SearchText.Value))
                    return true;

                if (SearchText.Value.StartsWith("t:"))
                {
                    var tag = SearchText.Value.Substring(2).ToLower();

                    return vm.Tags.Any(t => t.Name.ToLower().Contains(tag));
                }

                if (vm.OriginalTitle.ToLower().Contains(SearchText.Value.ToLower()))
                    return true;

                return false;
            };
            
            Items = AllItems.WhereMany(filter, t => t.Tags.ObserveCountChanged().AsUnitObservable(), SearchText.AsUnitObservable()).Sort(sortByTitle, null);

            SelectedItems = Items.WhereMany(vm => vm.IsSelected.Value, vm => vm.IsSelected.AsUnitObservable()).Sort(sortByTitle, null);
        }
    }

    public class CollectibleViewModel
    {
        public string OriginalTitle { get; }
        
        public IReadOnlyReactiveProperty<string> Title { get; }
        
        public IReactiveCollection<TagViewModel> Tags { get; }
        
        public IReactiveProperty<bool> IsSelected { get; }
        
        public IReadOnlyReactiveProperty<int> TagsCount { get; }
        
        public ReactiveCommand<Unit> AddRandomTag { get; }
        
        public CollectibleViewModel(Collectible item, IReadOnlyReactiveProperty<string> searchtText, IList<Tag> tags)
        {
            OriginalTitle = item.Title;

            Tags = item.Tags.Select(t => new TagViewModel(t, this)).ToReactiveCollection();

            TagsCount = Tags.ObserveCountChanged(true).ToReadOnlyReactiveProperty();
            
            AddRandomTag = new ReactiveCommand(TagsCount.Select(t => t < 2));
            AddRandomTag.Subscribe(_ => Tags.Add(new TagViewModel(tags[Random.Range(0, tags.Count - 1)], this)));
            
            IsSelected = new ReactiveProperty<bool>();
            
            Title = searchtText.Select(searchVal =>
            {
                if (string.IsNullOrEmpty(searchVal))
                    return OriginalTitle;

                try
                {
                    return Regex.Replace(OriginalTitle, "(" + searchVal
                                                            .Replace("[", "\\[")
                                                            .Replace("]", "\\]")
                                                            .Replace("(", "\\(")
                                                            .Replace(")", "\\)")+ ")",
                        "<b>$1</b>", RegexOptions.IgnoreCase);
                }
                catch (Exception e)
                {
                    return OriginalTitle;
                }
                
                
            }).ToReadOnlyReactiveProperty();
  
        }
    }

    public class TagViewModel
    {
        public string Name { get; }
        public Color Color { get; }
        
        public ReactiveCommand RemoveTag { get; }
        
        public TagViewModel(Tag tag, CollectibleViewModel collectible)
        {
            Name = tag.Name;
            Color = tag.Color;
            
            RemoveTag = new ReactiveCommand();

            RemoveTag.Subscribe(_ => { collectible.Tags.Remove(this); });
        }
    }
}