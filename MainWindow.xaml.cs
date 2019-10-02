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

using SimpleFM.ViewModels;

namespace SimpleFM {
	public partial class MainWindow : Window {
		public MainWindow () {
			InitializeComponent();
			InitializeDataContext();
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
