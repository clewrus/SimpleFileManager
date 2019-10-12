using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Collections.ObjectModel;
using SimpleFM.ModelCovers;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Windows.Data;
using System.Windows.Input;
using SimpleFM.Commands;
using System.IO;
using System.Collections;
using System.Windows;
using SimpleFM.PageManager;
using SimpleFM.Pages;

namespace SimpleFM.ViewModels {
	public class FileManagerViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable {
		public FileManagerViewModel () {
			AvailableDrives = new ObservableCollection<SFMDirectory>(FileSystemFacade.Instance.AvailableDrives);

			UpdateRootDirectory();
			AvailableDrives.CollectionChanged += UpdateRootDirectory;
			FileSystemFacade.Instance.ElementDeleted += OnElementDelete;
			SearchTreeInstance.StatusChanged += SearchTreeStatusChangedHandler;
		}

		~FileManagerViewModel () {
			Dispose(false);
		}

		public void Dispose () {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose (bool disposing) {
			if (FileSystemTree != null) {
				FileSystemTree.Dispose();
			}
			if (FocusedTreeNode != null) {
				FocusedTreeNode.Tree.Dispose();
			}
			if (_SearchTreeInstance != null) {
				_SearchTreeInstance.Dispose();
			}
			
		}

		private void SearchTreeStatusChangedHandler (object sender, EventArgs e) {
			ActionOnSearchCommand = "";
		}

		private void UpdateRootDirectory () {
			var availableDrives = AvailableDrives;
			if (RootDirectory == null || !availableDrives.Contains(RootDirectory)) {
				RootDirectory = availableDrives[0];
			}
		}
		private void UpdateRootDirectory (Object sender, NotifyCollectionChangedEventArgs e) {
			UpdateRootDirectory();
		}

		private void ChangeFocusedTreeNode (IFileSystemElement targetElement) {
			Task.Run(() => SearchTreeInstance.EndSearch());
			
			var nwTreeRoot = FileSystemFacade.Instance.CreateFileSystemTree(targetElement as SFMDirectory).Root;
			DynamicFileSystemTree.LoadNodesChildren(nwTreeRoot);

			FocusedTreeNode = nwTreeRoot;
		}

		private void HandleFocusedDirectoryPathChange (string nwFocusedDirectoryPath) {
			if (Directory.Exists(nwFocusedDirectoryPath)) {
				ChangeFocusedTreeNode(new SFMDirectory(nwFocusedDirectoryPath));
			}
		}

		private void OnElementDelete (object sender, SFMEventArgs e) {
			string currentPath = FocusedTreeNode.Value.ElementPath;
			string deletedPath = e.element.ElementPath;
			
			while (currentPath != deletedPath) {
				currentPath = Path.GetDirectoryName(currentPath);
				if (currentPath == null) return;
			}

			ChangeFocusedTreeNode(new SFMDirectory(RootDirectory.ElementPath));
		}

		private void StartNewSearch (string request, SearchTree.SearchType searchType, bool considerContent, bool caseSensetive, bool isRegularExpression) {
			while (SearchTreeInstance.IsSearching) {
				SearchTreeInstance.EndSearch();
			}
			
			SetSearchArguments(request, searchType, considerContent, caseSensetive, isRegularExpression);

			if (! (FocusedTreeNode is SearchNode)) {
				directoryBeforeSearch = new SFMDirectory(FocusedTreeNode.Value.ElementPath);

				if (!FileSystemTree.Contains(FocusedTreeNode)) {
					FocusedTreeNode.Tree.Dispose();
				}
			}

			FocusedTreeNode.DisposeObservableCollections();
			FocusedTreeNode = SearchTreeInstance.Root;

			SearchTreeInstance.StartSearch();
		}

		private void SetSearchArguments (string request, SearchTree.SearchType searchType, bool considerContent, bool caseSensetive, bool isRegularExpression) {
			var args = new SearchTree.SearchArguments() {
				requestString = request,
				considerFilesContent = considerContent,
				searchType = searchType,
				caseSensetive = caseSensetive,
				isRegularExpression = isRegularExpression
			};

			if (FocusedTreeNode is SearchNode) {
				args.searchRootPath = SearchTreeInstance.Arguments.searchRootPath;
			} else {
				args.searchRootPath = FocusedTreeNode.Value.ElementPath;
			}

			SearchTreeInstance.SetArguments(args);
		}

		private void EndSearch () {
			if (SearchTreeInstance.IsSearching) {
				SearchTreeInstance.EndSearch();
			}
			if (FocusedTreeNode is SearchNode) {
				ChangeFocusedTreeNode(directoryBeforeSearch);
			}
		}

		#region Properties
		private ObservableCollection<SFMDirectory> _AvailableDrives;
		public ObservableCollection<SFMDirectory> AvailableDrives {
			get { return _AvailableDrives; }
			set { SetProperty(ref _AvailableDrives, value); }
		}

		private SFMDirectory _RootDirectory;
		public SFMDirectory RootDirectory {
			get { return _RootDirectory; }
			set {
				SetProperty(ref _RootDirectory, value);

				FileSystemTree?.Dispose();
				FileSystemTree = FileSystemFacade.Instance.CreateFileSystemTree(RootDirectory);
				FocusedTreeNode = FileSystemFacade.Instance.CreateFileSystemTree(new SFMDirectory(FileSystemTree.Root.Value.ElementPath)).Root;
			}
		}
		
		private DynamicFileSystemTree _FileSystemTree;
		public DynamicFileSystemTree FileSystemTree {
			get { return _FileSystemTree; }
			set { SetProperty(ref _FileSystemTree, value); }
		}

		private FileTreeNode _FocusedTreeNode;
		public FileTreeNode FocusedTreeNode {
			get { return _FocusedTreeNode; }
			set { 
				if (FocusedTreeNode != null && value != null && FocusedTreeNode.Tree != value.Tree && !(FocusedTreeNode is SearchNode)) {
					FocusedTreeNode.Tree.Dispose();
				}

				SetProperty(ref _FocusedTreeNode, value);
				if (FocusedTreeNode == null || FocusedTreeNode.Value == null || FocusedTreeNode is SearchNode) {
					FocusedDirectoryPath = "";
				} else {
					FocusedDirectoryPath = FocusedTreeNode.Value.ElementPath;
				}
			}
		}

		public string FocusedDirectoryPath {
			get { return (FocusedTreeNode is SearchNode)? "" :FocusedTreeNode.Value.ElementPath; }
			set {
				if (FocusedTreeNode is SearchNode || FocusedTreeNode == null || value != FocusedTreeNode.Value.ElementPath) {
					HandleFocusedDirectoryPathChange(value);
				}

				InvokePropertyChanged();
			}
		}

		private SFMDirectory directoryBeforeSearch;

		private SearchTree _SearchTreeInstance;
		private SearchTree SearchTreeInstance {
			get {
				if (_SearchTreeInstance == null) {
					_SearchTreeInstance = new SearchTree();
				}
				return _SearchTreeInstance;
			}
		}

		public string ActionOnSearchCommand {
			get { return (SearchTreeInstance.IsSearching)? "Stop" : "Search"; }
			set { InvokePropertyChanged(); }
		}
		#endregion

		#region Commands
		public ICommand OpenElement {
			get => new ViewModelCommand(
				(arg) => {
					if (!(arg is FileTreeNode selectedNode)) return;

					if (selectedNode.Value.ElementType == FileSystemFacade.ElementType.File) {
						// runs file in default application
						System.Diagnostics.Process.Start(selectedNode.Value.ElementPath);
						return;
					}

					if (!FileSystemTree.Contains(selectedNode)) {
						FocusedTreeNode.Tree.Dispose();
					}

					ChangeFocusedTreeNode(selectedNode.Value);
				},
				(arg) => true
			);
		}

		public ICommand OpenInSFMTextEditor {
			get => new ViewModelCommand(
				(arg) => {
					if (arg is FileTreeNode targetNode && targetNode.Value != null && targetNode.Value is SFMFile) {
						PageBoundManager.Instance.CreatePageInNewWindow<TextEditorPage>("SFM Text Editor", targetNode.Value as SFMFile);
					}
				},

				(arg) =>  arg is FileTreeNode targetNode && targetNode.Value != null && targetNode.Value is SFMFile
			);
		}

		public ICommand SearchElement {
			get => new ViewModelCommand(
				(arg) => {
					if (!(arg is object[])) { return; }
					if (((object[])arg)[0].ToString() == "Stop_search" && SearchTreeInstance.IsSearching) {
						EndSearch();
						return;
					}

					object[] parameters = (object[])arg;

					string request = ((string)parameters[1]).Trim();
					bool recursive = (bool)parameters[3];
					bool considerContent = (bool)parameters[4];
					bool caseSensetive = (bool)parameters[5];
					bool isRegularExpression = (bool)parameters[6];
					var searchType = (recursive) ? SearchTree.SearchType.Recursive : SearchTree.SearchType.CurrentDirectory;

					if (request == string.Empty || request == "") {
						EndSearch();
						return;
					}

					StartNewSearch(request, searchType, considerContent, !caseSensetive, isRegularExpression);
				},
				(arg) => true
			);
		}

		private List<IFileSystemElement> ExtractSelectedElements (object obj) {
			var selectedElements = new List<IFileSystemElement>();
			if (obj is IEnumerable objEnumerable) {
				foreach (var node in objEnumerable) {
					if (node is FileTreeNode treeNode && treeNode.Value != null) {
						selectedElements.Add(treeNode.Value);
					} else if (node is IFileSystemElement fsElement) {
						selectedElements.Add(fsElement);
					}
				}
			}

			if (obj is FileTreeNode singleTreeNode && singleTreeNode.Value != null) {
				selectedElements.Add(singleTreeNode.Value);
			}

			if (obj is IFileSystemElement singleElement) {
				selectedElements.Add(singleElement);
			}

			return selectedElements;
		}

		public ICommand UpdateCommand {
			get => new ViewModelCommand(
				(arg) => {
					if (!(FocusedTreeNode is SearchNode)) {
						ChangeFocusedTreeNode(new SFMDirectory(FocusedTreeNode.Value.ElementPath));
					}
				},
				(arg) => ExtractSelectedElements(arg).Count == 0
			);
		}

		public ICommand CreateFolder {
			get => new ViewModelCommand(
				(arg) => {
					if (!(FocusedTreeNode is SearchNode)) {
						FileSystemFacade.Instance.CreateNewDirectory(FocusedTreeNode.Value.ElementPath);
					}
				},
				(arg) => ExtractSelectedElements(arg).Count == 0
			);
		}

		public ICommand CreateFile {
			get => new ViewModelCommand(
				(arg) => {
					if (!(FocusedTreeNode is SearchNode)) {
						FileSystemFacade.Instance.CreateNewFile(FocusedTreeNode.Value.ElementPath);
					}
				},
				(arg) => ExtractSelectedElements(arg).Count == 0
			);
		}

		public ICommand Copy {
			get => new ViewModelCommand(
				(arg) => {
					FileSystemFacade.Instance.ReleaseCuttedNodes();
					List<IFileSystemElement> selectedElements = ExtractSelectedElements(arg);
					if (selectedElements.Count == 0) return;

					FileSystemFacade.Instance.SendElementsToClipboard(selectedElements);
				},
				(arg) => ExtractSelectedElements(arg).Count > 0
			);
		}

		public ICommand Cut {
			get => new ViewModelCommand(
				(arg) => {
					FileSystemFacade.Instance.ReleaseClipboard();
					List<FileTreeNode> nodes = new List<FileTreeNode>();
					if (arg is IEnumerable selectedItems) {
						foreach (var item in selectedItems) {
							if (item is FileTreeNode selectedNode) {
								nodes.Add(selectedNode);
							}
						}
					}

					if (arg is FileTreeNode singleNode) {
						nodes.Add(singleNode);
					}

					FileSystemFacade.Instance.SetCuttedNodes(nodes.ToArray());
				},
				(arg) => ExtractSelectedElements(arg).Count > 0
			);
		}

		public ICommand Paste {
			get => new ViewModelCommand(
				(arg) => {
					string targetPath = null;
					var elements = ExtractSelectedElements(arg);

					if (arg is string message && message == "InspectorPaste" && !(FocusedTreeNode is SearchNode)) {
						targetPath = FocusedTreeNode.Value.ElementPath;
					} else if (elements.Count == 1 && elements[0].ElementType == FileSystemFacade.ElementType.Directory) {
						targetPath = elements[0].ElementPath;
					}
					if (targetPath == null) return;

					if (FileSystemFacade.Instance.HasCuttedNodes()) {
						FileSystemFacade.Instance.MoveCuttedElementsTo(new SFMDirectory(targetPath));
					}
					if (FileSystemFacade.Instance.ClipboardContainsElementPath()) {
						FileSystemFacade.Instance.PasteFromClipboardToDirectory(new SFMDirectory(targetPath));
					}
				},
				(arg) => {
					if (!FileSystemFacade.Instance.ClipboardContainsElementPath()
						&& !FileSystemFacade.Instance.HasCuttedNodes()) return false;

					if (arg is string message && message == "InspectorPaste") {
						return !(FocusedTreeNode is SearchNode);
					} else {
						return ExtractSelectedElements(arg).Count == 1;
					}
				}
			);
		}

		public ICommand InitiateRename {
			get => new ViewModelCommand(
				(arg) => {
					if (arg is IEnumerable selectedItems) {
						foreach (var item in selectedItems) {
							if (item is FileTreeNode selectedNode) {
								FileSystemFacade.Instance.SetRenamingNode(selectedNode);
							}
						}						
					}

					if (arg is FileTreeNode singleNode) {
						FileSystemFacade.Instance.SetRenamingNode(singleNode);
					}
				},
				(arg) => ExtractSelectedElements(arg).Count == 1
			);
		}

		public ICommand RenamingFault {
			get => new ViewModelCommand(
				(arg) => {
					if (FileSystemFacade.Instance.CurrentRenamingNode != null) {
						if (FileSystemTree.Contains(FileSystemFacade.Instance.CurrentRenamingNode)) {
							var rootDirectory = new SFMDirectory(FileSystemTree.Root.Value.ElementPath);
							FileSystemTree = FileSystemFacade.Instance.CreateFileSystemTree(rootDirectory);
						} else if (!(FocusedTreeNode is SearchNode)) {
							ChangeFocusedTreeNode(new SFMDirectory(FocusedTreeNode.Value.ElementPath));
						}
					}
					FileSystemFacade.Instance.SetRenamingNode(null);
				},
				(arg) => true
			);
		}

		public ICommand RenamingSuccess {
			get => new ViewModelCommand(
				(arg) => {
					FileTreeNode renamingNode = FileSystemFacade.Instance.CurrentRenamingNode;
					if (arg is string nwName && renamingNode != null) {
						if (!FileSystemFacade.Instance.RenameCurrentRenamingElement(nwName)) {
							if (FileSystemTree.Contains(renamingNode)) {
								var rootDirectory = new SFMDirectory(FileSystemTree.Root.Value.ElementPath);
								FileSystemTree = FileSystemFacade.Instance.CreateFileSystemTree(rootDirectory);
							} else if (!(FocusedTreeNode is SearchNode)) {
								ChangeFocusedTreeNode(new SFMDirectory(FocusedTreeNode.Value.ElementPath));
							}
						}
						FileSystemFacade.Instance.SetRenamingNode(null);
					}
				},
				(arg) => FileSystemFacade.Instance.CurrentRenamingNode != null
			);
				
		}

		public ICommand Remove {
			get => new ViewModelCommand(
				(arg) => {
					List<(string, string)> forbiddedElements = new List<(string, string)>();

					List<IFileSystemElement> selectedElements = ExtractSelectedElements(arg);
					if (selectedElements.Count == 0) return;

					foreach (var element in selectedElements) {
						try {
							if (element.ElementType == FileSystemFacade.ElementType.Directory) {
								Directory.Delete(element.ElementPath, true);
							} else {
								File.Delete(element.ElementPath);
							}
						} catch (Exception e) {
							forbiddedElements.Add((element.ElementName, e.Message.Trim()));
						}
					}

					if (forbiddedElements.Count > 0) {
						StringBuilder message = new StringBuilder();
						message.Append("Next elements can't be deleted: \n");
						foreach (var element in forbiddedElements) {
							message.Append($"{element.Item1}  ({element.Item1}) \n");
						}
						MessageBox.Show(message.ToString());
					}
				},
				(arg) => ExtractSelectedElements(arg).Count > 0
			);
		}
		#endregion
	}
}
