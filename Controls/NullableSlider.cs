using System.Windows.Controls;
using System.Windows;
using BookabookWPF.Converters;
using System.Windows.Data;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BookabookWPF.Controls
{
    public class NullableSlider : Slider
    {
        public static readonly DependencyProperty NullableValueProperty =
            DependencyProperty.Register(nameof(NullableValue), typeof(double?),
                typeof(NullableSlider), new PropertyMetadata(null, OnNullableValueChanged));

        public event EventHandler<ValueChangedEventArgs<double?>>? NullableValueChanged;
        protected bool isControlLoaded = false;
        private bool isInitializing = true;

        public NullableSlider()
        {
            // Set initial value to avoid validation issues
            Value = 0;

            Loaded += (s, e) =>
            {
                // Store original minimum
                double originalMin = Minimum;
                // Set minimum one less than original
                Minimum = originalMin - 1;

                isControlLoaded = true;
                isInitializing = false;

                // Set the value after initialization
                if (NullableValue.HasValue)
                {
                    Value = NullableValue.Value;
                }
                else
                {
                    Value = Minimum;
                }
            };

            ValueChanged += (s, e) =>
            {
                if (!isControlLoaded || isInitializing) return;

                if (Value <= Minimum)
                {
                    NullableValue = null;
                }
                else
                {
                    NullableValue = Value;
                }
            };
        }

        public double? NullableValue
        {
            get => (double?)GetValue(NullableValueProperty);
            set
            {
                if (value.HasValue && value.Value <= Minimum && !isInitializing)
                {
                    value = null;
                }
                SetValue(NullableValueProperty, value);
            }
        }

        private static void OnNullableValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NullableSlider slider && slider.isControlLoaded && !slider.isInitializing)
            {
                if (e.NewValue is double newValue)
                {
                    slider.Value = newValue;
                }
                else
                {
                    slider.Value = slider.Minimum;
                }

                slider.NullableValueChanged?.Invoke(slider,
                    new ValueChangedEventArgs<double?>(
                        (double?)e.OldValue,
                        (double?)e.NewValue));
            }
        }
    }



    public class ValueChangedEventArgs<T> : EventArgs
    {
        public T OldValue { get; }
        public T NewValue { get; }

        public ValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
