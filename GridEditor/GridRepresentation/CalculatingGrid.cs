using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.GridRepresentation {
	public class CalculatingGrid : CellGrid {
		public CalculatingGrid (int width, int height) : base (width, height) {
		
		}

		protected override Cell CreateCell () {
			return new ParsedCell();
		}
	}
}
