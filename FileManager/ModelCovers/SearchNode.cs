using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.FileManager.ModelCovers {
	public class SearchNode : FileTreeNode, INotifyPropertyChanged, IDisposable {
		internal SearchNode (SearchTree tree) : base(tree) {
			CreateChildNodesCollection();
		}

		private SearchNode (SearchTree tree, SearchNode parent, IFileSystemElement element) : base(tree) {
			Parent = parent;
			Value = element;
		}

		protected override void Dispose (Boolean disposing) {
			base.Dispose(disposing);
		}

		internal async void AddElementsInDispatcher (IFileSystemElement[] element) {
			await App.Current.Dispatcher.BeginInvoke((Action)(() => {
				foreach (var elem in element) {
					if (elem.ElementType == FileSystemFacade.ElementType.Directory) {
						ChildDirectoryNodes.Add(new SearchNode(Tree as SearchTree, this, elem));
					} else {
						ChildFileNodes.Add(new SearchNode(Tree as SearchTree, this, elem));
					}
				}
			}));
		}
	}
}
