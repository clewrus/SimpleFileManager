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
	public partial class InteractiveGrid : UserControl {
		public InteractiveGrid () {
			InitializeComponent();

			gridStructure = new List<List<UIElement>>(1024);
			gridHeaderStructure = new List<(UIElement, UIElement)>(256);
			rowNumberStructure = new List<(UIElement, UIElement)>(256);

			UpdateGrid();
			SubscribeToChangeEvents();
		}

		#region Dependency propertes events handlers
		private void SubscribeToChangeEvents () {
			var gridDataDescriptor = DependencyPropertyDescriptor.FromProperty(GridDataProperty, typeof(InteractiveGrid));
			gridDataDescriptor.AddValueChanged(this, GridDataChangedHandler);

			var headerBackgroundDescription = DependencyPropertyDescriptor.FromProperty(HeaderBackgroundProperty, typeof(InteractiveGrid));
			headerBackgroundDescription.AddValueChanged(this, HeaderBackgroundChangedHandler);

			var gridSplitterBrushDescription = DependencyPropertyDescriptor.FromProperty(GridSplitterBrushProperty, typeof(InteractiveGrid));
			gridSplitterBrushDescription.AddValueChanged(this, GridSplitterBrushChangedHandler);
		}

		private void GridDataChangedHandler (Object sender, EventArgs e) {
			UpdateGrid();
		}

		private void HeaderBackgroundChangedHandler (Object sender, EventArgs e) {
			GridHeader.Background = HeaderBackground;
			RowNumberGrid.Background = HeaderBackground;
		}

		private void GridSplitterBrushChangedHandler (Object sender, EventArgs e) {
			var changeBrush = new Action<List<(UIElement, UIElement)>> (headerList => {
				foreach (var header in headerList) {
					if (header.Item1 is Border border) {
						border.BorderBrush = GridSplitterBrush;
					}

					if (header.Item2 is GridSplitter splitter) {
						splitter.Background = GridSplitterBrush;
					}
				}
			});

			changeBrush(gridHeaderStructure);
			changeBrush(rowNumberStructure);
		}
		#endregion

		private void UpdateGrid () {
			AdjustWidth();
			AdjustHeight();
		}

		private void MainGrid_ScrollChanged (object sender, ScrollChangedEventArgs e) {
			var mainScrollView = sender as ScrollViewer;
			if (sender == null) return;

			RowNumberScroller.ScrollToVerticalOffset(mainScrollView.VerticalOffset);
			ColumnNumberScroller.ScrollToHorizontalOffset(mainScrollView.HorizontalOffset);
		}

		private UIElement CreateCell (Cell context) {
			var nwCell = new TextBox();
			
			nwCell.DataContext = context;
			nwCell.MinHeight = MIN_CELL_HEIGHT;
			nwCell.Height = MIN_CELL_HEIGHT;

			nwCell.MinWidth = MIN_CELL_WIDTH;
			nwCell.Width = MIN_CELL_WIDTH;
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
				AddColumnToGrid();
				FillLastColumn();
			}

			for (int i = 0; i < initWidth - targetWidth; i++) {
				RemoveLastColumnCells();
				RemoveLastColumnFromGrid();
			}
		}

		private void AdjustHeight () {
			int initHeight = MainGrid.RowDefinitions.Count;

			if (GridData == null) return;
			int targetHeight = GridData.Count;

			for (int i = 0; i < targetHeight - initHeight; i++) {
				AddRowToGrid();
				FillLastRow();
			}

			for (int i = 0; i < initHeight - targetHeight; i++) {
				RemoveLastRowCells();
				RemoveLastRowFromGrid();
			}
		}

		#region Columns Adjusting
		private void AddColumnToGrid () {
			var nwMainGridColumn = new ColumnDefinition() {
				Width = GridLength.Auto
			};
			MainGrid.ColumnDefinitions.Add(nwMainGridColumn);

			AddColumnHeader(nwMainGridColumn);
		}

		private void AddColumnHeader (ColumnDefinition nwMainGridColumn) {
			var nwGridHeaderColumm = new ColumnDefinition() {
				MinWidth = MIN_CELL_WIDTH
			};
			var widthBinding = new Binding("Width") {
				Source = nwMainGridColumn,
				Mode = BindingMode.TwoWay
			};
			
			nwGridHeaderColumm.SetBinding(ColumnDefinition.WidthProperty, widthBinding);
			nwGridHeaderColumm.Width = new GridLength(MIN_CELL_WIDTH);
			GridHeader.ColumnDefinitions.Add(nwGridHeaderColumm);

			UIElement nwHeader = CreateGridColumnHeaderContent(GridHeader.ColumnDefinitions.Count - 2);
			Grid.SetColumn(nwHeader, GridHeader.ColumnDefinitions.Count - 1);
			GridHeader.Children.Add(nwHeader);

			var nwGridSplitter = new GridSplitter() { 
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Stretch,
				Background = GridSplitterBrush,
				Width = 1
			};
			Grid.SetColumn(nwGridSplitter, GridHeader.ColumnDefinitions.Count - 1);
			GridHeader.Children.Add(nwGridSplitter);

			gridHeaderStructure.Add((nwHeader, nwGridSplitter));
			Debug.Assert(gridHeaderStructure.Count == GridHeader.ColumnDefinitions.Count - 1);
		}

		private UIElement CreateGridColumnHeaderContent (int headerIndex) {
			string name = "";
			int numOfLetters = 'Z' - 'A' + 1;

			do {
				int letterShift = (headerIndex < numOfLetters && name.Length > 0) ? -1 : 0;
				var leadingLetter = (char)((int)'A' + headerIndex % numOfLetters + letterShift);

				name = $"{leadingLetter}{name}";
				headerIndex /= numOfLetters;
			} while (headerIndex > 0);

			var content = new TextBlock() {
				Text = name,
				TextAlignment = TextAlignment.Center,
			};			

			return new Border() {
				Child = content,
				BorderThickness = new Thickness(1, 0, 0, 0),
				BorderBrush = GridSplitterBrush
			};
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

		private void RemoveLastColumnFromGrid () {
			GridHeader.Children.Remove(gridHeaderStructure[gridHeaderStructure.Count - 1].Item1);
			GridHeader.Children.Remove(gridHeaderStructure[gridHeaderStructure.Count - 1].Item2);

			gridHeaderStructure.RemoveAt(gridHeaderStructure.Count - 1);
			MainGrid.ColumnDefinitions.RemoveAt(MainGrid.ColumnDefinitions.Count - 1);
		}
		#endregion

		#region Rows Adjusting
		private void AddRowToGrid () {
			var nwMainGridRow = new RowDefinition() {
				Height = GridLength.Auto
			};
			MainGrid.RowDefinitions.Add(nwMainGridRow);

			AddRowHeader(nwMainGridRow);
		}

		private void AddRowHeader (RowDefinition nwMainGridRow) {
			var nwRowHeaderDefenition = new RowDefinition() {
				Height = GridLength.Auto,
				MinHeight = MIN_CELL_HEIGHT
			};
			var heightBinding = new Binding("Height") {
				Source = nwMainGridRow,
				Mode = BindingMode.TwoWay
			};

			nwRowHeaderDefenition.SetBinding(RowDefinition.HeightProperty, heightBinding);
			nwRowHeaderDefenition.Height = new GridLength(MIN_CELL_HEIGHT);
			RowNumberGrid.RowDefinitions.Add(nwRowHeaderDefenition);

			var rowHeaderContent = CreateRowHeaderContent(RowNumberGrid.RowDefinitions.Count - 1);
			Grid.SetRow(rowHeaderContent, RowNumberGrid.RowDefinitions.Count - 1);
			RowNumberGrid.Children.Add(rowHeaderContent);

			var nwGridSplitter = new GridSplitter() {
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Bottom,
				Background = GridSplitterBrush,
				Height = 1
			};

			Grid.SetRow(nwGridSplitter, RowNumberGrid.RowDefinitions.Count - 1);
			RowNumberGrid.Children.Add(nwGridSplitter);

			rowNumberStructure.Add((rowHeaderContent, nwGridSplitter));
			Debug.Assert(rowNumberStructure.Count == RowNumberGrid.RowDefinitions.Count);
		}

		private UIElement CreateRowHeaderContent (int rowIndex) {
			var content = new TextBlock() {
				Text = (rowIndex + 1).ToString(),
				HorizontalAlignment = HorizontalAlignment.Right,
				Margin = new Thickness(5, 3, 7, 3),
				VerticalAlignment = VerticalAlignment.Center
			};

			return new Border() {
				Child = content,
				BorderBrush = GridSplitterBrush,
				BorderThickness = new Thickness(0, 1, 0, 0)
			};
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

		private void RemoveLastRowFromGrid () {
			RowNumberGrid.Children.Remove(rowNumberStructure[rowNumberStructure.Count - 1].Item1);
			RowNumberGrid.Children.Remove(rowNumberStructure[rowNumberStructure.Count - 1].Item2);

			rowNumberStructure.RemoveAt(rowNumberStructure.Count - 1);
			MainGrid.RowDefinitions.RemoveAt(MainGrid.RowDefinitions.Count - 1);
		}
		#endregion

		#endregion

		#region Dependency properties
		public ObservableCollection<ObservableCollection<Cell>> GridData {
			get { return (ObservableCollection<ObservableCollection<Cell>>)GetValue(GridDataProperty); }
			set { SetValue(GridDataProperty, value); }
		}

		public static readonly DependencyProperty GridDataProperty = DependencyProperty.Register(
			"GridData", typeof(ObservableCollection<ObservableCollection<Cell>>), typeof(InteractiveGrid),
			new PropertyMetadata(new ObservableCollection<ObservableCollection<Cell>>())
		);

		public Brush GridSplitterBrush {
			get { return (Brush)GetValue(GridSplitterBrushProperty); }
			set { SetValue(GridSplitterBrushProperty, value); }
		}

		public static readonly DependencyProperty GridSplitterBrushProperty = DependencyProperty.Register(
			"GridSplitterBrush", typeof(Brush), typeof(InteractiveGrid), new PropertyMetadata(null)
		);

		public Brush HeaderBackground {
			get { return (Brush)GetValue(HeaderBackgroundProperty); }
			set { SetValue(HeaderBackgroundProperty, value); }
		}

		public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register(
			"HeaderBackground", typeof(Brush), typeof(InteractiveGrid), new PropertyMetadata(null)
		);
		#endregion

		private static readonly double MIN_CELL_HEIGHT = 22;
		private static readonly double MIN_CELL_WIDTH = 100;

		private List<List<UIElement>> gridStructure;
		private List<(UIElement, UIElement)> gridHeaderStructure;
		private List<(UIElement, UIElement)> rowNumberStructure;
	}
}
