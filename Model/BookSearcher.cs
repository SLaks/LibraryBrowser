using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading;
using SLaks.Progression;
using System.ComponentModel;

namespace LibraryBrowser.Model {
	class BookSearcher {
		public BookSearcher(params ILibrarySearcher[] libraries) : this((IEnumerable<ILibrarySearcher>)libraries) { }
		public BookSearcher(IEnumerable<ILibrarySearcher> libraries) {
			Libraries = new ReadOnlyCollection<ILibrarySearcher>(libraries.ToList());
		}

		public ReadOnlyCollection<ILibrarySearcher> Libraries { get; private set; }

		public LoadingCollection<BookSummary> DoSearch(string query, IProgressReporter progress) {
			var retVal = new LoadingCollection<BookSummary>();

			foreach (var library in Libraries)
				library.DoSearch(query, retVal.CreateContext(progress));

			return retVal;
		}
	}

	class LoadingCollection<T> : ReadOnlyObservableCollection<T> {
		public SynchronizationContext SyncContext { get; private set; }

		void BeginInvoke(Action m) {
			if (SyncContext != null)
				SyncContext.Post(_ => m(), null);
			else
				m();
		}

		public LoadingCollection()
			: base(new ObservableCollection<T>()) {
			SyncContext = SynchronizationContext.Current;
		}

		int remainingLoaders;
		public bool IsLoading { get { return remainingLoaders > 0; } }

		sealed class Context : LoadingContext<T> {
			readonly LoadingCollection<T> parent;
			public Context(LoadingCollection<T> parent, IProgressReporter progress) {
				this.parent = parent;
				base.Progress = progress;
			}

			public override void AddResult(T book) {
				parent.BeginInvoke(() => parent.Items.Add(book));
			}

			public override void SetCompleted() {
				if (0 == Interlocked.Decrement(ref parent.remainingLoaders))
					parent.OnPropertyChanged(new PropertyChangedEventArgs("IsLoading"));
			}
		}

		public LoadingContext<T> CreateContext(IProgressReporter progress) {
			if (1 == Interlocked.Increment(ref remainingLoaders))
				OnPropertyChanged(new PropertyChangedEventArgs("IsLoading"));

			return new Context(this, progress);
		}
	}
}
