namespace GitSquash.VisualStudio.Extension.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// A converter which will make a object visible if false.
    /// </summary>
    public class FalseToVisibilityConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool show = false;

            Type valueType = value?.GetType();
            if (valueType == null)
            {
                return Visibility.Collapsed;
            }

            if (valueType == typeof(bool))
            {
                show = !(bool)value;
            }
            else if (valueType == typeof(bool?))
            {
                bool? flag = (bool?)value;
                show = flag.Value == false;
            }

            return show ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
