using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SLaks.Progression;
using System.Net;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace LibraryBrowser.Model {
	public class BcclsClient : ILibraryClient {
		//index=default&record_screen=record.html&
		//index=default&query=tamuli&servers=1home&record_screen=record.html&sort_by=none&setting_key=BCCLS&highlightorlimit=Highlight+for&whichlibrary=All+Libraries&format_filter=ALL&location_filter=ALL&date_filter=all&language_filter=all&hitlist_screen=hitlistPF.html

		static string GetSearchUrl(string query) {
			return @"http://web2.bccls.org/web2/tramp2.exe/do_keyword_search/log_in?servers=1home&setting_key=BCCLS&query="
				   + Uri.EscapeDataString(query);
		}

		static readonly Regex countParser = new Regex(@": Item List \((\d+)\)");
		public void DoSearch(string query, LoadingContext<BookSummary> context) {
			context.Progress.Progress = null;

			Http.RequestHtml(GetSearchUrl(query), doc => {
				if (doc.DocumentNode.Descendants("title").First().InnerText.StartsWith("The Library Catalog : Item #")) {
					context.AddResult(new DetailsPageSummary(doc));
					context.SetCompleted();
					return;
				}

				var serverInput = doc.DocumentNode.Descendants("input").First(n => n.GetAttributeValue("name", "") == "server");
				var countString = serverInput.PreviousSibling.InnerText;
				var count = int.Parse(countParser.Match(countString).Groups[1].Value);

				context.Progress.Maximum = count;
				ParseResultList(doc, context);
			});
		}

		static readonly Regex publisherParser = new Regex(@", c?(\d{4})\.\s*$");
		static int? GetYear(string publisher) {
			int year;
			if (int.TryParse(publisherParser.Match(publisher).Groups[1].Value, out year))
				return year;
			return null;
		}

		#region Result List
		static readonly Uri baseUri = new Uri("http://web2.bccls.org");
		static void ParseResultList(HtmlDocument doc, LoadingContext<BookSummary> context) {
			var mainTable = doc.DocumentNode.FirstChild.Element("body").Element("center").Element("table");

			foreach (var elem in mainTable.Elements("tr")) {
				if (context.Progress.WasCanceled) {
					context.SetCompleted();
					return;
				}

				if (elem.Elements("th").Count() != 2)
					continue;	//These are <hr> rows; ignore them

				context.AddResult(new SearchResult(elem));
				context.Progress.Progress++;
			}

			//Their next link is supposed to be in a <form> tag,
			//but the <form> tag is malformed.  I do not want to
			//rely entirely on text matching for this, so I look
			//in the <form>'s parent.
			var nextLink = doc.DocumentNode.Descendants("form").SelectMany(f => f.ParentNode.Descendants("a"))
															   .FirstOrDefault(a => a.InnerText.Contains("Next"));
			if (nextLink == null) {
				Debug.Assert(context.Progress.Progress == context.Progress.Maximum, "Book count mismatch");
				context.SetCompleted();
			} else {
				Http.RequestHtml(new Uri(baseUri, nextLink.GetAttributeValue("href", "")),
					newDoc => ParseResultList(newDoc, context)
				);
			}
		}
		class SearchResult : BookSummary {
			static readonly Regex imageUrlParser = new Regex(@"http://www\.syndetics\.com/index\.php\?isbn=(\d+).*?/sc\.gif&client=bccls");
			static readonly Regex detailsUrlFinder = new Regex(@"document\.location\.href = ""(.+)&start="" \+ start");
			public SearchResult(HtmlNode tr) {
				var img = tr.Elements("th").First().Descendants("img").SingleOrDefault();
				if (img != null)
					ISBN = imageUrlParser.Match(img.GetAttributeValue("src", "")).Groups[1].Value;

				var details = tr.Elements("td").Last().Element("span");

				//This is "Last, First"
				Author = details.FirstChild.CleanTest().TrimEnd('.');

				var authorTitle = details.Element("b").CleanTest().Split('/');
				Title = authorTitle[0].Trim();

				//This is "First Last"
				//Author = authorTitle[0].Trim();

				var publisherNode = details.Element("b").NextSibling;
				while (String.IsNullOrWhiteSpace(publisherNode.InnerText))
					publisherNode = publisherNode.NextSibling;
				Year = GetYear(publisherNode.InnerText);

				var script = tr.Descendants("script").First();
				sessionDetailsUrl = detailsUrlFinder.Match(script.InnerText).Groups[1].Value;
			}

			readonly string sessionDetailsUrl;
			public override void GetDetails(Action<BookDetails> callback) {
				Http.RequestHtml(new Uri(baseUri, sessionDetailsUrl), doc => {
					if (!doc.DocumentNode.Descendants("title").First().InnerText.StartsWith("Session has expired"))
						callback(new DetailsPageSummary(doc).Details);
					else	//If the session timed out, do a search that will return the first book
						GetFirstResult(ISBN ?? (Author + " " + Title), callback);
				});
			}
		}
		#endregion

		static void GetFirstResult(string query, Action<BookDetails> callback) {
			Http.RequestHtml(GetSearchUrl(query), doc => {
				var isResultPage = doc.DocumentNode.Descendants("title").First().InnerText.StartsWith("The Library Catalog : Item #");

				if (isResultPage)	//Hopefully, the search only had one result, so it took us straight to a details page.
					callback(new DetailsPageSummary(doc).Details);
				else {				//Get the details of the first result
					var mainTable = doc.DocumentNode.FirstChild.Element("body").Element("center").Element("table");
					var tr = mainTable.Elements("tr").First(n => n.Elements("th").Count() == 2);
					new SearchResult(tr).GetDetails(callback);
				}
			});
		}

		static Regex numberFinder = new Regex(@"\d+");
		//http://web2.bccls.org/web2/tramp2.exe/do_keyword_search/log_in?servers=1home&setting_key=BCCLS&query=0061020710
		sealed class DetailsPageSummary : BookSummary {
			public BookDetails Details { get; private set; }
			public DetailsPageSummary(HtmlDocument doc) {
				var infoTable = doc.DocumentNode.Descendants("table").First(t => t.Descendants("th").First().CleanTest() == "Author");

				var rows = infoTable.Elements("tr").ToList();

				Author = rows[0].Element("td").CleanTest().TrimEnd('.');
				Title = rows[1].Descendants("a").First().CleanTest()
							   .TrimEnd('/', ':').TrimEnd();
				var publisher = rows[2].Element("td").CleanTest();
				Year = GetYear(publisher);

				var isbnRow = rows.FirstOrDefault(tr => tr.Element("th").CleanTest() == "ISBN");
				if (isbnRow != null)
					ISBN = numberFinder.Match(isbnRow.Element("td").InnerText).Captures[0].Value;

				Details = new BookDetails(this, publisher, ParseLocations(doc));
			}

			public override void GetDetails(Action<BookDetails> callback) { callback(Details); }
		}

		static IEnumerable<BookLocation> ParseLocations(HtmlDocument doc) {
			var libariesTable = doc.DocumentNode.Descendants("table").First(t => { var th = t.Descendants("th").FirstOrDefault(); return th != null && th.InnerText == "Library"; });

			foreach (var tr in libariesTable.Elements("tr").Skip(1)) {
				var cells = tr.Elements("td").ToList();
				if (cells.Count != 4) continue;
				yield return new BookLocation(
					library: cells[0].CleanTest(),
					material: cells[1].CleanTest(),
					identifier: cells[2].CleanTest(),
					status: cells[3].CleanTest()
				);
			}
		}
	}
}
