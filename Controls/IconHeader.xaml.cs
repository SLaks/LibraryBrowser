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

namespace LibraryBrowser.Controls {
	/// <summary>
	/// Interaction logic for IconHeader.xaml
	/// </summary>
	public partial class IconHeader : UserControl {
		public IconHeader() {
			InitializeComponent();
		}

		public BitmapSource Image {
			get { return (BitmapSource)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(BitmapSource), typeof(IconHeader));

		public string Text {
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register("Text", typeof(string), typeof(IconHeader), new UIPropertyMetadata("Loading"));
	}
}
