namespace GitSquash.VisualStudio.Extension.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Converter that will be visible only if all the boolean values passed are true.
    /// </summary>
    [ValueConversion(typeof(bool[]), typeof(Visibility))]
    public class AllTrueToVisibleConverter : IMultiValueConverter
    {
        /// <inheritdoc />
        public virtual object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool show = false;
            if (values == null || values.Length == 0)
            {
                return Visibility.Collapsed;
            }

            show = true;
            foreach (object obj in values)
            {
                bool boolValue = false;
                if (obj is bool)
                {
                    boolValue = (bool)obj;
                }

                if (boolValue)
                {
                    continue;
                }

                show = false;
                break;
            }

            return show ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <inheritdoc />
        public virtual object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            return new object[0];
        }
    }
}
