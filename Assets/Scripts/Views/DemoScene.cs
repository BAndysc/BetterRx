using System;
using BetterRx;
using Models;
using UnityEngine;
using UnityEngine.UI;
using ViewModels;

namespace Views
{
    public class DemoScene : MonoBehaviour
    {
        [SerializeField] private CollectiblesList collectiblesList;
        
        [SerializeField] private TagsList tagsList;

        [SerializeField] private CollectibleView collectibleView;

        [SerializeField] private Transform collectiblesTransform;

        [SerializeField] private InputField inputField;
        
        [SerializeField] private Transform selectedParent;
        
        private IDisposable binding;
        private CollectiblesViewModel collectiblesViewModel;

        private void Awake()
        {
            collectiblesViewModel = new CollectiblesViewModel(collectiblesList, tagsList);
        }

        private void Start()
        {
            binding = new CollectionBinder<CollectibleViewModel>(collectiblesViewModel.Items, collectibleView,
                collectiblesTransform);

            new CollectionBinder<CollectibleViewModel>(collectiblesViewModel.SelectedItems, collectibleView,
                selectedParent);
            
            inputField.BindText(collectiblesViewModel.SearchText);
        }

        private void OnDestroy()
        {
            binding.Dispose();
            binding = null;
        }
    }
}