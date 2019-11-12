using System.ComponentModel;
using System.Windows.Controls;

using System.Windows.Input;
using SimpleFM.Common.Commands;
using System.Windows;
using SimpleFM.Common.ViewModels;
using SimpleFM.FileManager.Pages;
using SimpleFM.PageManager;
using SimpleFM.GridEditor.Pages;

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

		public ICommand OpenEmptyGridEditor {
			get => new ViewModelCommand(
				(arg) =>  PageBoundManager.Instance.CreatePageInNewWindow<GridEditorPage>("Grid editor", new object[0])
			);
		}

		public ICommand ShowInfoMessageBox {
			get => new ViewModelCommand(
				(arg) => MessageBox.Show( "This is a file manager \nthat was created for education purpose.\n")
			);
		}

		public ICommand CreaterInfoMessageBox {
			get => new ViewModelCommand(
				(arg) => MessageBox.Show("This project was created by\nOleksii Saitarly (group K-26)")
			);
		}

		public ICommand CustomFileManagerInfoMessageBox {
			get => new ViewModelCommand(
				(arg) => MessageBox.Show("You may open a file by rightclick on it and select\n Open in SFMTextEditor")
			);
		}
	}
}
