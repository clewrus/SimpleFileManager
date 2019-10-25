using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.ModelCovers.TextEditor {
	class SimpleHtmlText : IHtmlElement {
		public SimpleHtmlText (string stringValue) {
			this.StringValue = stringValue;
		}

		public override String ToString () {
			return StringValue;
		}

		public string StringValue { get; private set; }
	}
}
