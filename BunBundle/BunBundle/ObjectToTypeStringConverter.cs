﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace BunBundle {
    public class ObjectToTypeStringConverter : IValueConverter {
        public object? Convert(
            object? value, Type targetType,
            object? parameter, System.Globalization.CultureInfo culture) {
            return value?.GetType().Name;
        }

        public object ConvertBack(
            object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture) {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
}
