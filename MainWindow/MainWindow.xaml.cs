using System.Windows;
using SimpleFM.FileManager.ModelCovers;
using SimpleFM.GridEditor.Pages;
using SimpleFM.PageManager;
using SimpleFM.ViewModels;

namespace SimpleFM.MainWindow {
	public partial class MainWindow : Window {
		public MainWindow () {
			InitializeComponent();
			InitializeDataContext();

			PageBoundManager.Instance.CreatePageInNewWindow<GridEditorPage>("SFM Text Editor", new SFMFile("C:\\TestFolder\\GridEditorTest"));
		}

		private void InitializeDataContext () {
			var leftFooterContext = new FooterViewModel();
			var rightFooterContext = new FooterViewModel();

			this.LeftFooter.DataContext = leftFooterContext;
			this.RightFooter.DataContext = rightFooterContext;

			this.DataContext = new MainWindowViewModel(leftFooterContext, rightFooterContext);
		}
	}
}
