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
using System.Runtime.InteropServices;
using System.Threading;

namespace SimpleFM.ModelCovers {
	public class FileSystemFacade {
		private FileSystemFacade () { }

		internal void OnTreeNodeDelete (FileTreeNode node) {
			ElementDeleted?.Invoke(node, new SFMEventArgs() { element = node.Value });
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

		public void ReleaseClipboard () {
			Clipboard.Clear();
		}

		public void SendElementsToClipboard (List<IFileSystemElement> selectedElements) {
			ReleaseClipboard();

			List<string> selectedElementsPath;
			if (selectedElements == null) {
				selectedElementsPath = new List<string>();
			} else {
				selectedElementsPath = new List<string>(selectedElements.Count);
			}

			foreach (var elem in selectedElements) {
				selectedElementsPath.Add(elem.ElementPath);
			}
			try {
				Clipboard.SetData(DataFormats.FileDrop, selectedElementsPath.ToArray());
			} catch (ExternalException) { } catch (ThreadStateException) { }
		}

		public void SetRenamingNode (FileTreeNode node) {
			CurrentRenamingNode?.Tree.SetRenamingNode(null);
			node?.Tree.SetRenamingNode(node);
			CurrentRenamingNode = node;
		}

		public bool RenameCurrentRenamingElement (string nwName) {
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
				return false;
			}

			return true;
		}

		public void CreateNewDirectory (string parentDirectory) {
			string selectedName = "New directory";
			int counter = 1;
			
			string exeptionMessage = "";
			try {
				while (Directory.Exists(Path.Combine(parentDirectory, selectedName))) {
					selectedName = $"New directory{counter++}";
				}

				Directory.CreateDirectory(Path.Combine(parentDirectory, selectedName));
			} catch (UnauthorizedAccessException e) {
				exeptionMessage += $"\n{e.Message}";
			} catch (IOException e) {
				exeptionMessage += $"\n{e.Message}";
			}

			if (!string.IsNullOrEmpty(exeptionMessage)) {
				MessageBox.Show($"Can't create a directory{exeptionMessage}");
			}
		}

		public void CreateNewFile (string parentDirectory) {
			string selectedName = "New file.txt";
			int counter = 1;

			string exeptionMessage = "";
			try {
				while (File.Exists(Path.Combine(parentDirectory, selectedName))) {
					selectedName = $"New file{counter++}.txt";
				}

				File.Create(Path.Combine(parentDirectory, selectedName)).Close();
			} catch (UnauthorizedAccessException e) {
				exeptionMessage += $"\n{e.Message}";
			} catch (IOException e) {
				exeptionMessage += $"\n{e.Message}";
			}
			
			if (!string.IsNullOrEmpty(exeptionMessage)) {
				MessageBox.Show($"Can't create a file{exeptionMessage}");
			}
		}

		public bool ClipboardContainsElementPath () {
			var dataObject = Clipboard.GetDataObject();
			if (!dataObject.GetDataPresent(DataFormats.FileDrop)) return false;
			string[] sourcePathes = (string[])dataObject.GetData(DataFormats.FileDrop);

			if (sourcePathes == null || sourcePathes.Length == 0) return false;
			foreach (var path in sourcePathes) {
				if (File.Exists(path) || Directory.Exists(path)) {
					return true;
				}
			}

			return false;
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
			if (!Directory.Exists(source.ElementPath)) {
				throw new Exception($"{source.ElementName} directory doesn't exist");
			}

			var operationQueue = new Queue<(string, string)>();
			AddChildElementToCopyQueue(source.ElementPath, target.ElementPath, operationQueue);

			while (operationQueue.Count > 0) {
				var curOp = operationQueue.Dequeue();
				try {
					if (curOp.Item1 == null) {
						Directory.CreateDirectory(curOp.Item2);
					} else {
						File.Copy(curOp.Item1, curOp.Item2);
					}
				} catch (Exception e) {
					exeptionLogs.Add((Path.GetFileName(curOp.Item2), e.Message));
				}
			}
		}

		public bool MoveCuttedElementsTo (SFMDirectory targetDirectory) {
			if (!Directory.Exists(targetDirectory.ElementPath)) {
				throw new Exception($"{targetDirectory.ElementName} directory doesn't exist");
			}

			DeleteRelatedNodesFromCuttedNodes();

			List<(string, string)> errorSummary = new List<(string, string)>();
			foreach (var node in currentCuttedNodes) {
				try {
					if (node.Value.ElementType == ElementType.Directory) {
						Directory.Move(node.Value.ElementPath, Path.Combine(targetDirectory.ElementPath, node.Value.ElementName));
					} else {
						File.Move(node.Value.ElementPath, Path.Combine(targetDirectory.ElementPath, node.Value.ElementName));
					}
				} catch (Exception e) {
					errorSummary.Add((node.Value.ElementName, e.Message));
				}
			}

			ReleaseCuttedNodes();
			if (errorSummary.Count > 0) {
				ShowPastSummery(errorSummary);
				return false;
			}
			return true;
		}

		private void DeleteRelatedNodesFromCuttedNodes () {
			for (int i = 0; i < currentCuttedNodes.Count; i++) {
				for (int j = 0; j < currentCuttedNodes.Count; j++) {
					if (i == j) {
						continue;
					}
					if (IsRelated(currentCuttedNodes[i].Value, currentCuttedNodes[j].Value)) {
						currentCuttedNodes.RemoveAt(j--);
					}
				}
			}
		}

		private bool IsRelated (IFileSystemElement parent, IFileSystemElement child) {
			string parentPath = parent.ElementPath;
			string childPath = child.ElementPath;

			while (childPath != null) {
				if (parentPath == childPath) {
					return true;
				}
				childPath = Path.GetDirectoryName(childPath);
			}

			return false;
		}

		public bool HasCuttedNodes () {
			return currentCuttedNodes != null && currentCuttedNodes.Count > 0;
		}

		public void SetCuttedNodes (FileTreeNode[] cuttedNodes) {
			if (currentCuttedNodes == null) {
				currentCuttedNodes = new List<FileTreeNode>();
			}

			ReleaseCuttedNodes();

			currentCuttedNodes.AddRange(cuttedNodes);
			foreach (var nwCuttedNode in currentCuttedNodes) {
				if (nwCuttedNode != null) {
					nwCuttedNode.IsCutted = true;
				}
			}
		}

		public void ReleaseCuttedNodes () {
			if (currentCuttedNodes == null) {
				currentCuttedNodes = new List<FileTreeNode>();
			}

			foreach (var oldCuttedNode in currentCuttedNodes) {
				if (oldCuttedNode != null) {
					oldCuttedNode.IsCutted = false;
				}
			}

			currentCuttedNodes.Clear();
		}

		private void AddChildElementToCopyQueue (string sourcePath, string targetPath, Queue<(string, string)> queue) {
			string pastedDirectoryPath = Path.Combine(targetPath, Path.GetFileName(sourcePath));
			queue.Enqueue((null, pastedDirectoryPath));

			string[] childFiles = Directory.GetFiles(sourcePath);
			foreach (var childFile in childFiles) {
				queue.Enqueue((childFile, Path.Combine(pastedDirectoryPath, Path.GetFileName(childFile)) ));
			}

			string[] childDirs = Directory.GetDirectories(sourcePath);
			foreach (var childDir in childDirs) {
				AddChildElementToCopyQueue(childDir, pastedDirectoryPath, queue);
			}
		}

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
		public event EventHandler<SFMEventArgs> ElementDeleted;

		public FileTreeNode CurrentRenamingNode { get; private set; }
		private List<FileTreeNode> currentCuttedNodes;
	}
}
