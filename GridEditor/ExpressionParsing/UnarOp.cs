using SimpleFM.GridEditor.GridRepresentation;
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

		public override void Validate (out ParserError error) {
			Argument.Validate(out error);
		}

		public override object GetValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			try {
				switch (OpName) {
					case OperationToken.Operation.If: return GetIfValue(childCells, out error);
					case OperationToken.Operation.Plus: return GetPlusValue(childCells, out error);
					case OperationToken.Operation.Minus: return GetMinusValue(childCells, out error);
					case OperationToken.Operation.Not: return GetNotValue(childCells, out error);
					default: {
						error = new ParserError($"Unexpected UnarOperation: {OpName.ToString()}");
						return null;
					}
				}
			} catch (Exception e) {
				error = new ParserError(e.Message);
				return null;
			}
		}

		private object GetIfValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			error = ParserError.Empty;
			var opRands = ((OpRandsList)Argument).OpRands;

			object[] value = new object[3];
			for (int i = 0; i < 3; i++) {
				value[i] = opRands[i].GetValue(childCells, out error);
				if (!error.IsEmpty) return null;
			}

			if (value[0] is bool ifValue) {
				return (ifValue) ? value[1] : value[2];
			}

			error = new ParserError($"First IF argument must has logic type.");
			return null;
		}

		private object GetPlusValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			object argValue = Argument.GetValue(childCells, out error);
			if (!error.IsEmpty) return null;

			if (argValue is Decimal decValue) {
				return +decValue;
			}

			error = new ParserError($"Unar {OpName.ToString()} operation has wrong argument type");
			return null;
		}

		private object GetMinusValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			object argValue = Argument.GetValue(childCells, out error);
			if (!error.IsEmpty) return null;

			if (argValue is Decimal decValue) {
				return -decValue;
			}

			error = new ParserError($"Unar {OpName.ToString()} operation has wrong argument type");
			return null;
		}

		private object GetNotValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			object argValue = Argument.GetValue(childCells, out error);
			if (!error.IsEmpty) return null;

			if (argValue is bool boolValue) {
				return !boolValue;
			}

			error = new ParserError($"Unar {OpName.ToString()} operation has wrong argument type");
			return null;
		}

		public OperationToken.Operation OpName { get; private set; }
		public OpRand Argument { get; private set; }
	}
}
