using SimpleFM.Common.Commands;
using SimpleFM.FileManager.ModelCovers;
using SimpleFM.GridEditor.ViewModels;
using SimpleFM.PageManager;
using System.Windows.Controls;


namespace SimpleFM.GridEditor.Pages {
	public partial class GridEditorPage : Page {
		public GridEditorPage () {
			InitializeComponent();
			this.DataContext = new GridEditorViewModel();

			SetBoundManagerCommands();
		}

		public GridEditorPage (SFMFile targetFile) {
			InitializeComponent();
			this.DataContext = new GridEditorViewModel(targetFile);

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
