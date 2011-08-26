using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace LibraryBrowser.Views {
	class GoogleStaticMapConverter : IValueConverter {
		public static readonly GoogleStaticMapConverter Instance = new GoogleStaticMapConverter();

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value == null) return null;
			return "http://maps.googleapis.com/maps/api/staticmap?sensor=false&size=" + parameter + "&markers=" + Uri.EscapeDataString(value.ToString());
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
