using SimpleFM.GridEditor.GridRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.ExpressionParsing {
	public abstract class Token : ILexem {
		public override String ToString () {
			return $"({this.GetType().Name}): {ActualValue}";
		}

		public int Position { get; set; }
		public string ActualValue { get; set; }
	}

	public class UndefinedToken : Token {
		public char Value { get; set; }
	}

	public class SeparatorToken : Token {
		public enum Separator { None, Space, Comma, LParen, RParen, Quote }
		public Separator Value { get; set; }
	}

	public class StringToken : Token {
		public string Value { get; set; }
	}

	public class LogicToken : Token {
		public bool Value { get; set; }
	}

	public class OperationToken : Token {
		public static readonly HashSet<string> FUNC_NAMES = new HashSet<string>() { };
		public enum Operation { None, FormulaSign, Plus, Minus, Mult, Divide, Div, Mod,
			If, Not, More, Less, MoreEq, LessEq, Equal, UnEqual, Func}
		public Operation Value;
	}

	public class NumberToken : Token {
		public string Value { get; set; }
	}

	public class CellNameToken : Token {
		public GridCoordinates Value { get; set; }
	}
}
