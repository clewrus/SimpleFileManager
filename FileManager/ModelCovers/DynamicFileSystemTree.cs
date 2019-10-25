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
	public class DynamicFileSystemTree : IDisposable {
		protected DynamicFileSystemTree () { }

		internal DynamicFileSystemTree (SFMDirectory root) {
			Root = new FileTreeNode(this, null, root);
		}

		public void Dispose () {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose (bool disposing) {
			if (Root != null) {
				Root.SetUpdating(false);
				Root.Dispose();
			}
			Root = null;
			if (_ExtraRootNode != null) {
				_ExtraRootNode.Dispose();
			}
		}

		~DynamicFileSystemTree () {
			Dispose(false);
		}

		public static void LoadNodesChildren (FileTreeNode targetNode) {
			targetNode.LoadChildren();
		}

		public void RefreshRoot () {
			Root.UpdateSelf();
		}

		public void SetRenamingNode (FileTreeNode nwRenamingNode) {
			if (CurrentRenamingNode != null) {
				CurrentRenamingNode.NotRenaming = true;
			}
			if (nwRenamingNode != null) {
				CurrentRenamingNode = nwRenamingNode;
				CurrentRenamingNode.NotRenaming = false;
			}
		}

		public bool Contains (FileTreeNode node) {
			if (!object.ReferenceEquals(node.Tree, this)) return false;

			var curNode = node;
			while (curNode.Parent != null) {
				var parentNode = curNode.Parent;
				if (!parentNode.ChildDirectoryNodes.Contains(curNode) && !parentNode.ChildFileNodes.Contains(curNode)) {
					return false;
				}
				curNode = parentNode;
			}

			return object.ReferenceEquals(curNode, Root);
		}

		internal void OnTreeNodeDelete (FileTreeNode node) {
			FileSystemFacade.Instance.OnTreeNodeDelete(node);
		}

		private FileTreeNode _ExtraRootNode;
		public FileTreeNode ExtraRootNode {
			get {
				if (_ExtraRootNode == null) {
					_ExtraRootNode = new FileTreeNode(this, Root);
				}
				return _ExtraRootNode;
			}
		}

		private FileTreeNode CurrentRenamingNode { get; set; }
		public virtual FileTreeNode Root { get; protected set; }
	}
}
