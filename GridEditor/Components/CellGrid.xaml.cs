using SimpleFM.GridEditor.GridRepresentation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleFM.GridEditor.Components {
	public partial class CellGrid : UserControl {
		public CellGrid () {
			InitializeComponent();

			gridStructure = new List<List<UIElement>>(1024);
			UpdateGrid();

			var gridDataDescriptor = DependencyPropertyDescriptor.FromProperty(GridDataProperty, typeof(CellGrid));
			gridDataDescriptor.AddValueChanged(this, GridDataChanged);
		}

		private void GridDataChanged (Object sender, EventArgs e) {
			UpdateGrid();
		}

		private void UpdateGrid () {
			AdjustWidth();
			AdjustHeight();
		}

		private UIElement CreateCell (Cell context) {
			var nwCell = new TextBox();
			
			nwCell.DataContext = context;
			nwCell.MinHeight = 20;
			nwCell.Height = 20;

			nwCell.MinWidth = 100;
			nwCell.Width = 100;
			nwCell.Text = context.Value;

			var binding = new Binding("Value");
			binding.Source = context;
			nwCell.SetBinding(TextBox.TextProperty, binding);

			return nwCell;
		}

		#region Resizing
		private void AdjustWidth () {
			int initWidth = MainGrid.ColumnDefinitions.Count;

			if (GridData == null || GridData.Count == 0) return;
			int targetWidth = GridData[0].Count;

			for (int i = 0; i < targetWidth - initWidth; i++) {
				MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
				FillLastColumn();
			}

			for (int i = 0; i < initWidth - targetWidth; i++) {
				RemoveLastColumnCells();
				MainGrid.ColumnDefinitions.RemoveAt(MainGrid.ColumnDefinitions.Count - 1);
			}
		}

		private void AdjustHeight () {
			int initHeight = MainGrid.RowDefinitions.Count;

			if (GridData == null) return;
			int targetHeight = GridData.Count;

			for (int i = 0; i < targetHeight - initHeight; i++) {
				MainGrid.RowDefinitions.Add(new RowDefinition());
				FillLastRow();
			}

			for (int i = 0; i < initHeight - targetHeight; i++) {
				RemoveLastRowCells();
				MainGrid.RowDefinitions.RemoveAt(MainGrid.RowDefinitions.Count - 1);
			}
		}

		private void FillLastColumn () {
			int targetColumn = MainGrid.ColumnDefinitions.Count - 1;

			for (var i = 0; i < gridStructure.Count; i++) {

				UIElement nwCell = CreateCell(GridData[i][targetColumn]);
				Grid.SetColumn(nwCell, targetColumn);
				Grid.SetRow(nwCell, i);
				gridStructure[i].Add(nwCell);

				Debug.Assert(gridStructure[i].Count - 1 == targetColumn);
				MainGrid.Children.Add(nwCell);
			}
		}

		private void RemoveLastColumnCells () {
			foreach (var row in gridStructure) {
				MainGrid.Children.Remove(row[row.Count - 1]);
				row.RemoveAt(row.Count - 1);
			}
		}

		private void FillLastRow () {
			int gridWidth = MainGrid.ColumnDefinitions.Count;
			int gridHeight = MainGrid.RowDefinitions.Count;
			var nwRow = new List<UIElement>(gridWidth);

			for (int i = 0; i < gridWidth; i++) {
				UIElement nwCell = CreateCell(GridData[gridHeight - 1][i]);

				Grid.SetRow(nwCell, gridHeight - 1);
				Grid.SetColumn(nwCell, i);
				
				nwRow.Add(nwCell);
				MainGrid.Children.Add(nwCell);
			}

			gridStructure.Add(nwRow);
		}

		private void RemoveLastRowCells () {
			int gridHeight = MainGrid.RowDefinitions.Count;
			foreach (var cell in gridStructure[gridHeight]) {
				MainGrid.Children.Remove(cell);
			}
			gridStructure.RemoveAt(gridStructure.Count - 1);
		}


		#endregion

		#region Dependency properties
		public ObservableCollection<ObservableCollection<Cell>> GridData {
			get { return (ObservableCollection<ObservableCollection<Cell>>)GetValue(GridDataProperty); }
			set { SetValue(GridDataProperty, value); }
		}

		public static readonly DependencyProperty GridDataProperty = DependencyProperty.Register(
				"GridData", typeof(ObservableCollection<ObservableCollection<Cell>>), typeof(CellGrid),
				new PropertyMetadata(new ObservableCollection<ObservableCollection<Cell>>())
			);
		#endregion

		private List<List<UIElement>> gridStructure;
	}
}
