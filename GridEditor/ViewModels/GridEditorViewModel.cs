using SimpleFM.Common.ViewModels;
using SimpleFM.FileManager.ModelCovers;
using SimpleFM.GridEditor.GridRepresentation;
using SimpleFM.ModelCovers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.ViewModels {
	public class GridEditorViewModel : ViewModelBase, INotifyPropertyChanged {
		public GridEditorViewModel (SFMFile targetFile) {
			GridRepresentation = new HistoryCalculatingGrid(30, 50);
		}


		private HistoryCellGrid _GridRepresentation;
		public HistoryCellGrid GridRepresentation {
			get => _GridRepresentation;
			set => SetProperty(ref _GridRepresentation, value);
		}
	}
}
