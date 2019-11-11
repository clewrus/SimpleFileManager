using SimpleFM.GridEditor.ExpressionParsing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.GridRepresentation {
	public class ParsedCell : Cell {
		public ParsedCell() : base () {
			this.ChangedByUser += ChangedByUserHandler;
		}

		private void ChangedByUserHandler (object sender, EventArgs e) {
			var expression = EvaluateCurrentExpression(out ParserError error);

			ErrorMessage = (error.IsEmpty) ? null : error.Message;
		}

		private Expression EvaluateCurrentExpression (out ParserError error) {
			var tokens = EvaluateTokens();

			var expression = Parser.Instance.ParseTokenList(tokens, out error);
			if (!error.IsEmpty || expression == null) {
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
			bool valueFinded = false;
			foreach (var token in tokens) {
				if (token is SeparatorToken sepToken && sepToken.Value == SeparatorToken.Separator.Space) {
					continue;
				}

				if (token is OperationToken opToken && opToken.Value == OperationToken.Operation.FormulaSign) {
					continue;
				}

				if (!valueFinded && (token is StringToken || token is LogicToken || token is NumberToken)) {
					valueFinded = true;
					continue;
				}

				return false;
			}

			return valueFinded;
		}
	}
}
