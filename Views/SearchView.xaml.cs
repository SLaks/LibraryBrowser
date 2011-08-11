using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LibraryBrowser.Views {
	/// <summary>
	/// Interaction logic for SearchView.xaml
	/// </summary>
	public partial class SearchView : UserControl {
		public SearchView() {
			InitializeComponent();
		}


		private void Book_MouseUp(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Left && e.ClickCount >= 2) {
				var source = (FrameworkElement)e.OriginalSource;
				var book = (Model.BookSummary)source.DataContext;
				this.FindAncestor<MainWindow>().OpenBook(book);
			}
		}
	}
}
