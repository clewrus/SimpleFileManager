using SimpleFM.GridEditor.ExpressionParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.GridRepresentation {
	public class ParsedCell : Cell {
		public ParsedCell() : base () {
			this.ChangedByUser += ChangedByUserHandler;
		}

		private void ChangedByUserHandler (object sender, EventArgs e) {
			var tokens = Tokenizer.Instance.TokenizeString(ExpressionStr);
		}
	}
}
