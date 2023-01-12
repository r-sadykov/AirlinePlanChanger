using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

namespace AirlinePlanChanges_MailCenter.Utils
{
    public static class Enum<T>
    {
        public static IEnumerable<string> GetNames()
        {
            var type = typeof(T);
            if (!type.IsEnum)
            {
                throw new ArgumentException("Type '" + type.Name + " is not ENUM");
            }
            return (
                from field in type.GetFields(BindingFlags.Public | BindingFlags.Static)
                where field.IsLiteral
                select field.Name).ToList();
        }
    }

    public class EnumDescriptionConverter : IValueConverter
    {
        private string GetEnumDescription(Enum enumObj)
        {
            FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

            object[] attribArray = fieldInfo.GetCustomAttributes(false);

            if (attribArray.Length == 0)
            {
                return enumObj.ToString();
            }

            DescriptionAttribute attrib = attribArray[0] as DescriptionAttribute;
            return attrib?.Description;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Enum myEnum = (Enum)value;
            string description = GetEnumDescription(myEnum);
            return description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }

    public static class EnumUtility
    {
        // Might want to return a named type, this is a lazy example (which does work though)
        public static object[] GetValuesAndDescriptions(Type enumType)
        {
            var values = Enum.GetValues(enumType).Cast<object>();
            var valuesAndDescriptions = from value in values
                select new
                {
                    Value = value,
                    value.GetType()
                        .GetMember(value.ToString())[0]
                        .GetCustomAttributes(true)
                        .OfType<DescriptionAttribute>()
                        .First()
                        .Description
                };
            // ReSharper disable once CoVariantArrayConversion
            return valuesAndDescriptions.ToArray();
        }
    }
}