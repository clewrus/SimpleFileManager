using SimpleFM.GridEditor.GridRepresentation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.ExpressionParsing {
	public class Term : OpRand {
		public Term (Token token) {
			this.Value = token;
		}

		public override void Validate (out ParserError error) {
			error = ParserError.Empty;
		}

		public override object GetValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			error = ParserError.Empty;
			if (IsLogic) return AsLogic;
			if (IsString) return AsString;
			if (IsNumber) return AsNumber;
			
			if (Value is CellNameToken cellName) {
				if (!childCells.ContainsKey(cellName.Value)) {
					(string x, string y) = cellName.Value.GetStringCoords();
					error = new ParserError($"Don't know the value of {x + y} cell.");
					return null;
				}

				return childCells[cellName.Value];
			}

			error = new ParserError($"Strange term at {Value.Position}");
			return null;
		}

		public GridCoordinates AsGridCoordinates => (Value is CellNameToken cellName) ? cellName.Value : default;
		public string AsString => (Value is StringToken stringToken) ? stringToken.Value : null;
		public bool AsLogic => (Value is LogicToken logicToken) ? logicToken.Value : false;
		public Decimal AsNumber {
			get {
				if (!(Value is NumberToken numToken)) {
					return default;
				}
	
				if (Decimal.TryParse(numToken.Value, NumberStyles.Any, new CultureInfo("en-US"), out Decimal number)) {
					return number;
				}

				return default;
			}
		}

		public override string ToString () {
			return Value.ToString();
		}

		public bool IsCellName => Value is CellNameToken;
		public bool IsNumber => Value is NumberToken;
		public bool IsString => Value is StringToken;
		public bool IsLogic => Value is LogicToken;

		public Token Value { get; protected set; }
	}
}
