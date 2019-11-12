using SimpleFM.FileManager.ModelCovers;
using SimpleFM.GridEditor.ViewModels;
using System.Windows.Controls;


namespace SimpleFM.GridEditor.Pages {
	public partial class GridEditorPage : Page {
		public GridEditorPage () {
			InitializeComponent();
			this.DataContext = new GridEditorViewModel();
		}

		public GridEditorPage (SFMFile targetFile) {
			InitializeComponent();
			this.DataContext = new GridEditorViewModel(targetFile);
		}
	}
}
