using SimpleFM.GridEditor.GridRepresentation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

			gridStructure = new List<List<UIElement>>(64);
			gridHeaderStructure = new List<(UIElement, UIElement)>(64);
			rowNumberStructure = new List<(UIElement, UIElement)>(64);

			selectManager = new CellSelectManager();
			selectManager.SelectedCellChanged += SelectedCellChangedHandler;

			UpdateGridSize();
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

			var expressionTextBoxDescription = DependencyPropertyDescriptor.FromProperty(ExpressionTextBoxProperty, typeof(InteractiveGrid));
			expressionTextBoxDescription.AddValueChanged(this, ExpressionTextBoxChanged);
		}

		private void GridDataChangedHandler (Object sender, EventArgs e) {
			GridData.CollectionChanged -= GridDataCollectionChangedHandler;
			GridData.CollectionChanged += GridDataCollectionChangedHandler;

			foreach (var row in GridData) {
				row.CollectionChanged -= GridDataCollectionChangedHandler;
				row.CollectionChanged += GridDataCollectionChangedHandler;
			}

			UpdateGridSize();
			UpdateCellsDataContext();
		}

		private void GridDataCollectionChangedHandler (Object sender, NotifyCollectionChangedEventArgs e) {
			if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove) {
				int width = GridData[0].Count;
				foreach (var row in GridData) {
					if (row.Count != width) {
						return;
					}
				}

				UpdateGridSize();
			}
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

		private void ExpressionTextBoxChanged (Object sender, EventArgs e) {
			if (selectManager == null) return;
			selectManager.ExpressionTextBox = ExpressionTextBox;
		}
		#endregion

		#region Selected cell changed handler
		private void SelectedCellChangedHandler (object sender, CellSelectManager.SelectedCellChangedEventArgs e) {
			UpdateScrollView(e);

			(string, string) cellPosition = e.ChangedCell.CellPosition.GetStringCoords();
			SelectedCell = $"{cellPosition.Item1}{cellPosition.Item2}";
		}

		private void UpdateScrollView (CellSelectManager.SelectedCellChangedEventArgs e) {
			Rect cellRect;
			if (!TryCalculateRelativeCellRect(e.ChangedCell, out cellRect))
				return;

			var gridRect = new Rect(0, 0, this.ActualWidth, this.ActualHeight);
			var intersection = Rect.Intersect(gridRect, cellRect);

			if (intersection == cellRect)
				return;

			ScrollToCell(cellRect);
		}

		private bool TryCalculateRelativeCellRect (GridCell targetCell, out Rect cellRect) {
			GeneralTransform cellToGridTransform;
			try {
				cellToGridTransform = targetCell.TransformToAncestor(this);
			} catch {
				cellRect = Rect.Empty;
				return false;
			}

			var localCellRect = new Rect(
				-SELECTED_CELL_MARGIN,
				-SELECTED_CELL_MARGIN,
				targetCell.ActualWidth + 2*SELECTED_CELL_MARGIN,
				targetCell.ActualHeight + 2*SELECTED_CELL_MARGIN
			);

			cellRect = cellToGridTransform.TransformBounds(localCellRect);
			return true;
		}

		private void ScrollToCell (Rect targetCell) {
			double deltaX = 0;
			double deltaY = 0;

			if (targetCell.Left < 0) {
				deltaX = targetCell.Left;
			} else if (this.ActualWidth < targetCell.Right) {
				deltaX = targetCell.Right - (this.ActualWidth);
			}

			if (targetCell.Top < 0) {
				deltaY = targetCell.Top;
			} else if (this.ActualHeight < targetCell.Bottom) {
				deltaY = targetCell.Bottom - (this.ActualHeight);
			}

			double resHorOffset = Math.Max(0, MainScrollView.HorizontalOffset + deltaX);
			double resVertOffset = Math.Max(0, MainScrollView.VerticalOffset + deltaY);

			MainScrollView.ScrollToHorizontalOffset(resHorOffset);
			MainScrollView.ScrollToVerticalOffset(resVertOffset);
		}
		#endregion

		private void UpdateGridSize () {
			AdjustWidth();
			AdjustHeight();
		}

		private void UpdateCellsDataContext () {
			for (int i = 0; i < gridStructure.Count; i++) {
				for (int j = 0; j < gridStructure[i].Count; j++) {
					((FrameworkElement)gridStructure[i][j]).DataContext = GridData[i][j];
				}
			}
		}

		private void DoNothingOnScrollWheel (Object sender, MouseWheelEventArgs e) {
			e.Handled = true;
		}

		private void MainGrid_ScrollChanged (object sender, ScrollChangedEventArgs e) {
			var mainScrollView = sender as ScrollViewer;
			if (sender == null) return;

			RowNumberScroller.ScrollToVerticalOffset(mainScrollView.VerticalOffset);
			ColumnNumberScroller.ScrollToHorizontalOffset(mainScrollView.HorizontalOffset);
		}

		private string ConfirmSelectedCellValue (string nwValue) {
			if (nwValue == null) {
				return SelectedCell;
			}

			if (!GridCoordinates.TryParse(nwValue, out GridCoordinates nwCoords)) {
				return SelectedCell;
			}

			var stringCoords = nwCoords.GetStringCoords();
			if (selectManager.SelectedCell.CellPosition.Equals(nwCoords) || selectManager.TrySelect(nwCoords)) {
				return $"{stringCoords.Item1}{stringCoords.Item2}";
			}

			var realSelectedCellPosition = selectManager.SelectedCell.CellPosition.GetStringCoords();
			return $"{realSelectedCellPosition.Item1}{realSelectedCellPosition.Item2}";
		}

		#region KeyDown event handler
		private void MainGrid_PreviewKeyDown (Object sender, KeyEventArgs e) {
			if (selectManager.SelectedCell.IsEditable) return;

			if (e.Key == Key.Enter) {
				selectManager.EditSelectedCell();
				e.Handled = true;
				return;
			}

			if ((IsKeyLetterOrDigin(e.Key) || IsKeyAllowedKey(e.Key)) && !IsModifierKeyDown()) {
				selectManager.EditSelectedCell();
				e.Handled = false;
				return;
			}

			HandleArrowKeys(e.Key);
			e.Handled = !IsModifierKeyDown();
		}

		private bool HandleArrowKeys (Key key) {
			bool cellMoved = false;
			switch (key) {
				case Key.Left: cellMoved |= selectManager.TryMoveSelection(CellSelectManager.Direction.Left); break;
				case Key.Up: cellMoved |= selectManager.TryMoveSelection(CellSelectManager.Direction.Top); break;
				case Key.Right: cellMoved |= selectManager.TryMoveSelection(CellSelectManager.Direction.Right); break;
				case Key.Down: cellMoved |= selectManager.TryMoveSelection(CellSelectManager.Direction.Bottom); break;
			}
			return cellMoved;
		}

		private bool IsKeyLetterOrDigin (Key key) {
			bool isLetter = Key.A <= key && key <= Key.Z;
			bool isDigit = Key.D0 <= key && key <= Key.D9 || Key.NumPad0 <= key && key <= Key.NumPad9;
			return isLetter || isDigit;
		}

		private bool IsKeyAllowedKey (Key key) {
			switch (key) {
				case Key.Space: return true;
				case Key.OemTilde: return true;
				case Key.OemCloseBrackets: return true;
				case Key.OemOpenBrackets: return true;
				case Key.OemMinus: return true;
				case Key.OemPlus: return true;
				case Key.OemPipe: return true;
				case Key.OemPeriod: return true;
				case Key.OemComma: return true;
				case Key.OemQuestion: return true;
				case Key.OemSemicolon: return true;
			}
			return false;
		}

		private bool IsModifierKeyDown () {
			return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ||
					Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
		}
		#endregion

		private UIElement CreateCell (Cell context, GridCoordinates cellPosition) {
			var nwCell = new GridCell(cellPosition) {
				DataContext = context
			};

			selectManager.SubscribeToCell(nwCell);
			return nwCell;
		}

		private void ReleaseCell (UIElement element) {
			var gridCell = element as GridCell;
			if (gridCell == null) return;

			selectManager.UnsubscribeCell(gridCell);
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

			UIElement nwHeader = CreateGridColumnHeaderContent(GridHeader.ColumnDefinitions.Count - 1);
			Grid.SetColumn(nwHeader, GridHeader.ColumnDefinitions.Count - 1);
			GridHeader.Children.Add(nwHeader);

			var nwGridSplitter = new GridSplitter() {
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Stretch,
				Background = new SolidColorBrush(),
				Width = 6,
			};
			Grid.SetColumn(nwGridSplitter, GridHeader.ColumnDefinitions.Count - 1);
			GridHeader.Children.Add(nwGridSplitter);

			gridHeaderStructure.Add((nwHeader, nwGridSplitter));
			Debug.Assert(gridHeaderStructure.Count == GridHeader.ColumnDefinitions.Count);
		}

		private UIElement CreateGridColumnHeaderContent (int headerIndex) {
			var columnCoords = new GridCoordinates(headerIndex, 0);

			var content = new TextBlock() {
				Text = columnCoords.GetStringCoords().Item1,
				TextAlignment = TextAlignment.Center,
			};			

			return new Border() {
				Child = content,
				BorderThickness = new Thickness(1, 0, 1, 0),
				BorderBrush = GridSplitterBrush
			};
		}

		private void FillLastColumn () {
			int targetColumn = MainGrid.ColumnDefinitions.Count - 1;

			for (var i = 0; i < gridStructure.Count; i++) {
				UIElement nwCell = CreateCell(GridData[i][targetColumn], new GridCoordinates(targetColumn, i));
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
				ReleaseCell(row[row.Count - 1]);
				row.RemoveAt(row.Count - 1);
			}
		}

		private void RemoveLastColumnFromGrid () {
			GridHeader.Children.Remove(gridHeaderStructure[gridHeaderStructure.Count - 1].Item1);
			GridHeader.Children.Remove(gridHeaderStructure[gridHeaderStructure.Count - 1].Item2);
			GridHeader.ColumnDefinitions.RemoveAt(GridHeader.ColumnDefinitions.Count - 1);

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
				Background = new SolidColorBrush(),
				Height = 6
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
				BorderThickness = new Thickness(0, 1, 0, 1)
			};
		}

		private void FillLastRow () {
			int gridWidth = MainGrid.ColumnDefinitions.Count;
			int gridHeight = MainGrid.RowDefinitions.Count;
			var nwRow = new List<UIElement>(gridWidth);

			for (int i = 0; i < gridWidth; i++) {
				UIElement nwCell = CreateCell(GridData[gridHeight - 1][i], new GridCoordinates(i, gridHeight - 1));

				Grid.SetRow(nwCell, gridHeight - 1);
				Grid.SetColumn(nwCell, i);
				
				nwRow.Add(nwCell);
				MainGrid.Children.Add(nwCell);
			}

			gridStructure.Add(nwRow);
		}

		private void RemoveLastRowCells () {
			int gridHeight = MainGrid.RowDefinitions.Count;
			foreach (var cell in gridStructure[gridHeight - 1]) {
				ReleaseCell(cell);
				MainGrid.Children.Remove(cell);
			}
			gridStructure.RemoveAt(gridStructure.Count - 1);
		}

		private void RemoveLastRowFromGrid () {
			RowNumberGrid.Children.Remove(rowNumberStructure[rowNumberStructure.Count - 1].Item1);
			RowNumberGrid.Children.Remove(rowNumberStructure[rowNumberStructure.Count - 1].Item2);
			RowNumberGrid.RowDefinitions.RemoveAt(RowNumberGrid.RowDefinitions.Count - 1);

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

		public TextBox ExpressionTextBox {
			get { return (TextBox)GetValue(ExpressionTextBoxProperty); }
			set { SetValue(ExpressionTextBoxProperty, value); }
		}

		public static readonly DependencyProperty ExpressionTextBoxProperty = DependencyProperty.Register(
			"ExpressionTextBox", typeof(TextBox), typeof(InteractiveGrid), new PropertyMetadata()
		);

		public string SelectedCell {
			get { return (string)GetValue(SelectedCellProperty); }
			set { SetValue(SelectedCellProperty, value); }
		}

		public static readonly DependencyProperty SelectedCellProperty = DependencyProperty.Register(
			"SelectedCell", typeof(string), typeof(InteractiveGrid), new PropertyMetadata("A1", SelectedCellChangedCallback), new ValidateValueCallback(ValidateSelectedCellCallback)
		);

		private static Boolean ValidateSelectedCellCallback (Object value) {
			var targetString = value as string;
			return targetString != null && GridCoordinates.TryParse(targetString, out GridCoordinates gridCoordinates);
		}

		private static void SelectedCellChangedCallback (DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var targetObject = d as InteractiveGrid;
			if (targetObject == null) return;

			var nwValue = e.NewValue as string;
			var confirmedValue = targetObject.ConfirmSelectedCellValue(nwValue);
			if (confirmedValue != nwValue) {
				targetObject.SelectedCell = confirmedValue;
			}
		}
		#endregion

		private static readonly double MIN_CELL_HEIGHT = 22;
		private static readonly double MIN_CELL_WIDTH = 100;

		private static readonly double SELECTED_CELL_MARGIN = 30;

		private CellSelectManager selectManager;

		private List<List<UIElement>> gridStructure;
		private List<(UIElement, UIElement)> gridHeaderStructure;
		private List<(UIElement, UIElement)> rowNumberStructure;
	}
}
