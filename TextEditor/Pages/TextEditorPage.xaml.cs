using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SimpleFM.Common.Commands;
using SimpleFM.FileManager.ModelCovers;
using SimpleFM.ModelCovers;
using SimpleFM.PageManager;
using SimpleFM.ViewModels;

namespace SimpleFM.TextEditor.Pages {
	/// <summary>
	/// Логика взаимодействия для FileEditorPage.xaml
	/// </summary>
	public partial class TextEditorPage : Page {
		public TextEditorPage (SFMFile targetFile) {
			InitializeComponent();
			this.DataContext = new TextEditorViewModel(targetFile);

			SetBoundManagerCommands();
		}

		private void SetBoundManagerCommands () {
			BoundToLeftItem.Command = new ViewModelCommand(
				(arg) => PageBoundManager.Instance.TryBoundToMainWindow(0, this),
				(arg) => true
			);

			BoundToRightItem.Command = new ViewModelCommand(
				(arg) => PageBoundManager.Instance.TryBoundToMainWindow(1, this),
				(arg) => true
			);

			UnboundItem.Command = new ViewModelCommand(
				(arg) => PageBoundManager.Instance.TryUnBound(this, "Grid editor"),
				(arg) => !PageBoundManager.Instance.HasOwnWindow(this)
			);

			CloseItem.Command = new ViewModelCommand(
				(arg) => PageBoundManager.Instance.TryClosePage(this),
				(arg) => true
			);
		}
	}
}
