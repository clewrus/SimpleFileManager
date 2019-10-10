using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Windows.Controls;

using SimpleFM.Pages;

namespace SimpleFM.ViewModels {
	public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged {
		public MainWindowViewModel (FooterViewModel leftFooterVM, FooterViewModel rightFooterVM) {
			LeftFooterViewModel = leftFooterVM;
			RightFooterViewModel = rightFooterVM;

			LeftFramePage = new FileManagerPage();
			RightFramePage = new FileManagerPage();
		}

		#region LeftFrameProperties

		private Page _LeftFramePage;
		public Page LeftFramePage {
			get { return _LeftFramePage; }
			set { SetProperty(ref _LeftFramePage, value); }
		}
		
		public FooterViewModel LeftFooterViewModel { get; private set; }
		#endregion

		#region RightFrameProperties

		private Page _RightFramePage;
		public Page RightFramePage {
			get { return _RightFramePage; }
			set { SetProperty(ref _RightFramePage, value); }
		}
		
		public FooterViewModel RightFooterViewModel { get; private set; }
		#endregion
	}
}
