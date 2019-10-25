using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.FileManager.ModelCovers {
	public class FileTreeNode : INotifyPropertyChanged, IDisposable {
		protected FileTreeNode (DynamicFileSystemTree tree) {
			this.Tree = tree;
			this.NotRenaming = true;
		}

		internal FileTreeNode (DynamicFileSystemTree tree, FileTreeNode child) : this(tree) {
			ChildDirectoryNodes = new ObservableCollection<FileTreeNode>();
			ChildFileNodes = new ObservableCollection<FileTreeNode>();

			ChildDirectoryNodes.Add(child);
		}

		internal FileTreeNode (DynamicFileSystemTree tree, FileTreeNode parent, IFileSystemElement value) : this(tree) {
			this.Value = value;
			this.IsRoot = (parent == null);
			this.Parent = parent;
			
			this.OpenedAtLeastOnce = IsRoot;

			this.ChildrenInitialized = !IsDirectory;
			this.IsUpdating = IsExpanded;

			this.PropertyChanged += SetIsUpdatingToIsExpanding;

			if (IsRoot || (parent != null && parent.OpenedAtLeastOnce && parent.ChildDirectoryNodes.Count < 256)) {
				LoadChildren();
			} else {
				parent.PropertyChanged += OnPropertyChanged;
			}
		}

		~FileTreeNode () {
			Dispose(false);
		}

		public void Dispose () {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose (bool disposing) {
			if (_FileWatcher != null) {
				_FileWatcher.Dispose();
				_FileWatcher = null;
			}

			DisposeObservableCollections();
		}

		internal void DisposeObservableCollections () {
			if (ChildDirectoryNodes != null) {
				foreach (var child in ChildDirectoryNodes) {
					child.Dispose();
				}
				try {
					ChildDirectoryNodes.Clear();
				} catch (NotSupportedException) { }
				
			}

			if (ChildFileNodes != null) {
				foreach (var child in ChildFileNodes) {
					child.Dispose();
				}
				try {
					ChildFileNodes.Clear();
				} catch (NotSupportedException) { }
			}
		}

		private void SetIsUpdatingToIsExpanding (object obj, PropertyChangedEventArgs args) {
			if (obj == this && args.PropertyName == "IsExpanded") {
				IsUpdating = IsExpanded;
				if (IsExpanded == true) {
					UpdateSelf();
				}
			}
		}

		internal void UpdateSelf () {
			if (!IsDirectory) return;
			ChildrenInitialized = false;

			foreach (var child in ChildDirectoryNodes) {
				child.Dispose();
			}
			ChildDirectoryNodes.Clear();
			
			foreach (var child in ChildFileNodes) {
				child.Dispose();
			}
			ChildFileNodes.Clear();

			LoadChildren();
		}

		private void OnPropertyChanged (object obj, PropertyChangedEventArgs args) {
			if (obj == Parent && args.PropertyName == "IsExpanded") {
				if (!ChildrenInitialized && Parent.OpenedAtLeastOnce && Parent.ChildDirectoryNodes.Count < 256) {
					LoadChildren();
				}
			}
		}

		internal void LoadChildren () {
			if (ChildrenInitialized) return;

			CreateChildNodesCollection();
			StartFillingChildNodesCollections();
		}

		#region Initialization
		protected void CreateChildNodesCollection () {
			ChildDirectoryNodes = new ObservableCollection<FileTreeNode>();
			ChildFileNodes = new ObservableCollection<FileTreeNode>();
		}

		private void StartFillingChildNodesCollections () {
			Task.Run(() => FillChildNodesCollections());

			ChildrenInitialized = true;
		}

		private async void FillChildNodesCollections () {
			Task<string[]> fileTask = Task.Run(() => {
				try {
					return Directory.GetFiles(Value.ElementPath);
				} catch (IOException) { } catch (UnauthorizedAccessException) { };
				return new string[0];
			});

			Task<string[]> directoryTask = Task.Run(() => {
				try {
					return Directory.GetDirectories(Value.ElementPath);
				} catch (IOException) { } catch (UnauthorizedAccessException) { };
				return new string[0];
			});

			string[] files = await fileTask;
			string[] directories = await directoryTask;
			await App.Current.Dispatcher.BeginInvoke((Action)(() => {
				foreach (var file in files) {
					ChildFileNodes.Add(new FileTreeNode(Tree, this, new SFMFile(file)));
				}

				foreach (var directory in directories) {
					ChildDirectoryNodes.Add(new FileTreeNode(Tree, this, new SFMDirectory(directory)));
				}
			}));
		}
		#endregion

		#region TreeNodeUpdating
		internal void SetUpdating (bool value) {
			IsUpdating = IsDirectory && value;

			if (ChildDirectoryNodes == null) return;
			foreach (var childDir in ChildDirectoryNodes) {
				childDir.SetUpdating(value);
			}
		}

		private async void SetFileSystemWatcher (bool value) {
			UnBindToFileWatcherEvents(FileWatcher);

			if (value) {
				BindToFileWatcherEvents(FileWatcher);
			}

			await Task.Run( () => {
				try {
					FileWatcher.EnableRaisingEvents = value;
				} catch (FileNotFoundException) { };
			});
		}

		private void BindToFileWatcherEvents (FileSystemWatcher watcher) {
			watcher.Deleted += ElementDeleatedHandler;
			watcher.Renamed += ElementRenamedHandler;
			watcher.Changed += ElementChangedHandler;

			watcher.Created += ElementCreatedHandler;
		}

		private void UnBindToFileWatcherEvents (FileSystemWatcher watcher) {
			watcher.Deleted -= ElementDeleatedHandler;
			watcher.Renamed -= ElementRenamedHandler;
			watcher.Changed -= ElementChangedHandler;

			watcher.Created -= ElementCreatedHandler;
		}

		private FileTreeNode FindNodeInCollection (out ObservableCollection<FileTreeNode> collection, IFileSystemElement elem) {
			bool elemIsDirectory = (elem.ElementType == FileSystemFacade.ElementType.Directory);
			collection = (elemIsDirectory) ? ChildDirectoryNodes : ChildFileNodes;

			foreach (var node in ((elemIsDirectory)? ChildDirectoryNodes: ChildFileNodes)) {
				if (node.Value.ElementPath == elem.ElementPath) {
					return node;
				}
			}
			return null;
		}

		private bool RemoveChildFromNode (IFileSystemElement element) {
			ObservableCollection<FileTreeNode> targetCollection;
			FileTreeNode targetNode = FindNodeInCollection(out targetCollection, element);

			if (targetNode == null) return false;

			targetCollection.Remove(targetNode);
			targetNode.SetUpdating(false);

			Tree.OnTreeNodeDelete(targetNode);
			return true;
		}

		private void AddChildToNode (IFileSystemElement element) {
			FileTreeNode nwNode = new FileTreeNode(Tree, this, element);
			if (nwNode.IsDirectory) {
				ChildDirectoryNodes?.Add(nwNode);
			} else {
				ChildFileNodes?.Add(nwNode);
			}
		}

		private async void ElementDeleatedHandler (object source, FileSystemEventArgs e) {
			if (!ChildrenInitialized) return;

			await App.Current.Dispatcher.BeginInvoke((Action)(() => RemoveChildFromNode(new SFMFile(e.FullPath))));
			await App.Current.Dispatcher.BeginInvoke((Action)(() => RemoveChildFromNode(new SFMDirectory(e.FullPath))));

			Console.WriteLine("File was removed: " + e.FullPath);
		}

		private void ElementCreatedHandler (object source, FileSystemEventArgs e) {
			if (!ChildrenInitialized) return;

			IFileSystemElement element = FileSystemFacade.Instance.GetElementFromPath(e.FullPath);
			App.Current.Dispatcher.Invoke(() => AddChildToNode(element));

			Console.WriteLine("File was created: " + e.FullPath);
		}

		private void ElementRenamedHandler (object source, RenamedEventArgs e) {
			if (!ChildrenInitialized) return;

			IFileSystemElement element = FileSystemFacade.Instance.GetElementFromPath(e.FullPath);

			IFileSystemElement oldElement;
			if (element.ElementType == FileSystemFacade.ElementType.Directory) {
				oldElement = new SFMDirectory(e.OldFullPath);
			} else {
				oldElement = new SFMFile(e.OldFullPath);
			}

			App.Current.Dispatcher.Invoke(() => RemoveChildFromNode(oldElement));
			App.Current.Dispatcher.Invoke(() => AddChildToNode(element));

			Console.WriteLine("File was renamed: " + e.FullPath);
		}

		private void ElementChangedHandler (object source, FileSystemEventArgs e) {
			if (!ChildrenInitialized) return;
			Console.WriteLine("File was changed: " + e.FullPath);
		}
		#endregion

		private void InvokePropertyChanged ([CallerMemberName] string name = "") {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		public IFileSystemElement Value { get; protected set; }

		public bool IsDirectory { get { return Value is SFMDirectory; } }
		public bool IsRoot { get; private set; }

		#region TreeStructureProperties
		public bool ChildrenInitialized { get; private set; }

		private ObservableCollection<FileTreeNode> _ChildFileNodes;
		public ObservableCollection<FileTreeNode> ChildFileNodes {
			get { return _ChildFileNodes; }
			set { _ChildFileNodes = value; InvokePropertyChanged(); }
		}

		private ObservableCollection<FileTreeNode> _ChildDirectoryNodes;
		public ObservableCollection<FileTreeNode> ChildDirectoryNodes {
			get { return _ChildDirectoryNodes; }
			set { _ChildDirectoryNodes = value; InvokePropertyChanged(); }
		}

		private bool _NotRenaming;
		public bool NotRenaming {
			get => _NotRenaming;
			set { _NotRenaming = value; InvokePropertyChanged(); }
		}

		private bool _IsCutted;
		public bool IsCutted {
			get => _IsCutted;
			set { _IsCutted = value; InvokePropertyChanged(); }
		}

		public ObservableCollection<FileTreeNode> ParentNodeCollection {
			get {
				var parentCollection = new ObservableCollection<FileTreeNode>();

				if (Parent != null) {
					parentCollection.Add(Parent);
					return parentCollection;
				}

				string parentPath = (Value == null)? null: Path.GetDirectoryName(Value.ElementPath);
				if (parentPath != null && parentPath != string.Empty) {
					parentCollection.Add(FileSystemFacade.Instance.CreateFileSystemTree(new SFMParentDirectory(parentPath)).Root);
				}

				return parentCollection;
			}
		}
		#endregion

		private bool _IsExpanded;
		public bool IsExpanded {
			get { return IsRoot || IsDirectory && _IsExpanded; }
			set {
				bool initVal = IsExpanded;
				_IsExpanded = value;
				OpenedAtLeastOnce |= value;

				if (IsExpanded && !ChildrenInitialized) {
					LoadChildren();
				}

				if (initVal != IsExpanded) {
					InvokePropertyChanged();
				}
			}
		}

		private FileSystemWatcher _FileWatcher;
		private FileSystemWatcher FileWatcher {
			get {
				if (_FileWatcher == null) {
					try {
						if (Value == null) return null;

						_FileWatcher = new FileSystemWatcher(Value.ElementPath);
					} catch (ArgumentException) {
						return null;
					}
				}
				return _FileWatcher;
			}
		}

		private bool _IsUpdating;
		public bool IsUpdating {
			get { return _IsUpdating; }
			set {
				var initValue = IsUpdating;
				_IsUpdating = IsDirectory && value;

				if (initValue != IsUpdating) {
					SetFileSystemWatcher(value);
				}
			}
		}

		public DynamicFileSystemTree Tree { get; private set; }
		public bool OpenedAtLeastOnce { get; private set; }

		public FileTreeNode Parent { get; protected set; }
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
