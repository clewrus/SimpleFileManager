using SimpleFM.GridEditor.GridRepresentation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.ExpressionParsing {
	public class Tokenizer {
		private Tokenizer () {}

		private LinkedListNode<T> InsertInsteadSequence<T> (LinkedListNode<T> start, LinkedListNode<T> end, T value) {
			var tokens = start.List;

			var curNode = start;
			var inclusiveEnd = (end == null) ? tokens.Last : end.Previous;
			Debug.Assert(start.List == inclusiveEnd.List, "Nodes from different lists");

			bool done = false;
			while (!done) {
				var nxt = curNode.Next;
				tokens.Remove(curNode);

				done |= curNode == inclusiveEnd;
				curNode = nxt;
			}

			if (curNode == null) {
				tokens.AddLast(value);
				return tokens.Last;
			}

			tokens.AddBefore(curNode, value);
			return curNode.Previous;
		}

		private LinkedList<Token> GenerateInitialTokenList (string initStr) {
			var tokenList = new LinkedList<Token>();
			for (int i = 0; i < initStr.Length; i++) {
				tokenList.AddLast(new UndefinedToken() {
					ActualValue = initStr[i].ToString(),
					Position = i,
					Value = char.ToLower(initStr[i])
				});
			}

			return tokenList;
		}

		private LinkedList<Token> GenerateTrivialTokenList (string initString) {
			var tokens = new LinkedList<Token>();
			tokens.AddLast(new StringToken() {
				ActualValue = initString,
				Position = 0,
				Value = initString
			});

			return tokens;
		}

		public LinkedList<Token> TokenizeString (string initString) {
			if (string.IsNullOrEmpty(initString)) {
				return new LinkedList<Token>();
			}

			if (initString[0] != '=') {
				return GenerateTrivialTokenList(initString);
			}

			var initialTokenList = GenerateInitialTokenList(initString);
			FindSeparators(initialTokenList);
			FindStrings(initialTokenList);
			FindOperations(initialTokenList);
			FindLogicals(initialTokenList);
			FindCellNames(initialTokenList);
			FindNumbers(initialTokenList);
			
			return initialTokenList;
		}

		#region Separators
		private void FindSeparators (LinkedList<Token> tokens) {
			var curNode = tokens.First;
			while (curNode != null) {
				if (curNode.Value is UndefinedToken) {
					if (TryFindSeparator(curNode, out SeparatorToken sepToken, out LinkedListNode<Token> next)) {
						curNode = InsertInsteadSequence(curNode, next, sepToken);
					}
				}

				curNode = curNode.Next;
			}
		}

		private bool TryFindSeparator (LinkedListNode<Token> initNode, out SeparatorToken sepToken, out LinkedListNode<Token> nxtNode) {
			sepToken = null;
			nxtNode = initNode.Next;

			var targetToken = initNode.Value as UndefinedToken;
			if (targetToken == null) {
				return false;
			}

			var sepType = SeparatorToken.Separator.None;
			switch (targetToken.Value) {
				case ' ': sepType = SeparatorToken.Separator.Space; break;
				case '(': sepType = SeparatorToken.Separator.LParen; break;
				case ')': sepType = SeparatorToken.Separator.RParen; break;
				case '"': sepType = SeparatorToken.Separator.Quote; break;
				case ',': sepType = SeparatorToken.Separator.Comma; break;
			}
			if (sepType == SeparatorToken.Separator.None) {
				return false;
			}

			sepToken = new SeparatorToken() {
				Position = targetToken.Position,
				Value = sepType,
				ActualValue = targetToken.ActualValue
			};

			if (sepType == SeparatorToken.Separator.Space) {
				while (initNode.Next != null && initNode.Next.Value is UndefinedToken nextToken && nextToken.Value == ' ') {
					initNode = initNode.Next;
					sepToken.ActualValue += " ";
					nxtNode = initNode.Next;
				}
			}

			return true;
		}
		#endregion

		#region Strings
		private void FindStrings (LinkedList<Token> tokens) {
			var curNode = tokens.First;
			while (curNode != null) {
				if (IsQuoteToken(curNode.Value)) {

					if (TryFindString(curNode, out StringToken strToken, out LinkedListNode<Token> endQuote)) {
						InsertInsteadSequence(curNode.Next, endQuote, strToken);
						curNode = endQuote;
					}
				}

				curNode = curNode.Next;
			}
		}

		private bool TryFindString (LinkedListNode<Token> startQuoteNode, out StringToken strToken, out LinkedListNode<Token> endQuoteNode) {
			Debug.Assert(IsQuoteToken(startQuoteNode.Value));

			strToken = null;
			endQuoteNode = startQuoteNode;

			bool symbolProtected = false;
			var curNode = startQuoteNode.Next;

			string value = "";
			while (curNode != null && (!IsQuoteToken(curNode.Value) || symbolProtected)) {
				bool isProtectSymbol = curNode.Value.ActualValue == "\\";
				symbolProtected = isProtectSymbol && !symbolProtected;

				if (!symbolProtected) {
					value += curNode.Value.ActualValue;
				}

				curNode = curNode.Next;
				if (curNode == null) {
					return false;
				}
			}

			if (curNode == null) {
				return false;
			}

			endQuoteNode = curNode;
			string strTokenValue = SampleStringFromTokens(startQuoteNode, endQuoteNode);
			strToken = new StringToken() {
				ActualValue = strTokenValue,
				Position = startQuoteNode.Value.Position + 1,
				Value = value
			};

			return true;
		}

		private string SampleStringFromTokens (LinkedListNode<Token> startQuote, LinkedListNode<Token> endQuote, bool includeQuotes=false) {
			Debug.Assert(includeQuotes || startQuote != endQuote);
			Debug.Assert(startQuote.List == endQuote.List);

			var curNode = (includeQuotes) ? startQuote : startQuote.Next;
			var targetQuote = (includeQuotes) ? endQuote.Next : endQuote;

			string sampledString = "";
			while (curNode != null && curNode != targetQuote) {
				sampledString += curNode.Value.ActualValue;
				curNode = curNode.Next;
			}

			return sampledString;
		}

		private bool IsQuoteToken (Token token) {
			return token is SeparatorToken sepToken && sepToken.Value == SeparatorToken.Separator.Quote;
		}
		#endregion

		#region Operations
		private void FindOperations (LinkedList<Token> tokens) {
			var curNode = tokens.First;
			while (curNode != null) {
				if (curNode.Value is UndefinedToken) {
					if (TryFindOperation(curNode, out OperationToken opToken, out LinkedListNode<Token> next)) {
						curNode = InsertInsteadSequence(curNode, next, opToken);
					}
				}

				curNode = curNode.Next;
			}
		}

		private bool TryFindOperation (LinkedListNode<Token> initNode, out OperationToken opToken, out LinkedListNode<Token> nxtNode) {
			Debug.Assert(initNode != null);
			Debug.Assert(initNode.Value is UndefinedToken);

			opToken = null;
			nxtNode = initNode;

			foreach (var operation in opNames) {
				if (TryFindSubsequence(initNode, operation.label, out nxtNode)) {
					if (!IsValidOperation(initNode.Previous, nxtNode, operation.opName)) continue;

					var previous = (nxtNode == null) ? initNode.List.Last : nxtNode.Previous;
					opToken = new OperationToken() {
						Value = operation.opName,
						Position = initNode.Value.Position,
						ActualValue = SampleStringFromTokens(initNode, previous, true)
					};
					
					return true;
				}
			}

			return false;			
		}

		private bool TryFindSubsequence (LinkedListNode<Token> initNode, string subSeq, out LinkedListNode<Token> nxtNode) {
			nxtNode = initNode;
			var curNode = initNode;

			for (int i = 0; i < subSeq.Length; i++) {
				if (curNode == null) {
					return false;
				}

				var undefinedToken = curNode.Value as UndefinedToken;

				if (undefinedToken == null || undefinedToken.Value != subSeq[i]) {
					return false;
				}

				curNode = curNode.Next;
			}

			nxtNode = curNode;
			return true;
		}

		private bool IsValidOperation (LinkedListNode<Token> preStartNode, LinkedListNode<Token> nextNode, OperationToken.Operation opName) {
			if (opName == OperationToken.Operation.If || 
				opName == OperationToken.Operation.Mod ||
				opName == OperationToken.Operation.Div)
			{
				return (preStartNode == null || !IsLetter(preStartNode.Value)) &&
						(nextNode == null || !IsLetter(nextNode.Value));
			}

			if (opName == OperationToken.Operation.FormulaSign) {
				return preStartNode == null;
			}

			return true;
		}

		private bool IsLetter (Token token) {
			return token is UndefinedToken undefinedToken && Char.IsLetter(undefinedToken.Value);
		}
		#endregion

		#region Logics
		private void FindLogicals (LinkedList<Token> tokens) {
			var curNode = tokens.First;
			while (curNode != null) {
				if (curNode.Value is UndefinedToken) {
					if (TryFindLogical(curNode, out LogicToken logicToken, out LinkedListNode<Token> next)) {
						curNode = InsertInsteadSequence(curNode, next, logicToken);
					}
				}

				curNode = curNode.Next;
			}
		}

		private bool TryFindLogical (LinkedListNode<Token> initNode, out LogicToken logicToken, out LinkedListNode<Token> nxtNode) {
			logicToken = null;
			nxtNode = initNode;
			if (initNode.Previous != null && IsLetter(initNode.Previous.Value)) return false;

			if (TryFindSubsequence(initNode, "true", out nxtNode)) {
				var lastNode = (nxtNode == null) ? initNode.List.Last : nxtNode.Previous;

				if (nxtNode != null && IsLetter(nxtNode.Value)) {
					return false;
				}
					
				logicToken = new LogicToken() {
					Position = initNode.Value.Position,
					Value = true,
					ActualValue = SampleStringFromTokens(initNode, lastNode, true)
				};
				return true;
			}

			if (TryFindSubsequence(initNode, "false", out nxtNode)) {
				var prevNode = (nxtNode == null) ? initNode.List.Last : nxtNode.Previous;

				if (nxtNode != null && IsLetter(nxtNode.Value)) {
					return false;
				}

				logicToken = new LogicToken() {
					Position = initNode.Value.Position,
					Value = false,
					ActualValue = SampleStringFromTokens(initNode, prevNode, true)
				};
				return true;
			}

			return false;
		}
		#endregion

		#region Number
		private void FindNumbers (LinkedList<Token> tokens) {
			var curNode = tokens.First;
			while (curNode != null) {
				if (curNode.Value is UndefinedToken) {
					if (TryFindNumber(curNode, out NumberToken numToken, out LinkedListNode<Token> next)) {
						curNode = InsertInsteadSequence(curNode, next, numToken);
					}
				}

				curNode = curNode.Next;
			}
		}

		private bool TryFindNumber (LinkedListNode<Token> initNode, out NumberToken numToken, out LinkedListNode<Token> nxtNode) {
			numToken = null;
			nxtNode = initNode;

			bool hasDot = false;
			bool hasExp = false;
			bool hasExpSign = false;
			bool hasDigits = false;
			bool hasExpDigits = false;
			var curNode = initNode;
			string actualValue = "";

			while (curNode != null) {
				var curValue = curNode.Value;
				if (!IsDigit(curValue) && !IsDot(curValue) && !IsExp(curValue) && !IsPlusMinus(curValue)) break;
				if ((hasDot || hasExp) && IsDot(curValue)) break;
				if ((!hasDigits || hasExp) && IsExp(curValue)) break;
				if ((hasExpSign || !hasExp || hasExpDigits) && IsPlusMinus(curValue)) break;

				hasDot = hasDot || IsDot(curValue);
				hasExp = hasExp || IsExp(curValue);
				hasExpSign = hasExpSign || IsPlusMinus(curValue);
				hasExpDigits = hasExpDigits || hasExp && IsDigit(curValue);
				hasDigits = hasDigits || IsDigit(curValue);

				actualValue += curValue.ActualValue;
				curNode = curNode.Next;
			}

			if (actualValue == "") {
				return false;
			}

			nxtNode = curNode;
			numToken = new NumberToken() {
				ActualValue = actualValue,
				Position = initNode.Value.Position,
				Value = (actualValue[0] == '.') ? $"0{actualValue.ToLower()}" : actualValue.ToLower()
			};

			return true;
		}

		private bool IsDigit (Token token) {
			return token is UndefinedToken undefinedToken && char.IsDigit(undefinedToken.Value);
		}

		private bool IsDot (Token token) {
			return token is UndefinedToken undefinedToken && undefinedToken.Value == '.';
		}

		private bool IsExp (Token token) {
			return token is UndefinedToken undefinedToken && undefinedToken.Value == 'e';
		}

		private bool IsPlusMinus (Token token) {
			return token is OperationToken opToken && 
				(opToken.Value == OperationToken.Operation.Plus || opToken.Value == OperationToken.Operation.Minus);
		}
		#endregion

		#region CellName
		private void FindCellNames (LinkedList<Token> tokens) {
			var curNode = tokens.First;
			while (curNode != null) {
				if (curNode.Value is UndefinedToken) {
					if (TryFindCellName(curNode, out CellNameToken nameToken, out LinkedListNode<Token> next)) {
						curNode = InsertInsteadSequence(curNode, next, nameToken);
					}
				}

				curNode = curNode.Next;
			}
		}

		private bool TryFindCellName (LinkedListNode<Token> initNode, out CellNameToken nameToken, out LinkedListNode<Token> nxtNode) {
			nxtNode = initNode;
			nameToken = null;

			if (!IsBigLetter(initNode.Value)) {
				return false;
			}
			if (initNode.Previous != null && IsDigit(initNode.Previous.Value)) {
				return false;
			}

			bool numPart = false;
			string actualValue = "";
			var curNode = initNode;
			while (curNode != null && (!numPart && IsBigLetter(curNode.Value) || numPart && IsDigit(curNode.Value))) {
				actualValue += curNode.Value.ActualValue;
				curNode = curNode.Next;
				numPart = numPart || (curNode != null && IsDigit(curNode.Value));
			}

			if (!GridCoordinates.TryParse(actualValue, out GridCoordinates value) || !numPart) {
				return false;
			}

			nxtNode = curNode;
			nameToken = new CellNameToken() {
				ActualValue = actualValue,
				Position = initNode.Value.Position,
				Value = value
			};

			return true;
		}

		private bool IsBigLetter (Token token) {
			return token is UndefinedToken t && ('A' <= t.ActualValue[0] && t.ActualValue[0] <= 'Z');
		}
		#endregion

		private static readonly List<(string label, OperationToken.Operation opName)> opNames = 
				new List<(string label, OperationToken.Operation opName)>() {
            ("=", OperationToken.Operation.FormulaSign),
			("+", OperationToken.Operation.Plus),
			("-", OperationToken.Operation.Minus),
			("*", OperationToken.Operation.Mult),
			("/", OperationToken.Operation.Divide),
			(">=", OperationToken.Operation.MoreEq),
			(">", OperationToken.Operation.More),
			("<=", OperationToken.Operation.LessEq),
			("<", OperationToken.Operation.Less),
			("==", OperationToken.Operation.Equal),
			("!=", OperationToken.Operation.UnEqual),
			("!", OperationToken.Operation.Not),
			("if", OperationToken.Operation.If),
			("mod", OperationToken.Operation.Mod),
			("div", OperationToken.Operation.Div),
		};

		private static Tokenizer _Instance;
		public static Tokenizer Instance {
			get {
				if (_Instance == null) {
					_Instance = new Tokenizer();
				}
				return _Instance;
			}
		}
	}
}
