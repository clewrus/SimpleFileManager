using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.ExpressionParsing {
	public class Parser {
		private Parser () { }

		public Expression ParseTokenList (LinkedList<Token> tokens, out ParserError error) {
			error = ParserError.Empty;

			if (ContainsUndefined(tokens, out UndefinedToken undefined)) {
				error = new ParserError($"Undefined token: {undefined.ActualValue} at {undefined.Position}");
				return null;
			}

			var lexems = new LinkedList<ILexem>(tokens);

			if (!IsFormula(tokens)) {
				Debug.Assert(tokens.Count == 1, "NonFormula token list must contain one element");
				return new Expression(lexems, out error);
			}

			lexems.RemoveFirst();
			RemoveSpaces(lexems);
			return new Expression(lexems, out error);
		}

		private bool ContainsUndefined (LinkedList<Token> tokens, out UndefinedToken undefined) {
			foreach (var token in tokens) {
				if (token is UndefinedToken curToken) {
					undefined = curToken;
					return true;
				}
			}

			undefined = null;
			return false;
		}

		private bool IsFormula (LinkedList<Token> tokens) {
			return tokens.First.Value is OperationToken opToken && opToken.Value == OperationToken.Operation.FormulaSign;
		}

		private void RemoveSpaces (LinkedList<ILexem> lexems) {
			var curNode = lexems.First;
			while (curNode != null) {
				var nextNode = curNode.Next;

				if (curNode.Value is SeparatorToken sepToken && sepToken.Value == SeparatorToken.Separator.Space) {
					lexems.Remove(curNode);
				}

				curNode = nextNode;
			}
		}

		private static Parser _Instance;
		public static Parser Instance {
			get {
				if (_Instance == null) {
					_Instance = new Parser();
				}
				return _Instance;
			}
		}
	}
}
