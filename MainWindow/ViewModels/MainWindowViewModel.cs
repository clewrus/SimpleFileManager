using System.ComponentModel;
using System.Windows.Controls;

using System.Windows.Input;
using SimpleFM.Common.Commands;
using System.Windows;
using SimpleFM.Common.ViewModels;
using SimpleFM.FileManager.Pages;
using SimpleFM.PageManager;
using SimpleFM.GridEditor.Pages;
using System;

namespace SimpleFM.ViewModels {
	public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged {
		public MainWindowViewModel (FooterViewModel leftFooterVM, FooterViewModel rightFooterVM) {
			LeftFooterViewModel = leftFooterVM;
			RightFooterViewModel = rightFooterVM;

			LeftFramePage = new FileManagerPage();
			RightFramePage = new FileManagerPage();

			fileManagerLeftPage = LeftFramePage;
			fileManagerRightPage = RightFramePage;

			PageBoundManager.Instance.PageBindingChange += PageBindingChangeHandler;
		}

		private void PageBindingChangeHandler (object sender, PageBindingChangeEventArgs e) {
			if (e.action == PageBindingChangeEventArgs.ActionType.Bind) {
				if (e.targetSpotIndex == 0) {
					if (LeftFramePage != fileManagerLeftPage) {
						string title = (LeftFramePage is GridEditorPage) ? "Grid editor" : "Text editor";
						PageBoundManager.Instance.TryUnBound(LeftFramePage, title);
					}
					LeftFramePage = e.page;
				} else {
					if (RightFramePage != fileManagerRightPage) {
						string title = (RightFramePage is GridEditorPage) ? "Grid editor" : "Text editor";
						PageBoundManager.Instance.TryUnBound(RightFramePage, title);
					}
					RightFramePage = e.page;
				}
			}

			if (e.action == PageBindingChangeEventArgs.ActionType.Unbind) {
				if (LeftFramePage == e.page) {
					LeftFramePage = fileManagerLeftPage;
				} else if (RightFramePage == e.page) {
					RightFramePage = fileManagerRightPage;
				}
			}

			e.handled = true;
		}

		private Page fileManagerLeftPage;

		private Page _LeftFramePage;
		public Page LeftFramePage {
			get { return _LeftFramePage; }
			set { SetProperty(ref _LeftFramePage, value); }
		}

		private Page fileManagerRightPage;

		private Page _RightFramePage;
		public Page RightFramePage {
			get { return _RightFramePage; }
			set { SetProperty(ref _RightFramePage, value); }
		}


		public FooterViewModel LeftFooterViewModel { get; private set; }
		public FooterViewModel RightFooterViewModel { get; private set; }

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
