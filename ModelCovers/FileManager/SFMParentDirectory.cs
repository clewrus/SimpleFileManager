using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.ModelCovers {
	public class SFMParentDirectory : SFMDirectory, IFileSystemElement {
		internal SFMParentDirectory (string path) : base(path) { }
		
		public override string ElementName {
			get => "...";
		}
	}
}
