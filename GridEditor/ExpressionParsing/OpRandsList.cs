using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.ExpressionParsing {
	public class OpRandsList : OpRand {
		public OpRandsList (Expression expr, out ParserError error) {
			error = ParserError.Empty;

			OpRands = new List<OpRand>();
			var lexems = new LinkedList<ILexem>(expr.expressionComponents);

			bool isOprand = true;
			var curNode = lexems.First;
			while (curNode != null) {
				var curOpRand = curNode.Value as OpRand;
				var curSeparation = curNode.Value as SeparatorToken;

				if (isOprand && curOpRand == null) {
					if (curSeparation != null) {
						error = new ParserError($"Unexpected token: {curSeparation.ActualValue} at {curSeparation.Position}");
					} else {
						error = new ParserError("Corrupted argument list");
					}
					return;
				}

				if (!isOprand && curSeparation == null) {
					error = new ParserError("Corrupted argument list");
					return;
				}

				if (curSeparation != null && curSeparation.Value != SeparatorToken.Separator.Comma) {
					error = new ParserError($"Unexpected token: {curSeparation.ActualValue} at {curSeparation.Position}");
					return;
				}

				isOprand = !isOprand;
				if (curOpRand != null) {
					OpRands.Add(curOpRand);
				}

				curNode = curNode.Next;
			}
		}

		public override void Validate (out ParserError error) {
			foreach (var opRand in OpRands) {
				opRand.Validate(out error);
				if (!error.IsEmpty) {
					return;
				}
			}

			error = ParserError.Empty;
		}

		public List<OpRand> OpRands { get; private set; }
	}
}
