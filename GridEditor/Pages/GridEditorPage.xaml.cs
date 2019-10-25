using SimpleFM.FileManager.ModelCovers;
using SimpleFM.GridEditor.ViewModels;
using SimpleFM.ModelCovers;
using SimpleFM.ViewModels;
using System.Windows.Controls;


namespace SimpleFM.GridEditor.Pages {
	public partial class GridEditorPage : Page {
		public GridEditorPage (SFMFile targetFile) {
			InitializeComponent();
			this.DataContext = new GridEditorViewModel(targetFile);
		}
	}
}
