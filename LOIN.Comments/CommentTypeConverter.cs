using LOIN.Comments.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace LOIN.Comments
{
    public class CommentTypeConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is CommentType c))
                return null;

            switch (c)
            {
                case CommentType.Comment:
                    return "Komentář";
                case CommentType.NewRequirement:
                    return "Nový požadavek";
                default:
                    throw new NotImplementedException();
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
