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
