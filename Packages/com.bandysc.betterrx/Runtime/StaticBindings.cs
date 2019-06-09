using System;
using UniRx;
using UnityEngine.UI;

namespace BetterRx
{
    public static class StaticBindings
    {
        public static IDisposable BindText(this InputField inputField, IReactiveProperty<string> property)
        {
            return new InputFieldBinding(inputField, property);
        }
        
        public static IDisposable BindChecked(this Toggle toggle, IReactiveProperty<bool> property)
        {
            return new ToggleBinding(toggle, property);
        }
    }

    public class ToggleBinding : IDisposable
    {
        private readonly Toggle toggle;
        private readonly IReactiveProperty<bool> property;

        private IDisposable binding;

        private bool inEventHandler;
        
        public ToggleBinding(Toggle toggle, IReactiveProperty<bool> property)
        {
            this.toggle = toggle;
            this.property = property;

            binding = property.Subscribe(newValue =>
            {
                if (inEventHandler)
                    return;

                this.toggle.isOn = newValue;
            });
            
            toggle.onValueChanged.AddListener(OnToggle);
        }

        private void OnToggle(bool val)
        {
            inEventHandler = true;

            property.Value = val;
            
            inEventHandler = false;
        }

        public void Dispose()
        {
            toggle.onValueChanged.RemoveListener(OnToggle);
            binding.Dispose();
        }
    }

    public class InputFieldBinding : IDisposable
    {
        private readonly InputField inputField;
        private readonly IReactiveProperty<string> property;

        private IDisposable binding;

        private bool inEventHandler;
        
        public InputFieldBinding(InputField inputField, IReactiveProperty<string> property)
        {
            this.inputField = inputField;
            this.property = property;
            inputField.onValueChanged.AddListener(OnValueChanged);

            binding = property.Subscribe(newVal =>
            {
                if (inEventHandler)
                    return;

                this.inputField.text = newVal;
            });
        }

        private void OnValueChanged(string arg0)
        {
            inEventHandler = true;

            property.Value = arg0;
            
            inEventHandler = false;
        }

        public void Dispose()
        {
            inputField.onValueChanged.RemoveListener(OnValueChanged);
            binding.Dispose();
        }
    }
}