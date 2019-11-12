using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleFM.PageManager {
	public class PageBoundManager {
		private PageBoundManager () {
			pageContainers = new Dictionary<Page, PageContainerWindow>();
		}

		public Page CreatePageInNewWindow<PageType> (string windowTitle, params object[] pageConstructorArgs) where PageType : Page {
			var nwPage = (PageType)Activator.CreateInstance(typeof(PageType), pageConstructorArgs);
			var nwContainerWindow = new PageContainerWindow(nwPage, windowTitle);

			pageContainers.Add(nwPage, nwContainerWindow);
			nwContainerWindow.Show();

			return nwPage;
		}

		public bool HasOwnWindow (Page page) {
			return pageContainers.ContainsKey(page) && pageContainers[page] != null;
		}



		public bool TryUnBound (Page page, string windowTitle) {
			if (HasOwnWindow(page)) {
				return false;
			}

			var eventArgs = new PageBindingChangeEventArgs() {
				page = page,
				action = PageBindingChangeEventArgs.ActionType.Unbind
			};

			PageBindingChange?.Invoke(this, eventArgs);

			var nwContainerWindow = new PageContainerWindow(page, windowTitle);
			pageContainers.Add(page, nwContainerWindow);
			nwContainerWindow.Show();

			return eventArgs.handled;
		}

		public bool TryBoundToMainWindow (int sideIndex, Page page) {
			if (!HasOwnWindow(page)) {
				PageBindingChange?.Invoke(this, new PageBindingChangeEventArgs() {
					page = page,
					action = PageBindingChangeEventArgs.ActionType.Unbind
				});
			}

			var eventArgs = new PageBindingChangeEventArgs() {
				page = page,
				targetSpotIndex = sideIndex,
				action = PageBindingChangeEventArgs.ActionType.Bind
			};

			PageBindingChange?.Invoke(this, eventArgs);

			if (pageContainers.ContainsKey(page) && pageContainers[page] != null) {
				pageContainers[page].Close();
				pageContainers.Remove(page);
			}

			return eventArgs.handled;
		}

		public bool TryClosePage (Page page) {
			if (HasOwnWindow(page)) {
				pageContainers[page].Close();
				return true;
			}

			var eventArgs = new PageBindingChangeEventArgs() {
				page = page,
				action = PageBindingChangeEventArgs.ActionType.Unbind
			};

			PageBindingChange?.Invoke(this, eventArgs);
			return true;
		}

		private static PageBoundManager _Instance;
		public static PageBoundManager Instance {
			get {
				if (_Instance == null) {
					_Instance = new PageBoundManager();
				}
				return _Instance;
			}
		}

		public event EventHandler<PageBindingChangeEventArgs> PageBindingChange;

		private Dictionary<Page, PageContainerWindow> pageContainers;
	}
}
