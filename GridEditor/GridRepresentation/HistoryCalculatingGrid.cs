using SimpleFM.FileManager.ModelCovers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.GridRepresentation {
	public class HistoryCalculatingGrid : HistoryCellGrid {
		public HistoryCalculatingGrid (int width, int height) : base (width, height) {

		}

		public HistoryCalculatingGrid (SFMFile targetFile) : base(targetFile) { }
		
		protected override CellGrid CreateGrid (int width, int height) {
			return new CalculatingGrid(width, height);
		}
	}
}
