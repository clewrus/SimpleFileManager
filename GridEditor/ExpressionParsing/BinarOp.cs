using SimpleFM.GridEditor.GridRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.ExpressionParsing {
	public class BinarOp : Operation {
		public BinarOp (OperationToken.Operation opName, OpRand leftOpRand, OpRand rightOpRand, out ParserError error) {
			error = ParserError.Empty;

			this.OpName = opName;
			this.LeftOpRand = leftOpRand;
			this.RightOpRand = rightOpRand;
		}

		public override void Validate (out ParserError error) {
			LeftOpRand.Validate(out error);
			if (!error.IsEmpty) return;

			RightOpRand.Validate(out error);
		}

		public override object GetValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			error = ParserError.Empty;
			return null;
		}

		public override string ToString () {
			return $"BinOp {OpName}: {LeftOpRand}  {RightOpRand}";
		}

		public OperationToken.Operation OpName { get; private set; }
		public OpRand LeftOpRand { get; private set; }
		public OpRand RightOpRand { get; private set; }
	}
}
