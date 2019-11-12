using SimpleFM.GridEditor.ExpressionParsing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.GridRepresentation {
	public class ParsedCell : Cell {
		public ParsedCell(GridCoordinates coordinates) : base () {
			this.Coordinates = coordinates;
			ChildCells = new HashSet<GridCoordinates>();
			this.PreChangedByUser += PreChangedByUserHandler;
		}

		private void PreChangedByUserHandler (object sender, EventArgs e) {
			var tokens = EvaluateTokens();
			UpdateChildCells(tokens);

			curExpression = EvaluateCurrentExpression(tokens, out ParserError error);
			ParseErrorMessage = (error.IsEmpty) ? null : error.Message;
			ErrorMessage = ParseErrorMessage;
		}

		private void UpdateChildCells (IEnumerable<Token> tokens) {
			ChildCells.Clear();
			foreach (var token in tokens) {
				if (token is CellNameToken cellToken) {
					ChildCells.Add(cellToken.Value);
				}
			}
		}

		#region Parsing
		private Expression EvaluateCurrentExpression (LinkedList<Token> tokens, out ParserError error) {
			var expression = Parser.Instance.ParseTokenList(tokens, out error);
			if (!error.IsEmpty || expression == null || !IsFormula(tokens)) {
				return expression;
			}

			expression.Validate(out error);
			return expression;
		}

		private LinkedList<Token> EvaluateTokens () {
			var tokens = Tokenizer.Instance.TokenizeString(ExpressionStr);
			if (!IsFormula(tokens)) {
				var formulizedTokens = Tokenizer.Instance.TokenizeString("=" + ExpressionStr);
				if (HasSingleValueToken(formulizedTokens)) {
					tokens = formulizedTokens;
				}
			}

			return tokens;
		}

		private bool IsFormula (LinkedList<Token> tokens) {
			return tokens.First != null && tokens.First.Value is OperationToken opToken && opToken.Value == OperationToken.Operation.FormulaSign;
		}

		private bool HasSingleValueToken (LinkedList<Token> tokens) {
			bool unarSign = false;
			bool valueFound = false;
			foreach (var token in tokens) {
				if (token is SeparatorToken sepToken && sepToken.Value == SeparatorToken.Separator.Space) {
					continue;
				}

				if (token is OperationToken formulaToken && formulaToken.Value == OperationToken.Operation.FormulaSign) {
					continue;
				}

				if (!unarSign && !valueFound && token is OperationToken signToken && 
					(signToken.Value == OperationToken.Operation.Plus || signToken.Value == OperationToken.Operation.Minus))
				{
					unarSign = true;
					continue;
				} 

				if (!valueFound && (token is StringToken || token is LogicToken || token is NumberToken)) {
					valueFound = true;

					if (!unarSign || (token is NumberToken)) continue;
				}

				return false;
			}

			return valueFound;
		}
		#endregion

		public bool Calculate (Dictionary<GridCoordinates, object> childCells) {
			if (curExpression == null) return false;

			Value = curExpression.GetValue(childCells, out ParserError error);
			ParseErrorMessage = (error.IsEmpty) ? null : error.Message;
			ErrorMessage = ParseErrorMessage;

			return error.IsEmpty;
		}

		public HashSet<GridCoordinates> ChildCells { get; private set; }
		public string ParseErrorMessage { get; private set; }
		public GridCoordinates Coordinates { get; private set; }

		private Expression curExpression;
	}
}
