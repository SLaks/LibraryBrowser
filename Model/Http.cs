using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System.Threading.Tasks;

namespace LibraryBrowser.Model {
	public static class Http {
		public const string UserAgent = "SLaks.LibraryBrowser (I would like to rewrite your website; contact Dev@SLaks.net)";

		public static void SendRequest(string url, Action<HttpWebResponse> callback) { SendRequest(new Uri(url, UriKind.Absolute), callback); }
		public static void SendRequest(Uri url, Action<HttpWebResponse> callback) {
			var request = (HttpWebRequest)WebRequest.Create(url);

			request.UserAgent = UserAgent;
			request.BeginGetResponse(result => {
				using (var response = request.EndGetResponse(result))
					callback((HttpWebResponse)response);
			}, null);
		}

		public static void RequestHtml(string url, Action<HtmlDocument> callback) { RequestHtml(new Uri(url, UriKind.Absolute), callback); }
		public static void RequestHtml(Uri url, Action<HtmlDocument> callback) {
			SendRequest(url, r => {
				var doc = new HtmlDocument();
				using (var stream = r.GetResponseStream())
					doc.Load(stream);
				callback(doc);
			});
		}

		public static Task<TResult> HtmlTask<TResult>(Uri url, Func<HtmlDocument, TResult> callback) {
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = UserAgent;

			var task = Task.Factory.FromAsync(request.BeginGetResponse(null, null), result => {
				var doc = new HtmlDocument();

				using (var response = request.EndGetResponse(result))
				using (var stream = response.GetResponseStream()) {
					doc.Load(stream);
				}

				return callback(doc);
			});
			return task;
		}
	}
}
