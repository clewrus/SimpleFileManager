using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SimpleFM.PageManager {
	public class PageBindingChangeEventArgs : EventArgs {
		public Page page;
		public int targetSpotIndex;
		public ActionType action;
		public bool handled;

		public enum ActionType { None, Bind, Unbind }
	}
}
