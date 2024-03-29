﻿using System;
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
using SLaks.Progression.Display;

namespace LibraryBrowser {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
		}

		private void Search_Click(object sender, RoutedEventArgs e) {
			OpenSearch(searchBox.Text);
		}

		static readonly Model.BookSearcher searcher = new Model.BookSearcher(new Model.BcclsClient());
		public void OpenSearch(string query) {
			var progress = new ProgressModel();
			var results = searcher.DoSearch(query, progress);

			var headerControl = new Controls.ProgressHeader { Caption = query, DataContext = progress, Image = Icons.SearchResults16 };
			headerControl.SetBinding(Controls.ProgressHeader.IsLoadingProperty, new Binding("IsLoading") { Source = results });

			var tab = new TabItem {
				Header = headerControl,
				Content = new Views.SearchView { DataContext = results }
			};

			tabs.Items.Add(tab);
			tab.IsSelected = true;
		}
		public void OpenBook(Model.BookSummary book) {
			var headerControl = new Controls.IconHeader { Image = Icons.Loading16, Text = book.Title + " by " + book.Author };
			var detailsTask = book.GetDetails();

			var tab = new TabItem {
				Header = headerControl,
				Content = new Controls.LoadingContentPresenter {
					Content = detailsTask,
					ContentTemplate = new DataTemplate { VisualTree = new FrameworkElementFactory(typeof(Views.BookDetailsView)) }
				}
			};

			tabs.Items.Add(tab);
			tab.IsSelected = true;

			detailsTask.ContinueWith(t => Dispatcher.BeginInvoke(new Action(delegate {
				headerControl.Image = Icons.Book16;
				headerControl.Text = t.Result.Summary.Title + " by " + t.Result.Summary.Author;
			})));
		}

		private void TabItem_MouseUp(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Middle)
				tabs.Items.Remove(sender);
		}
	}
}
