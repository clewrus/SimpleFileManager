using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.GridRepresentation {
	public struct GridCoordinates {

		public GridCoordinates (int x, int y) {
			this.x = x;
			this.y = y;
		}

		public GridCoordinates (string x, string y) {
			this.y = int.Parse(y) - 1;

			this.x = -1;
			this.x = ParseRowIndex(x);
		}

		public (int, int) GetNumericCoords () {
			return (this.x, this.y);
		}

		public (string, string) GetStringCoords () {
			string name = "";
			int numOfLetters = 'Z' - 'A' + 1;
			int headerIndex = this.x;

			do {
				int letterShift = (headerIndex < numOfLetters && name.Length > 0) ? -1 : 0;
				var leadingLetter = (char)((int)'A' + headerIndex % numOfLetters + letterShift);

				name = $"{leadingLetter}{name}";
				headerIndex /= numOfLetters;
			} while (headerIndex > 0);

			return (name, (this.y + 1).ToString());
		}

		private int ParseRowIndex (string x) {
			int rowIndex = 0;
			int numOfLetters = (int)'Z' - (int)'A' + 1;

			for (int i = 0; i < x.Length; i++) {
				rowIndex *= numOfLetters;
				rowIndex += ((int)x[i] - (int)'A');
				if (i == 0 && x.Length > 1) {
					rowIndex += 1;
				}
			}

			return rowIndex;
		}

		private int x;
		private int y;
	}
}
