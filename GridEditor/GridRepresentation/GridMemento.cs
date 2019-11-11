using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.GridRepresentation {

	[System.Serializable]
	public class GridMemento : IMemento {

		public GridMemento (GridInfo storedData) {
			Data = new GridInfo(storedData);
		}

		[System.Serializable]
		public struct CellInfo {
			public object value;
			public string expressionStr;
		}

		[System.Serializable]
		public class GridInfo {
			public GridInfo () {
				content = new Dictionary<(int, int), CellInfo>();
			}

			public GridInfo (GridInfo another) {
				this.height = another.height;
				this.width = another.width;
				this.content = new Dictionary<(int, int), CellInfo>(another.content);
			}

			public override String ToString () {
				return $"width: {this.width} height: {this.height} contentCount: {this.content.Count}";
			}

			public int width;
			public int height;

			public Dictionary<(int, int), CellInfo> content;
		}

		public override String ToString () {
			return Data.ToString();
		}

		public object Data { get; private set; }
	}
}
