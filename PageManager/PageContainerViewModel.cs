using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SimpleFM.ViewModels {
	public class PageContainerViewModel : INotifyPropertyChanged {
		
		public PageContainerViewModel (Page initialPage, string windowName) {
			SourcePage = initialPage;
			this.WindowName = windowName;
		}

		private Page _SourcePage;
		public Page SourcePage {
			get => _SourcePage;
			set {
				_SourcePage = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SourcePage"));
			}
		}

		public string WindowName { get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
