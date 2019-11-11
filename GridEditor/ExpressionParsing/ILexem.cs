using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.ExpressionParsing {
    public interface ILexem { }

	public abstract class OpRand : ILexem {
		public abstract void Validate (out ParserError error);
	}

	public abstract class Operation : OpRand {

	}	
}
