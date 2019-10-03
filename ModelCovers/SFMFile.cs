using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.ComponentModel;

namespace SimpleFM.ModelCovers {

	public class SFMFile : IFileSystemElement, INotifyPropertyChanged {
		internal SFMFile (string path) {
			ElementPath = path;
		}

		public override Boolean Equals (Object obj) {
			if (obj is SFMFile castedObj) {
				return ElementPath == castedObj.ElementPath;
			}
			return false;
		}

		public override Int32 GetHashCode () {
			return ElementPath.GetHashCode();
		}

		public string ElementPath { get; private set; }
		public string ElementName { get { return Path.GetFileName(ElementPath); } }

		public FileSystemFacade.ElementType ElementType { get { return FileSystemFacade.ElementType.File; } }

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
