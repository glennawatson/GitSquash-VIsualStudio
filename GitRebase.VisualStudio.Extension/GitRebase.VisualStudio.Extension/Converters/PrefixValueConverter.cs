namespace GitSquash.VisualStudio.Extension.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Markup;

    /// <summary>
    /// Grabs only the prefix of a value.
    /// </summary>
    public class PrefixValueConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// Gets or sets the length of the prefix.
        /// </summary>
        public int PrefixLength { get; set; } = 6;

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string s = value.ToString();

            return s.Length <= this.PrefixLength ? s : s.Substring(0, this.PrefixLength);
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
