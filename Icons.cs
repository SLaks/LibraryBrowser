using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace LibraryBrowser {
	static class Icons {
		public static readonly BitmapImage Loading16 = new BitmapImage(new Uri("pack://application:,,,/Images/Loading16.gif"));
		public static readonly BitmapImage Loading32 = new BitmapImage(new Uri("pack://application:,,,/Images/Loading32.gif"));

		public static readonly BitmapImage Book16 = new BitmapImage(new Uri("pack://application:,,,/Images/Book16.png"));
		public static readonly BitmapImage SearchResults16 = new BitmapImage(new Uri("pack://application:,,,/Images/SearchResults16.png"));
	}
}
