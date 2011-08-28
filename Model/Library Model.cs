using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SLaks.Progression;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LibraryBrowser.Model {
	public class LibraryInfo {
		public string FullTitle { get; protected set; }
		public string Name { get; protected set; }
		public string PhotoUrl { get; protected set; }
		public string Address { get; protected set; }
		public string Phone { get; protected set; }

		public string DetailsUrl { get; protected set; }
	}

	public interface ILibraryClient {
		void DoSearch(string query, LoadingContext<BookSummary> context);

		//TODO: Library info
		Task<LibraryInfo> GetLibrary(string name);
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

		public abstract Task<BookDetails> GetDetails();
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
		readonly Func<Task<LibraryInfo>> detailGetter;
		public BookLocation(string library, string material, string identifier, string status, Func<Task<LibraryInfo>> detailGetter) {
			Library = library;
			Material = material;
			Identifier = identifier;
			Status = status;

			this.detailGetter = detailGetter;
		}

		public string Library { get; private set; }
		public string Material { get; private set; }
		public string Identifier { get; private set; }
		public string Status { get; private set; }

		public Task<LibraryInfo> LibraryInfo { get { return detailGetter(); } }
	}
}
