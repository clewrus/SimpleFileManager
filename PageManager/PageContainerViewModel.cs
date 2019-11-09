using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleFM.ViewModels {
	public class PageContainerViewModel : INotifyPropertyChanged {		
		public PageContainerViewModel (Page initialPage, string windowNamePrefix) {
			this.namePrefix = windowNamePrefix;
			SourcePage = initialPage;
		}

		private void TitleChangedHandler (object sender, EventArgs e) {
			WindowName = SourcePage.Title;
		}

		private Page _SourcePage;
		public Page SourcePage {
			get => _SourcePage;
			set {
				var titleDescriptor = DependencyPropertyDescriptor.FromProperty(Page.TitleProperty, typeof(Page));
				titleDescriptor.RemoveValueChanged(_SourcePage, TitleChangedHandler);

				_SourcePage = value;
				WindowName = SourcePage.Title;

				titleDescriptor.AddValueChanged(_SourcePage, TitleChangedHandler);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SourcePage"));
			}
		}

		private string _WindowName;
		public string WindowName {
			get => $"{namePrefix} - {_WindowName}";
			set {
				_WindowName = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WindowName"));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private string namePrefix;
	}
}
