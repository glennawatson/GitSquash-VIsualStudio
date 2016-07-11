namespace GitSquash.VisualStudio.Extension.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Converter which will make controls visible if any of the boolean values are true.
    /// </summary>
    [ValueConversion(typeof(bool[]), typeof(Visibility))]
    public class AnyTrueToVisibleConverter : IMultiValueConverter
    {
        /// <inheritdoc />
        public virtual object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool show = false;
            if (values == null || values.Length == 0)
            {
                return Visibility.Collapsed;
            }

            foreach (object obj in values)
            {
                if (!(obj is bool) || !(bool)obj)
                {
                    continue;
                }

                show = true;
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
