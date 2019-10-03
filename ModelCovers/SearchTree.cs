using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.ModelCovers {
	public class SearchTree : DynamicFileSystemTree {
		internal SearchTree () {
			this.Root = new SearchNode(this);
		}

		public struct SearchArguments {
			public SearchType searchType;
			public string searchRootPath;

			public bool considerFilesContent;
			public bool caseSensetive;
			public string requestString;
		}

		public void SetArguments (SearchArguments args){
			isArgsInitialized = true;
			if (IsSearching) {
				throw new Exception("Search was not ended");
			}
			this.Arguments = args;
		}

		public void StartSearch () {
			if (!isArgsInitialized) {
				throw new Exception("SearchTree args were not initialized.");
			}

			Root.ClearObservableCollections();

			var args = Arguments;
			args.requestString = args.requestString.Trim();
			if (Arguments.caseSensetive) {
				args.requestString = args.requestString.ToLower();
			}
			Arguments = args;

			taskExecutionAllowed = true;
			IsSearching = true;
			searchTask = Task.Run(() => {
				SearchMethod(Arguments.searchRootPath, new List<IFileSystemElement>(16));
				IsSearching = false;
			});
		}
		#region ParallelCode
		private void SearchMethod (string curRoot, List<IFileSystemElement> finded) {
			if (!taskExecutionAllowed) return;
			finded.Clear();

			string[] files, directories;
			LoadPathesFromRoot(curRoot, out files, out directories);
			if (!taskExecutionAllowed) return;

			FindNameMatches(finded, files, directories);
			if (!taskExecutionAllowed) return;

			if (Arguments.considerFilesContent) {
				FindContentMatches(finded, files);
			}

			((SearchNode)Root).AddElementsInDispatcher(finded.ToArray());

			if (Arguments.searchType == SearchType.CurrentDirectory) return;
			foreach (var d in directories) {
				SearchMethod(d, finded);
			}
		}

		private void LoadPathesFromRoot (string rootPath, out string[] files, out string[] directories) {
			files = new string[0];
			try {
				files = Directory.GetFiles(rootPath);
			} catch (IOException) { } catch (UnauthorizedAccessException) { };

			directories = new string[0];
			try {
				directories = Directory.GetDirectories(rootPath);
			} catch (IOException) { } catch (UnauthorizedAccessException) { };
		}

		private void FindNameMatches (List<IFileSystemElement> finded, string[] files, string[] directories) {
			string target = Arguments.requestString;
			bool caseSensetive = Arguments.caseSensetive;

			foreach (var f in files) {
				if (!taskExecutionAllowed) break;

				string fileName = (caseSensetive) ? f.ToLower() : f;
				if (fileName.Contains(target)) {
					finded.Add(new SFMFile(f));
				}
			}

			foreach (var d in directories) {
				if (!taskExecutionAllowed) break;

				string directoryName = (caseSensetive) ? Path.GetFileName(d).ToLower() : Path.GetFileName(d);
				if (directoryName.Contains(target)) {
					finded.Add(new SFMDirectory(d));
				}
			}
		}

		private void FindContentMatches (List<IFileSystemElement> finded, string[] files) {
			foreach (string f in files) {
				if (!taskExecutionAllowed) break;

				try {
					string wholeFile;
					using (StreamReader sr = new StreamReader(f)) {
						wholeFile = sr.ReadToEnd();
					}

					if (!taskExecutionAllowed) break;

					if (wholeFile.Contains(Arguments.requestString)) {
						finded.Add(new SFMFile(f));
					}
				} catch (IOException) { }
			}

		}
		#endregion

		public void EndSearch () {
			taskExecutionAllowed = false;

			if (searchTask != null) {
				searchTask.Wait();
				searchTask = null;
			}

			isArgsInitialized = false;
		}

		private Task searchTask;
		private bool taskExecutionAllowed;

		private bool isArgsInitialized;
		public SearchArguments Arguments { get; set; }

		private bool _IsSearching;
		public bool IsSearching {
			get => _IsSearching;
			set {
				if (_IsSearching != value){
					_IsSearching = value;
					OnStatusChanged?.Invoke();
				}
			}
		}
		public event Action OnStatusChanged;

		public enum SearchType { None, CurrentDirectory, Recursive }
	}
}
