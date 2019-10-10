using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.ModelCovers.TextEditor {
	public class TagTreeNode : INotifyPropertyChanged {
		private string _CommonName;
		public string CommonName {
			get => _CommonName;
			set {
				_CommonName = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CommonName"));
			}
		}

		private ObservableCollection<SimpleHtmlTag> _Tags;
		public ObservableCollection<SimpleHtmlTag> Tags {
			get => _Tags;
			set {
				_Tags = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Tags"));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
