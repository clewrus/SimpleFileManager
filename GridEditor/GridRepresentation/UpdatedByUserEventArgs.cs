using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.GridRepresentation {
	public class UpdatedByUserEventArgs : EventArgs {
		public UpdatedByUserEventArgs (Cell updatedCell) {
			this.UpdatedCell = updatedCell;
			this.Type = ChangeType.CellChanged;
		}

		public UpdatedByUserEventArgs (int width, int height) {
			this.Size = (width, height);
			this.Type = ChangeType.SizeChanged;
		}

		public enum ChangeType { None, CellChanged, SizeChanged }

		public ChangeType Type { get; private set; }
		public Cell UpdatedCell { get; private set; }
		public (int, int) Size { get; private set; }
	}
}
