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
			leftValue = LeftOpRand.GetValue(childCells, out error);
			if (!error.IsEmpty) return null;

			rightValue = RightOpRand.GetValue(childCells, out error);
			if (!error.IsEmpty) return null;

			try {
				switch (OpName) {
					case OperationToken.Operation.Plus: return GetPlusValue(childCells, out error);
					case OperationToken.Operation.Minus: return GetMinusValue(childCells, out error);
					case OperationToken.Operation.Mult: return GetMultValue(childCells, out error);
					case OperationToken.Operation.Divide: return GetDivideValue(childCells, out error);

					case OperationToken.Operation.Mod: return GetModValue(childCells, out error);
					case OperationToken.Operation.Div: return GetDivValue(childCells, out error);

					case OperationToken.Operation.More: return GetMoreValue(childCells, out error);
					case OperationToken.Operation.MoreEq: return GetMoreEqValue(childCells, out error);
					case OperationToken.Operation.Less: return GetLessValue(childCells, out error);
					case OperationToken.Operation.LessEq: return GetLessEqValue(childCells, out error);

					case OperationToken.Operation.Equal: return GetEqualValue(childCells, out error);
					case OperationToken.Operation.UnEqual: return GetUnEqualValue(childCells, out error);

					default: {
						error = new ParserError($"Strange binar operation{OpName.ToString()}");
						return null;
					}
				}
			} catch (Exception e) {
				error = new ParserError(e.Message);
				return null;
			}
		}

		private object GetUnEqualValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			error = ParserError.Empty;
			return !leftValue.Equals(rightValue);
		}

		private object GetEqualValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			error = ParserError.Empty;
			return leftValue.Equals(rightValue);
		}

		private object GetLessEqValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			error = ParserError.Empty;

			if (leftValue is bool || rightValue is bool) {
				error = new ParserError($"{OpName} operation chant have bool arguments");
			}

			if (leftValue is string leftStr && rightValue is string rightStr) {
				return leftStr.Length <= rightStr.Length;
			}

			if (leftValue is string leftStr1 && rightValue is Decimal rightNum1) {
				return leftStr1.Length <= rightNum1;
			}

			if (leftValue is Decimal leftNum && rightValue is Decimal rightNum) {
				return leftNum <= rightNum;
			}

			error = new ParserError($"{OpName} operation has wrong argument types");
			return null;
		}

		private object GetLessValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			error = ParserError.Empty;

			if (leftValue is bool || rightValue is bool) {
				error = new ParserError($"{OpName} operation chant have bool arguments");
			}

			if (leftValue is string leftStr && rightValue is string rightStr) {
				return leftStr.Length < rightStr.Length;
			}

			if (leftValue is string leftStr1 && rightValue is Decimal rightNum1) {
				return leftStr1.Length < rightNum1;
			}

			if (leftValue is Decimal leftNum && rightValue is Decimal rightNum) {
				return leftNum < rightNum;
			}

			error = new ParserError($"{OpName} operation has wrong argument types");
			return null;
		}

		private object GetMoreEqValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			error = ParserError.Empty;

			if (leftValue is bool || rightValue is bool) {
				error = new ParserError($"{OpName} operation chant have bool arguments");
			}

			if (leftValue is string leftStr && rightValue is string rightStr) {
				return leftStr.Length >= rightStr.Length;
			}

			if (leftValue is string leftStr1 && rightValue is Decimal rightNum1) {
				return leftStr1.Length >= rightNum1;
			}

			if (leftValue is Decimal leftNum && rightValue is Decimal rightNum) {
				return leftNum >= rightNum;
			}

			error = new ParserError($"{OpName} operation has wrong argument types");
			return null;
		}

		private object GetMoreValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			error = ParserError.Empty;

			if (leftValue is bool || rightValue is bool) {
				error = new ParserError($"{OpName} operation chant have bool arguments");
			}

			if (leftValue is string leftStr && rightValue is string rightStr) {
				return leftStr.Length > rightStr.Length;
			}

			if (leftValue is string leftStr1 && rightValue is Decimal rightNum1) {
				return leftStr1.Length > rightNum1;
			}

			if (leftValue is Decimal leftNum && rightValue is Decimal rightNum) {
				return leftNum > rightNum;
			}

			error = new ParserError($"{OpName} operation has wrong argument types");
			return null;
		}

		private object GetDivValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			error = ParserError.Empty;
			if (leftValue is Decimal leftNum && rightValue is Decimal rightNum) {
				if (rightNum == 0) {
					error = new ParserError("Omg, it was so predictable...\nBut it's just an error\n Infinities are bad");
					return null;
				}

				bool isNegative = (leftNum < 0) != (rightNum < 0);
				var absValue = Math.Floor(Math.Abs(leftNum / rightNum));
				return (isNegative) ? -absValue : absValue;
			}

			error = new ParserError($"{OpName} operation must have number type arguments:\n" +
				$" {leftValue.GetType().ToString()} and {rightValue.GetType().ToString()} were found.");
			return null;
		}

		private object GetModValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			error = ParserError.Empty;
			if (leftValue is Decimal leftNum && rightValue is Decimal rightNum) {
				if (rightNum == 0) {
					error = new ParserError("Omg, it was so predictable...\nBut it's just an error\n Infinities are bad");
					return null;
				}

				bool isNegative = (leftNum < 0) != (rightNum < 0);
				var absValue = Math.Floor(Math.Abs(leftNum / rightNum));
				var divResult = (isNegative) ? -absValue : absValue;

				return leftNum - divResult * rightNum;
			}

			error = new ParserError($"{OpName} operation must have number type arguments:\n" +
				$" {leftValue.GetType().ToString()} and {rightValue.GetType().ToString()} were found.");
			return null;
		}

		private object GetDivideValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			error = ParserError.Empty;
			if (leftValue is Decimal leftNum && rightValue is Decimal rightNum) {
				if (rightNum == 0) {
					error = new ParserError("Omg, it was so predictable...\nBut it's just an error\n Infinities are bad");
					return null;
				}

				return leftNum / rightNum;
			}

			error = new ParserError($"{OpName} operation must have number type arguments:\n" +
				$" {leftValue.GetType().ToString()} and {rightValue.GetType().ToString()} were found.");
			return null;
		}

		private object GetMultValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			error = ParserError.Empty;
			if (leftValue is Decimal leftNum && rightValue is Decimal rightNum) {
				return leftNum * rightNum;
			}

			error = new ParserError($"{OpName} operation must have number type arguments:\n" +
				$" {leftValue.GetType().ToString()} and {rightValue.GetType().ToString()} were found.");
			return null;
		}

		private object GetMinusValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			error = ParserError.Empty;
			if (leftValue is Decimal leftNum && rightValue is Decimal rightNum) {
				return leftNum - rightNum;
			}

			error = new ParserError($"{OpName} operation must have number type arguments:\n" +
				$" {leftValue.GetType().ToString()} and {rightValue.GetType().ToString()} were found.");
			return null;
		}

		private object GetPlusValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			error = ParserError.Empty;
			if (leftValue is Decimal leftNum && rightValue is Decimal rightNum) {
				return leftNum + rightNum;
			}

			if (leftValue is string leftStr && rightValue is string rightStr) {
				return leftStr + rightStr;
			}

			error = new ParserError($"{OpName} operation must have number or string type arguments:\n" +
				$" {leftValue.GetType().ToString()} and {rightValue.GetType().ToString()} were found.");
			return null;
		}

		public override string ToString () {
			return $"BinOp {OpName}: {LeftOpRand}  {RightOpRand}";
		}

		private object leftValue;
		private object rightValue;

		public OperationToken.Operation OpName { get; private set; }
		public OpRand LeftOpRand { get; private set; }
		public OpRand RightOpRand { get; private set; }
	}
}
