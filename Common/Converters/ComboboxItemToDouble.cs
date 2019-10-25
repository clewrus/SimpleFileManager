using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace SimpleFM.Common.Converters {
	public class ComboboxItemToDouble : IValueConverter {
		public Object Convert (Object value, Type targetType, Object parameter, CultureInfo culture) {
			if (value is ComboBoxItem item && item.Content is string stringValue) {
				return Double.Parse(stringValue);
			}
			return 0;
		}

		public Object ConvertBack (Object value, Type targetType, Object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
