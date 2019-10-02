using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace SimpleFM.ViewModels {
	public class ViewModelBase : INotifyPropertyChanged, INotifyPropertyChanging {
		protected void SetProperty<T> (ref T storage, T value, [CallerMemberName] string caller = "") {
			if (!EqualityComparer<T>.Default.Equals(storage, value)) {
				if (PropertyChanging != null) {
					PropertyChanging(this, new PropertyChangingEventArgs(caller));
				}

				storage = value;

				if (PropertyChanged != null) {
					PropertyChanged(this, new PropertyChangedEventArgs(caller));
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public event PropertyChangingEventHandler PropertyChanging;
	}
}
