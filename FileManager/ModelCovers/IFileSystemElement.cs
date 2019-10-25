using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace SimpleFM.FileManager.ModelCovers {
	public interface IFileSystemElement {
		string ElementPath { get; }
		string ElementName { get; }

		FileSystemFacade.ElementType ElementType { get; }
	}
}
