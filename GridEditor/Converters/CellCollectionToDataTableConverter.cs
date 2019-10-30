using SimpleFM.GridEditor.GridRepresentation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace SimpleFM.GridEditor.Converters {
	class CellCollectionToDataTableConverter : IValueConverter {
		public Object Convert (Object value, Type targetType, Object parameter, CultureInfo culture) {
			var cellCollection = value as ObservableCollection<ObservableCollection<Cell>>;
			if (cellCollection == null) {
				return null;
			}

			DataTable nwDataTableInstance = SampleDataTable(cellCollection);
			return nwDataTableInstance.DefaultView;
		}

		public Object ConvertBack (Object value, Type targetType, Object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}

		private DataTable SampleDataTable (ObservableCollection<ObservableCollection<Cell>> tableData) {
			var nwTable = new DataTable();

			int width = FindWidth(tableData);
			int height = tableData.Count;

			for (int i = 0; i < width; i++) {
				nwTable.Columns.Add(new DataColumn(EvaluateColumnName(i)));
			}

			for (int i = 0; i < tableData.Count; i++) {
				var nwRow = nwTable.NewRow();

				for (int j = 0; j < tableData[i].Count; j++) {
					nwRow[j] = tableData[i][j];
				}
				nwTable.Rows.Add(nwRow);
			}

			return nwTable;
		}

		private int FindWidth (ObservableCollection<ObservableCollection<Cell>> tableData) {
			int curMax = tableData[0].Count;
			foreach (var row in tableData) {
				Debug.Assert(curMax == row.Count, "Table rows has different length.");
				curMax = Math.Max(curMax, row.Count);
			}
			return curMax;
		}

		private string EvaluateColumnName (int i) {
			string name = "";
			int numOfLetters = 'Z' - 'A' + 1;

			do {
				var leadingLetter = (char)((int)'A' + i % numOfLetters + ((i < numOfLetters && name.Length > 0) ? -1 : 0));

				name = $"{leadingLetter}{name}";
				i /= numOfLetters;
			} while (i > 0);

			return name;
		}
	}
}