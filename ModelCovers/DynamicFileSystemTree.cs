using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.ModelCovers {
	public class DynamicFileSystemTree {
		protected DynamicFileSystemTree () { }

		internal DynamicFileSystemTree (SFMDirectory root) {
			Root = new FileTreeNode(this, null, root);
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

		public void ReleaseTree () {
			Root.SetUpdating(false);
			Root.DisposeWatcher();
			Root.ClearObservableCollections();
			GC.Collect();
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
		public FileTreeNode Root { get; protected set; }
	}
}
