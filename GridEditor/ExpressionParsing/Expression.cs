using SimpleFM.GridEditor.GridRepresentation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.ExpressionParsing {
	public class Expression : OpRand {
		public Expression (LinkedList<ILexem> lexems, out ParserError error) {

			FindAllSubExpressions(lexems, out error);
			if (!error.IsEmpty) return;

			FindAllTerms(lexems, out error);
			if (!error.IsEmpty) return;

			FindAllUnarOperations(lexems, out error);
			if (!error.IsEmpty) return;

			FindAllBinarOperations(lexems, out error);
			if (!error.IsEmpty) return;

			expressionComponents = lexems;
		}

		public override void Validate (out ParserError error) {
			if (expressionComponents.Count != 1 || !(expressionComponents.First.Value is OpRand)) {
				error = new ParserError("Can't parse expression");
				return;
			}

			((OpRand)expressionComponents.First.Value).Validate(out error);
		}

		public override object GetValue (Dictionary<GridCoordinates, object> childCells, out ParserError error) {
			try {
				error = new ParserError("Something went wrong in expression");

				if (expressionComponents.First.Value is OpRand opRand) {
					return opRand.GetValue(childCells, out error);
				}

				if (expressionComponents.First.Value is StringToken strToken) {
					error = ParserError.Empty;
					return strToken.Value;
				}

				return null;
			} catch {
				error = new ParserError("Wrong expression");
				return null;
			}
		}

		#region SubExpression
		private void FindAllSubExpressions (LinkedList<ILexem> lexems, out ParserError error) {
			error = ParserError.Empty;

			bool insideSubExpr = false;
			int parenNum = 0;

			LinkedListNode<ILexem> openQuoteNode = null;
			var curNode = lexems.First;

			while (curNode != null) {
				var curSepToken = curNode.Value as SeparatorToken;
				if (curSepToken == null) {
					curNode = curNode.Next;
					continue;
				}

				if (curSepToken.Value == SeparatorToken.Separator.LParen) {
					parenNum += 1;
					if (!insideSubExpr) {
						insideSubExpr = true;
						openQuoteNode = curNode;
					}
				}

				if (curSepToken.Value == SeparatorToken.Separator.RParen) {
					parenNum -= 1;
				}

				if (parenNum < 0) {
					error = new ParserError($"Unexpected token: ) at {curSepToken.Position}");
					return;
				}

				if (insideSubExpr && parenNum == 0) {
					insideSubExpr = false;
					curNode = HandleSubExpression(openQuoteNode, curNode, out error);
					openQuoteNode = null;

					if (!error.IsEmpty) return;
				}

				curNode = curNode.Next;
			}

			if (insideSubExpr) {
				error = new ParserError($"Can't find ) for ( at: {((Token)openQuoteNode.Value).Position}");
			}
		}

		private LinkedListNode<ILexem> HandleSubExpression (LinkedListNode<ILexem> lParNode, LinkedListNode<ILexem> rParNode, out ParserError error) {
			Debug.Assert(lParNode != null && rParNode != null);
			Debug.Assert(lParNode.Value is SeparatorToken lSep && lSep.Value == SeparatorToken.Separator.LParen);
			Debug.Assert(rParNode.Value is SeparatorToken rSep && rSep.Value == SeparatorToken.Separator.RParen);
			Debug.Assert(lParNode.List == rParNode.List);

			var tokens = lParNode.List;
			var subExprLexems = new LinkedList<ILexem>();

			var curNode = lParNode.Next;
			while (curNode != rParNode) {
				var nxtNode = curNode.Next;
				subExprLexems.AddLast(curNode.Value);

				tokens.Remove(curNode);
				curNode = nxtNode;
			}

			curNode = tokens.AddBefore(rParNode, new Expression(subExprLexems, out error));
			tokens.Remove(lParNode);
			tokens.Remove(rParNode);

			return curNode;
		}
		#endregion

		#region Terms
		private void FindAllTerms (LinkedList<ILexem> lexems, out ParserError error) {
			error = ParserError.Empty;

			var curNode = lexems.First;
			while (curNode != null) {				
				if (curNode.Value is CellNameToken cellNameToken) {
					curNode = Replace(curNode, new Term(cellNameToken));
				}

				if (curNode.Value is NumberToken numToken) {
					curNode = Replace(curNode, new Term(numToken));
				}

				if (curNode.Value is LogicToken logicToken) {
					curNode = Replace(curNode, new Term(logicToken));
				}
				
				if (IsQuote(curNode.Value)) {
					curNode = HandleStringTerm(curNode, out error);
					if (!error.IsEmpty) return;
				}

				curNode = curNode.Next;
			}
		}

		private LinkedListNode<ILexem> HandleStringTerm (LinkedListNode<ILexem> initNode, out ParserError error) {
			error = ParserError.Empty;
			var posibleError = new ParserError($"Unexpected token: \" at {((Token)initNode.Value).Position}");

			var tokens = initNode.List;
			var strNode = initNode.Next;

			if (strNode == null) {
				error = posibleError;
				return initNode;
			}

			if (!(strNode.Value is StringToken strToken)) {
				error = posibleError;
				return initNode;
			}

			tokens.Remove(initNode);
			strNode = Replace(strNode, new Term(strToken));

			var closeQuote = strNode.Next;
			if (closeQuote == null || !IsQuote(closeQuote.Value)) {
				error = posibleError;
				return initNode;
			}

			tokens.Remove(closeQuote);
			return strNode;
		}
		#endregion

		#region UnarOperation
		private void FindAllUnarOperations (LinkedList<ILexem> lexems, out ParserError error) {
			error = ParserError.Empty;

			var curNode = lexems.First;
			while (curNode != null) {
				if (curNode.Value is OperationToken opToken) {
					if (opToken.Value == OperationToken.Operation.If) {
						curNode = HandleIfOperation(curNode, out error);
						if (!error.IsEmpty) return;
					}

					foreach (var op in trivialUnarOperations) {
						if (opToken.Value == op) {
							curNode = HandleUnarOperation(curNode, out error);
							if (!error.IsEmpty) return;
						}
					}
				}
				curNode = curNode.Next;
			}
		}

		private LinkedListNode<ILexem> HandleIfOperation (LinkedListNode<ILexem> curNode, out ParserError error) {
			var argumentNode = curNode.Next;
			if (argumentNode == null) {
				error = new ParserError($"Can't find IF arguments at: {((Token)curNode.Value).Position}");
				return curNode;
			}

			if (!(argumentNode.Value is Expression argExpr)) {
				error = new ParserError($"IF must has arguments at: {((Token)curNode.Value).Position}");
				return curNode;
			}

			curNode.List.Remove(curNode);

			var oprands = new OpRandsList(argExpr, out error);
			if (!error.IsEmpty) return argumentNode;

			return Replace(argumentNode, new UnarOp(OperationToken.Operation.If, oprands, out error));
		}

		private LinkedListNode<ILexem> HandleUnarOperation (LinkedListNode<ILexem> curNode, out ParserError error) {
			var currentOperation = ((OperationToken)curNode.Value).Value;

			var nextNode = curNode.Next;
			if (nextNode == null) {
				error = new ParserError($"Can't find unar plus' argument at: {((Token)curNode.Value).Position}");
				return curNode;
			}

			if (curNode.Previous != null && curNode.Previous.Value is OpRand) {
				error = ParserError.Empty;
				return curNode;
			}

			curNode.List.Remove(curNode);
			var nextOpRand = nextNode.Value as OpRand;
			if (nextOpRand == null) {
				error = new ParserError($"Can't find Unar {currentOperation.ToString()} argument");
				return curNode;
			}

			return Replace(nextNode, new UnarOp(currentOperation, nextOpRand, out error));
		}
		#endregion

		#region BinarOperation
		private void FindAllBinarOperations (LinkedList<ILexem> lexems, out ParserError error) {
			error = ParserError.Empty;

			foreach (var priority in opPriority) {

				var curNode = lexems.First;
				while (curNode != null) {
					if (curNode.Value is OperationToken opToken) {
						foreach (var op in priority) {
							if (opToken.Value == op) {
								curNode = HandleBinarOperation(curNode, out error);
								if (!error.IsEmpty) return;
							}
						}
					}
					curNode = curNode.Next;
				}
			}
		}

		private LinkedListNode<ILexem> HandleBinarOperation (LinkedListNode<ILexem> initNode, out ParserError error) {
			var curOpToken = initNode.Value as OperationToken;

			var prevNode = initNode.Previous;
			var nextNode = initNode.Next;

			if (prevNode == null) {
				error = new ParserError($"Can't find left argument of {curOpToken.ActualValue} at: {curOpToken.Position}");
				return initNode;
			}

			if (nextNode == null) {
				error = new ParserError($"Can't find right argument of {curOpToken.ActualValue} at: {curOpToken.Position}");
				return initNode;
			}

			if (!(prevNode.Value is OpRand leftOpRand)) {
				error = new ParserError($"Wrong left operand of {curOpToken.ActualValue} at: {curOpToken.Position}");
				return initNode;
			}

			if (!(nextNode.Value is OpRand rightOpRand)) {
				error = new ParserError($"Wrong right operand of {curOpToken.ActualValue} at: {curOpToken.Position}");
				return initNode;
			}

			prevNode.List.Remove(prevNode);
			nextNode.List.Remove(nextNode);

			return Replace(initNode, new BinarOp(curOpToken.Value, leftOpRand, rightOpRand, out error));
		}
		#endregion

		private LinkedListNode<T> Replace<T> (LinkedListNode<T> oldLexemNode, T nwLexem) {
			var nxtNode = oldLexemNode.Next;
			var lexems = oldLexemNode.List;

			lexems.Remove(oldLexemNode);
			if (nxtNode == null) {
				return lexems.AddLast(nwLexem);
			} else {
				return lexems.AddBefore(nxtNode, nwLexem);
			}
		}

		private bool IsQuote (ILexem lexem) {
			return lexem is SeparatorToken sepToken && sepToken.Value == SeparatorToken.Separator.Quote;
		}

		public LinkedList<ILexem> expressionComponents;

		private static readonly List<OperationToken.Operation> trivialUnarOperations = new List<OperationToken.Operation> {
			OperationToken.Operation.Plus,
			OperationToken.Operation.Minus,
			OperationToken.Operation.Not
		};

		private static readonly List<OperationToken.Operation[]> opPriority = new List<OperationToken.Operation[]> {
			new OperationToken.Operation[] {
				OperationToken.Operation.Mult,
				OperationToken.Operation.Divide,
				OperationToken.Operation.Div,
				OperationToken.Operation.Mod
			},
			new OperationToken.Operation[] {
				OperationToken.Operation.Plus,
				OperationToken.Operation.Minus
			},
			new OperationToken.Operation[] {
				OperationToken.Operation.More,
				OperationToken.Operation.MoreEq,
				OperationToken.Operation.Less,
				OperationToken.Operation.LessEq,
				OperationToken.Operation.Equal,
				OperationToken.Operation.UnEqual
			},
		};
	}
}
