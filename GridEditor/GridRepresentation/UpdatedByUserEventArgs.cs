using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.GridRepresentation {
	public class UpdatedByUserEventArgs : EventArgs {
		public UpdatedByUserEventArgs (Cell updatedCell) {
			this.UpdatedCell = updatedCell;
		}

		public Cell UpdatedCell { get; private set; }
	}
}
