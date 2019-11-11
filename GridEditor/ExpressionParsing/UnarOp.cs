using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.ExpressionParsing {
	public class UnarOp : Operation {
		public UnarOp (OperationToken.Operation opName, OpRand argument, out ParserError error) {
			error = ParserError.Empty;
			this.OpName = opName;
			this.Argument = argument;

			if (opName == OperationToken.Operation.If) {
				CheckIfArgument(out error);
			}
		}

		public override void Validate (out ParserError error) {
			Argument.Validate(out error);
		}

		private void CheckIfArgument (out ParserError error) {
			error = ParserError.Empty;

			var argList = Argument as OpRandsList;
			if (argList == null || argList.OpRands.Count != 3) {
				error = new ParserError("If must have 3 arguments: If( bool, v1, v2)");
			}
		}

		public override string ToString () {
			return $"UnarOp {OpName}: {Argument}";
		}

		public OperationToken.Operation OpName { get; private set; }
		public OpRand Argument { get; private set; }
	}
}
