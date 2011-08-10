using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SLaks.Progression;
using System.Collections.ObjectModel;

namespace LibraryBrowser.Model {
	public interface ILibrarySearcher {
		void DoSearch(string query, LoadingContext<BookSummary> context);

		//TODO: Library info
	}

	 public abstract class LoadingContext<T> {
		public abstract void AddResult(T book);
		public IProgressReporter Progress { get; protected set; }

		public abstract void SetCompleted();
	}

	public abstract class BookSummary {
		public string Author { get; protected set; }
		public string Title { get; protected set; }
		public int? Year { get; protected set; }
		public string ISBN { get; protected set; }

		public abstract void GetDetails(Action<BookDetails> callback);
	}

	public class BookDetails {
		public BookDetails(BookSummary summary, string publisher, IEnumerable<BookLocation> locations) {
			Summary = summary;
			Publisher = publisher;
			Locations = new ReadOnlyCollection<BookLocation>(locations.ToList());
		}

		public BookSummary Summary { get; private set; }

		public string Publisher { get; private set; }

		public ReadOnlyCollection<BookLocation> Locations { get; private set; }
	}
	public class BookLocation {
		public BookLocation(string library, string material, string identifier, string status) {
			Library = library;
			Material = material;
			Identifier = identifier;
			Status = status;
		}

		public string Library { get; private set; }
		public string Material { get; private set; }
		public string Identifier { get; private set; }
		public string Status { get; private set; }
	}
}
