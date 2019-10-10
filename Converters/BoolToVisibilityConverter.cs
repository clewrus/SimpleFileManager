using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SimpleFM.Converters {
	public class BoolToVisibilityConverter : IValueConverter {
		public Object Convert (Object value, Type targetType, Object parameter, CultureInfo culture) {
			if (value is bool v) {
				return (v) ? Visibility.Visible : Visibility.Hidden;
			}

			return Visibility.Visible;
		}

		public Object ConvertBack (Object value, Type targetType, Object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
