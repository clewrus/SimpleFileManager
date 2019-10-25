using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using SimpleFM.FileManager.ModelCovers;
using SimpleFM.ModelCovers;

namespace SimpleFM.Common.Converters {
	class FileSystemElementConverter : IValueConverter {
		public Object Convert (Object value, Type targetType, Object parameter, CultureInfo culture) {
			return null;
		}

		public Object ConvertBack (Object value, Type targetType, Object parameter, CultureInfo culture) {
			if (targetType != typeof(string)) {
				throw new NotImplementedException();
			}

			if (value is IFileSystemElement element) {
				return element.ElementName;
			}

			return null;
		}
	}
}
