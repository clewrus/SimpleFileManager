using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.ExpressionParsing {
	public struct ParserError {
		private ParserError (bool isEmpty) {
			Message = "";
			IsEmpty = isEmpty;
		}

		public ParserError (string message) {
			Message = message;
			IsEmpty = false;
		}

		public override bool Equals (object obj) {
			return obj is ParserError other && other.IsEmpty == other.IsEmpty && other.Message == other.Message;
		}

		public override int GetHashCode () {
			return IsEmpty.GetHashCode() ^ Message.GetHashCode();
		}

		public static ParserError Empty {
			get => new ParserError(true);
		}

		public string Message { get; private set; }
		public bool IsEmpty { get; private set; }
	}
}
