using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using HtmlAgilityPack;

namespace LibraryBrowser {
	static class Extensions {
		public static T FindAncestor<T>(this DependencyObject obj) where T : DependencyObject {
			if (obj == null) return null;
			var parent = VisualTreeHelper.GetParent(obj);
			return parent as T ?? parent.FindAncestor<T>();
		}


		public static string CleanTest(this HtmlNode node) {
			return HtmlEntity.DeEntitize(node.InnerText).Trim();
		}
	}
}
