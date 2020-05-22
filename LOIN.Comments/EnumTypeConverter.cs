using LOIN.Comments.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace LOIN.Comments
{
    public class EnumTypeConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var fieldInfo = value.GetType().GetField(value.ToString());

            var attribute = fieldInfo
                .GetCustomAttributes(false)
                .OfType<DescriptionAttribute>()
                .FirstOrDefault();

            if (attribute == null)
            {
                return value.ToString();
            }
            else
            {
                return attribute.Description;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
