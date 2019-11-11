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
		}

		public override void Validate (out ParserError error) {
			Argument.Validate(out error);
		}

		public override string ToString () {
			return $"UnarOp {OpName}: {Argument}";
		}

		public OperationToken.Operation OpName { get; private set; }
		public OpRand Argument { get; private set; }
	}
}
