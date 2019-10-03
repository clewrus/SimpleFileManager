using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Diagnostics;

namespace SimpleFM.ModelCovers {
	public class FileSystemFacade : INotifyPropertyChanged {
		private FileSystemFacade () { }

		private void SetProperty<T> (ref T storage, T value, [CallerMemberName] string caller = "") {
			if (!EqualityComparer<T>.Default.Equals(storage, value)) {
				PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(caller));

				storage = value;

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
			}
		}

		internal void OnTreeNodeDelete (FileTreeNode node) {
			OnElementDelete?.Invoke(node.Value);
		}

		public DynamicFileSystemTree CreateFileSystemTree (SFMDirectory rootDirectory) {
			return new DynamicFileSystemTree(rootDirectory);
		}

		public SFMDirectory[] AvailableDrives {
			get {
				var allDrives = DriveInfo.GetDrives();
				var availableDrives = new SFMDirectory[allDrives.Length];

				for (int i = 0; i < availableDrives.Length; i++) {
					availableDrives[i] = new SFMDirectory(allDrives[i].RootDirectory.FullName);
				}

				return availableDrives;
			}
		}

		public IFileSystemElement GetElementFromPath (string path) {
			FileAttributes attributes = File.GetAttributes(path);

			if ((attributes & FileAttributes.Directory) == FileAttributes.Directory) {
				return new SFMDirectory(path);
			} else {
				return new SFMFile(path);
			}
		}

		public void SendElementsToClipboard (List<IFileSystemElement> selectedElements) {
			List<string> selectedElementsPath = new List<string>(selectedElements.Count);
			foreach (var elem in selectedElements) {
				selectedElementsPath.Add(elem.ElementPath);
			}

			Clipboard.SetData(DataFormats.FileDrop, selectedElementsPath.ToArray());
		}

		public void SetRenamingNode (FileTreeNode node) {
			CurrentRenamingNode?.Tree.SetRenamingNode(null);
			node?.Tree.SetRenamingNode(node);
			CurrentRenamingNode = node;
		}

		public void RenameCurrentRenamingElement (string nwName) {
			IFileSystemElement requestedElement = CurrentRenamingNode.Value;
			string destPath = Path.Combine(Path.GetDirectoryName(requestedElement.ElementPath), nwName);
			string errorMessage = "";

			try {
				if (requestedElement.ElementType == ElementType.Directory) {
					Directory.Move(requestedElement.ElementPath, destPath);
				} else {
					File.Move(requestedElement.ElementPath, destPath);
				}
			} catch (UnauthorizedAccessException e) {
				errorMessage += e.Message;
			} catch (IOException e) {
				errorMessage += e.Message;
			}

			if (errorMessage != "") {
				MessageBox.Show("Something went wrong: \n" + errorMessage);
			}
		}

		public void CreateNewDirectory (string parentDirectory) {
			string selectedName = "New directory";
			int counter = 1;
			while (Directory.Exists(Path.Combine(parentDirectory, selectedName))) {
				selectedName = $"New directory{counter++}";
			}

			Directory.CreateDirectory(Path.Combine(parentDirectory, selectedName));
		}

		public void CreateNewFile (string parentDirectory) {
			string selectedName = "New file.txt";
			int counter = 1;
			while (Directory.Exists(Path.Combine(parentDirectory, selectedName))) {
				selectedName = $"New file{counter++}.txt";
			}

			File.Create(Path.Combine(parentDirectory, selectedName));
		}

		public void PasteFromClipboardToDirectory (SFMDirectory targetDirectory) {
			string targetPath = targetDirectory.ElementPath;
			var dataObject = Clipboard.GetDataObject();

			if (!dataObject.GetDataPresent(DataFormats.FileDrop)) return;
			string[] sourcePathes = (string[])dataObject.GetData(DataFormats.FileDrop);

			var targetDirectoryCopy = new SFMDirectory(targetDirectory.ElementPath);
			Task.Run(() => {
				List<(string, string)> exeptionLogs = new List<(string, string)>();
				foreach (var path in sourcePathes) {
					CopyPathToSource(path, targetDirectoryCopy, exeptionLogs);
				}
				ShowPastSummery(exeptionLogs);
			});
		}

		private void CopyPathToSource (string path, SFMDirectory targetDirectory, List<(string, string)> exeptionLogs) {
			var element = GetElementFromPath(path);
			if (element.ElementType == ElementType.Directory) {
				CopyDirectoryTo((SFMDirectory)element, targetDirectory, exeptionLogs);
			} else {
				try {
					File.Copy(element.ElementPath, Path.Combine(targetDirectory.ElementPath, element.ElementName));
				} catch (Exception e) {
					exeptionLogs.Add((element.ElementName, e.Message));
				}
			}
		}

		private void ShowPastSummery (List<(string, string)> exeptionLogs) {
			if (exeptionLogs.Count == 0) return;

			var message = new StringBuilder();
			message.Append("Next elements were not copied: \n");
			foreach (var e in exeptionLogs) {
				message.Append($"{e.Item1} ({e.Item2.Trim()})\n");
			}
			MessageBox.Show(message.ToString());
		}

		public void CopyDirectoryTo (SFMDirectory source, SFMDirectory target, List<(string, string)> exeptionLogs) {
			DirectoryInfo sourceInfo = new DirectoryInfo(source.ElementPath);
			if (!sourceInfo.Exists) {
				throw new Exception($"{source.ElementName} directory doesn't exist");
			}

			var directoryPath = Path.Combine(target.ElementPath, source.ElementName);
			if (!Directory.Exists(directoryPath)) {
				try {
					Directory.CreateDirectory(directoryPath);
				} catch (Exception e) {
					exeptionLogs.Add((target.ElementName, e.Message));
					return;
				}
			}

			DirectoryInfo[] childDirectories = sourceInfo.GetDirectories();
			FileInfo[] childFiles = sourceInfo.GetFiles();

			foreach (var fileInfo in childFiles) {
				try {
					fileInfo.CopyTo(Path.Combine(directoryPath, fileInfo.Name));
				} catch (Exception e) {
					exeptionLogs.Add((fileInfo.Name, e.Message));
				}
			}

			foreach (var directoryInfo in childDirectories) {
				var nwSource = new SFMDirectory(directoryInfo.FullName);
				var nwTarget = new SFMDirectory(Path.Combine(target.ElementPath, directoryInfo.Name));
				CopyDirectoryTo(nwSource, nwTarget, exeptionLogs);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public event PropertyChangingEventHandler PropertyChanging;

		public static FileSystemFacade _Instance;
		public static FileSystemFacade Instance {
			get {
				if (_Instance == null) {
					_Instance = new FileSystemFacade();
				}
				return _Instance;
			}
		}

		public enum ElementType { Directory, File }
		public event Action<IFileSystemElement> OnElementDelete;

		public FileTreeNode CurrentRenamingNode { get; private set; }
	}
}
