using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.ComponentModel;
using System.Security.AccessControl;

namespace SimpleFM.FileManager.ModelCovers {
	public class SFMDirectory : IFileSystemElement, INotifyPropertyChanged {
		internal SFMDirectory (string path) {
			ElementPath = path;
			IsRoot = (Path.GetDirectoryName(ElementPath) == null);
		}

		public override Boolean Equals (Object obj) {
			if (obj is SFMDirectory castedObj) {
				return ElementPath == castedObj.ElementPath;
			}
			return false;
		}

		public override Int32 GetHashCode () {
			return ElementPath.GetHashCode();
		}

		public bool IsRoot { get; private set; }
		public string ElementPath { get; private set; }
		public virtual string ElementName { 
			get { return (IsRoot)? ElementPath : Path.GetFileName(ElementPath); }
		}

		public FileSystemFacade.ElementType ElementType { get { return FileSystemFacade.ElementType.Directory; } }

#pragma warning disable CS0067
		public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
	}
}
