using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace LibraryBrowser.Views {
	class BookImageConverter : IValueConverter {
		public string Size { get; set; }

		public static readonly BookImageConverter Small = new BookImageConverter { Size = "S" };
		public static readonly BookImageConverter Large = new BookImageConverter { Size = "L" };

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			if (value == null) return null;
			return new Uri("http://covers.openlibrary.org/b/isbn/" + Uri.EscapeUriString(value.ToString()) + "-" + Size + ".jpg");

			//return new Uri("http://www.syndetics.com/index.php?isbn=" + Uri.EscapeDataString(value.ToString()) + "/sc.gif&client=bccls");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}
