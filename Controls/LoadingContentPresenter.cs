using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace LibraryBrowser.Controls {
	[ContentProperty("ContentTemplate")]
	public class LoadingContentPresenter : Decorator {
		readonly ContentPresenter presenter = new ContentPresenter();

		static LoadingContentPresenter() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingContentPresenter), new FrameworkPropertyMetadata(typeof(LoadingContentPresenter)));
		}

		public LoadingContentPresenter() {
			Child = presenter;
			OnContentChanged();
		}

		public static readonly DependencyProperty ContentProperty =
			DependencyProperty.Register("Content", typeof(object), typeof(LoadingContentPresenter),
			new UIPropertyMetadata(null, (s, e) => ((LoadingContentPresenter)s).OnContentChanged())
		);
		public object Content {
			get { return (object)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(LoadingContentPresenter));
		public static readonly DependencyProperty LoadingTemplateProperty = DependencyProperty.Register("LoadingTemplate", typeof(DataTemplate), typeof(LoadingContentPresenter));

		public DataTemplate ContentTemplate {
			get { return (DataTemplate)GetValue(ContentTemplateProperty); }
			set { SetValue(ContentTemplateProperty, value); }
		}
		public DataTemplate LoadingTemplate {
			get { return (DataTemplate)GetValue(LoadingTemplateProperty); }
			set { SetValue(LoadingTemplateProperty, value); }
		}

		void OnContentChanged() {
			if (Content == null || Content == DependencyProperty.UnsetValue)
				SetLoading();
			else {
				var task = Content as Task;

				if (task == null)
					SetContent("Content");
				else if (task.IsCompleted)
					SetContent("Content.Result");
				else {
					SetLoading();
					task.ContinueWith(delegate {
						Dispatcher.BeginInvoke(new Action(() => SetContent("Content.Result")));
					});
				}
			}
		}

		void SetLoading() {
			presenter.SetBinding(ContentPresenter.ContentTemplateProperty, new Binding("LoadingTemplate") { Source = this });
			presenter.Content = "Loading";
		}
		void SetContent(string path) {
			presenter.SetBinding(ContentPresenter.ContentTemplateProperty, new Binding("ContentTemplate") { Source = this });
			presenter.SetBinding(ContentPresenter.ContentProperty, new Binding(path) { Source = this });
		}
	}
}
