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
	/// Interaction logic for ProgressHeader.xaml
	/// </summary>
	public partial class ProgressHeader : UserControl {
		public ProgressHeader() {
			InitializeComponent();
		}
		
		public string Caption {
			get { return (string)GetValue(CaptionProperty); }
			set { SetValue(CaptionProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CaptionProperty =
			DependencyProperty.Register("Caption", typeof(string), typeof(ProgressHeader), new UIPropertyMetadata("Loading"));

		public bool IsLoading {
			get { return (bool)GetValue(IsLoadingProperty); }
			set { SetValue(IsLoadingProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsLoading.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsLoadingProperty =
			DependencyProperty.Register("IsLoading", typeof(bool), typeof(ProgressHeader), new UIPropertyMetadata(true));

		public BitmapSource Image {
			get { return (BitmapSource)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(BitmapSource), typeof(ProgressHeader));
	}
}
