using SimpleFM.GridEditor.ExpressionParsing;
using SimpleFM.GridEditor.GridRepresentation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleFM.GridEditor.Components {
	public class CellSelectManager {
		public CellSelectManager () {
			watchedCells = new Dictionary<GridCoordinates, GridCell>();
			includedCells = new HashSet<GridCell>();
		}

		public class SelectedCellChangedEventArgs : EventArgs {
			public SelectedCellChangedEventArgs (GridCell changedCell) {
				this.ChangedCell = changedCell;	
			}

			public GridCell ChangedCell { get; private set; }
		}

		private void OnSelectedCellChanged (GridCell nwSelectedCell) {
			SelectedCellChanged?.Invoke(this, new SelectedCellChangedEventArgs(nwSelectedCell));
		}

		#region Cells Adding and Removing
		public bool SubscribeToCell (GridCell nwCell) {
			if (watchedCells.ContainsKey(nwCell.CellPosition) && watchedCells[nwCell.CellPosition] == nwCell) {
				return false;
			}

			if (SelectedCell == null) {
				SelectedCell = nwCell;
				SelectedCell.IsSelected = true;
			}

			nwCell.GridCellInteraction += GridCellInteractionHandler;
			watchedCells.Add(nwCell.CellPosition, nwCell);

			return true;
		}

		public bool UnsubscribeCell (GridCell oldCell) {
			if (!watchedCells.ContainsKey(oldCell.CellPosition)) {
				return false;
			}

			oldCell.GridCellInteraction -= GridCellInteractionHandler;
			watchedCells.Remove(oldCell.CellPosition);

			if (PointedCell == oldCell) {
				oldCell.IsPointed = false;
				PointedCell = null;
			}

			if (SelectedCell == oldCell) {
				SellectOtherCell();
			}

			return true;
		}

		private void SellectOtherCell () {
			SelectedCell.IsSelected = false;
			
			foreach (var someCell in watchedCells) {
				SelectedCell = someCell.Value;
				SelectedCell.IsSelected = true;
				break;
			}

			UpdateBinding();
		}
		#endregion

		#region ExpressionTextBox event handlers
		private void SubscribeExpressionTextBox () {
			ExpressionTextBox.IsKeyboardFocusWithinChanged += ExpressionTextBoxKeyboardFocusWithinChangedHandler;
			ExpressionTextBox.TextChanged += ExpressionTextBoxChangedHandler;
			ExpressionTextBox.KeyDown += ExpressionTextBoxKeyDownHandler;
			ExpressionTextBox.PreviewLostKeyboardFocus += ExpressionTextBoxLostKeyboardFocusHandler;
		}

		private void UnsubscribeExpressionTextBox () {
			ExpressionTextBox.IsKeyboardFocusWithinChanged -= ExpressionTextBoxKeyboardFocusWithinChangedHandler;
			ExpressionTextBox.TextChanged -= ExpressionTextBoxChangedHandler;
			ExpressionTextBox.KeyDown -= ExpressionTextBoxKeyDownHandler;
			ExpressionTextBox.PreviewLostKeyboardFocus -= ExpressionTextBoxLostKeyboardFocusHandler;
		}

		private void ExpressionTextBoxKeyboardFocusWithinChangedHandler (object sender, DependencyPropertyChangedEventArgs e) {
			if (sender != ExpressionTextBox) return;

			if (ExpressionTextBox.IsKeyboardFocusWithin && SelectedCell != null && !SelectedCell.IsEditable) {
				SelectedCell.IsEditable = true;
				UpdateBinding();
			}
		}

		private void ExpressionTextBoxChangedHandler (object sender, TextChangedEventArgs e) {
			if (sender != ExpressionTextBox) return;

			UpdateIncludedCells(ExpressionTextBox.Text);

			if (!settingPointedCell) {
				resentlyPointed = false;
			}
		}

		private void ExpressionTextBoxKeyDownHandler (object sender, KeyEventArgs e) {
			if (sender != ExpressionTextBox) return;
			if (SelectedCell == null) return;

			if (e.Key == Key.Enter || e.Key == Key.Return) {

				try {
					Keyboard.ClearFocus();
					Keyboard.Focus(SelectedCell);
				} catch { }
				
				SelectedCell.IsEditable = false;
				SelectedCell.IsSelected = true;

				UnpointCell();
				UpdateBinding();
			}
		}

		private void ExpressionTextBoxLostKeyboardFocusHandler (object sender, KeyboardFocusChangedEventArgs e) {
			if (SelectedCell == null || !SelectedCell.IsEditable)
				return;

			var targetObject = e.NewFocus as DependencyObject;
			while (targetObject != null) {
				if (targetObject is GridCell newFocusOwner) {
					e.Handled = (newFocusOwner != SelectedCell);
					return;
				}

				if (targetObject is InteractiveGrid) {
					e.Handled = true;
					return;
				}

				targetObject = VisualTreeHelper.GetParent(targetObject);
			}
		}
		#endregion

		private void UpdateIncludedCells (string text) {
			foreach (var cell in includedCells) {
				cell.IsIncluded = false;
			}
			includedCells.Clear();

			var tokens = Tokenizer.Instance.TokenizeString(text);
			if (tokens.Count > 0 && tokens.First.Value is OperationToken opToken && opToken.Value == OperationToken.Operation.FormulaSign) {
				foreach (var token in tokens) {
					if (token is CellNameToken cellToken && watchedCells.ContainsKey(cellToken.Value)) {
						var curCell = watchedCells[cellToken.Value];
						curCell.IsIncluded = true;
						includedCells.Add(curCell);
					}
				}
			}
		}

		private void UpdateBinding () {
			if (ExpressionTextBox == null || SelectedCell == null) return;

			ExpressionTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
			BindingOperations.ClearBinding(ExpressionTextBox, TextBox.TextProperty);

			Binding textBinding = null;
			if (SelectedCell.IsEditable) {
				textBinding = new Binding("Text") {
					Source = SelectedCell.ContentBox,
					Mode = BindingMode.TwoWay,
					UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
				};
			} else {
				textBinding = new Binding("ExpressionStr") {
					Source = SelectedCell.DataContext,
					Mode = BindingMode.OneWay,
					UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
				};
			}

			ExpressionTextBox.SetBinding(TextBox.TextProperty, textBinding);
		}

		private bool ContainsFormula (GridCell targetCell) {
			if (targetCell == null || targetCell.ContentBox == null) {
				return false;
			}

			string targetText = targetCell.ContentBox.Text;
			if (targetText != null && targetText.Length == 0) {
				return false;
			}

			return targetText[0] == '=';
		}

		private int FindCurrentCaretIndex () {
			if (ExpressionTextBox.IsKeyboardFocusWithin) {
				return ExpressionTextBox.CaretIndex;
			} else if (SelectedCell != null && SelectedCell.IsEditable && SelectedCell.IsKeyboardFocusWithin) {
				return SelectedCell.CaretIndex;
			}

			return 0;
		}

		private void SetCurrentCaretIndex (int nwIndex) {
			if (ExpressionTextBox.IsKeyboardFocusWithin) {
				ExpressionTextBox.CaretIndex = nwIndex;
			} else if (SelectedCell != null && SelectedCell.IsEditable && SelectedCell.IsKeyboardFocusWithin) {
				SelectedCell.CaretIndex = nwIndex;
			}
		}

		public bool TryMoveSelection (Direction dir) {
			if (SelectedCell == null) {
				return false;
			}

			if (!CoordsInDirectionExist(dir, out GridCoordinates targetCoordinates)) {
				return false;
			}

			SelectedCell.UncheckCell();
			SelectedCell = watchedCells[targetCoordinates];
			SelectedCell.IsSelected = true;

			UnpointCell();
			UpdateBinding();

			return true;
		}

		public bool TrySelect (GridCoordinates targetCell) {
			if (!watchedCells.ContainsKey(targetCell)) return false;
			if (targetCell.Equals(SelectedCell.CellPosition)) return false;

			UnpointCell();

			SelectedCell.UncheckCell();
			SelectedCell = watchedCells[targetCell];
			SelectedCell.IsSelected = true;

			UpdateBinding();
			return true;
		}

		public void EditSelectedCell () {
			if (SelectedCell == null || SelectedCell.IsEditable) return;

			SelectedCell.IsEditable = true;
			UpdateBinding();
			try {
				Keyboard.Focus(SelectedCell.ContentBox);
			} catch { }
		}

		private bool CoordsInDirectionExist (Direction dir, out GridCoordinates targetCoordinates) {
			(int, int) position = SelectedCell.CellPosition.GetNumericCoords();
			switch (dir) {
				case Direction.Left: position.Item1 -= 1; break;
				case Direction.Top: position.Item2 -= 1; break;
				case Direction.Right: position.Item1 += 1; break;
				case Direction.Bottom: position.Item2 += 1; break;
			}

			targetCoordinates = new GridCoordinates(position.Item1, position.Item2);

			if (position.Item1 < 0 || position.Item2 < 0) {
				return false;
			}

			if (!watchedCells.ContainsKey(targetCoordinates)) {
				return false;
			}

			return true;
		}

		#region Cell pointing
		private void UnpointCell () {
			if (PointedCell != null) {
				PointedCell.IsPointed = false;
				PointedCell = null;
			}
		}

		private bool PointingIsAllowed () {
			bool hasKeyboardFocus = ExpressionTextBox.IsKeyboardFocusWithin;
			hasKeyboardFocus = hasKeyboardFocus || (SelectedCell != null && SelectedCell.IsKeyboardFocusWithin);

			if (!hasKeyboardFocus) {
				return false;
			}

			int carInd = FindCurrentCaretIndex();
			bool repeatPointing = resentlyPointed && carInd == lastCaretIndex;

			if (!ContainsFormula(SelectedCell) && !repeatPointing) {
				return false;
			}
			
			string curText = ExpressionTextBox.Text;
			bool prevSymbolIsFree = !Char.IsLetterOrDigit(curText[Math.Max(0, carInd - 1)]);
			bool nextSymbolIsFree = !Char.IsLetterOrDigit(curText[Math.Min(carInd, curText.Length - 1)]);

			return repeatPointing || (prevSymbolIsFree && nextSymbolIsFree);
		}

		private void PointToCell (GridCell pointedCell) {
			int curCaretIndex = FindCurrentCaretIndex();
			if (!resentlyPointed || curCaretIndex != lastCaretIndex) {
				resentlyPointed = true;
				
				string currentExpression = ExpressionTextBox.Text;
				textBeforePointing = (currentExpression.Substring(0, curCaretIndex), currentExpression.Substring(curCaretIndex));
			}

			AddPointedCellNameToExpressionTextBox(pointedCell, out int nwCaretIndex);
			SetCurrentCaretIndex(nwCaretIndex);
			lastCaretIndex = nwCaretIndex;

			UnpointCell();
			PointedCell = pointedCell;
			PointedCell.IsPointed = true;
		}

		private void AddPointedCellNameToExpressionTextBox (GridCell pointedCell, out int nwCaretIndex) {
			settingPointedCell = true;

			(string, string) targetCoords = pointedCell.CellPosition.GetStringCoords();
			string cellName = $"{targetCoords.Item1}{targetCoords.Item2}";
			ExpressionTextBox.Text = $"{textBeforePointing.Prefix}{cellName}{textBeforePointing.Suffix}";

			nwCaretIndex = textBeforePointing.Prefix.Length + cellName.Length;
			settingPointedCell = false;
		}
		#endregion

		#region Cell event handlers
		private void GridCellInteractionHandler (Object sender, GridCell.GridCellInteractionEventArgs e) {
			var targetCell = sender as GridCell;
			if (targetCell == null) return;

			if (e.eventType == GridCell.GridCellInteractionEventArgs.CellEvent.MouseClick) {
				if (e.clickNumber == 1) {
					GridCellMouseClickHandler(targetCell, e);
				} else if (e.clickNumber == 2) {
					GridCellMouseDoubleClickHandler(targetCell, e);
				}
				
			} else if (e.eventType == GridCell.GridCellInteractionEventArgs.CellEvent.EditFinished) {
				GridCellEditFinishedHandler(targetCell, e);
			}
		}

		private void GridCellMouseClickHandler (GridCell targetCell, GridCell.GridCellInteractionEventArgs e) {
			if (SelectedCell == targetCell) return;
			UnpointCell();

			if (SelectedCell.IsEditable) {
				if (PointingIsAllowed()) {
					PointToCell(targetCell);
					return;
				} else {
					SelectedCell.IsEditable = false;
					SelectedCell = targetCell;
					SelectedCell.IsSelected = true;
				}
			} else {
				SelectedCell.IsSelected = false;
				SelectedCell = targetCell;
				SelectedCell.IsSelected = true;
			}

			UpdateBinding();
		}

		private void GridCellMouseDoubleClickHandler (GridCell targetCell, GridCell.GridCellInteractionEventArgs e) {
			if (SelectedCell == targetCell && SelectedCell.IsEditable) return;
			UnpointCell();

			SelectedCell.UncheckCell();
			SelectedCell = targetCell;
			SelectedCell.IsEditable = true;

			UpdateBinding();
		}

		private void GridCellEditFinishedHandler (GridCell gridCell, GridCell.GridCellInteractionEventArgs e) {
			if (gridCell == null) return;
			if (gridCell != SelectedCell) return;
			if (!gridCell.IsEditable) return;

			UnpointCell();

			gridCell.IsEditable = false;
			gridCell.IsSelected = true;
			UpdateBinding();

			TryMoveSelection(Direction.Bottom);
		}
		#endregion

		public enum Direction { Left, Top, Right, Bottom }

		private TextBox _ExpressionTextBox;
		public TextBox ExpressionTextBox {
			get => _ExpressionTextBox;
			set {
				if (_ExpressionTextBox != null) {
					BindingOperations.ClearBinding(_ExpressionTextBox, TextBox.TextProperty);
					UnsubscribeExpressionTextBox();
				}

				resentlyPointed = false;
				_ExpressionTextBox = value;

				if (_ExpressionTextBox != null) {
					SubscribeExpressionTextBox();
				}

				UpdateBinding();
			}
		}

		private GridCell _SelectedCell;
		public GridCell SelectedCell { 
			get => _SelectedCell;
			private set {
				_SelectedCell = value;
				OnSelectedCellChanged(_SelectedCell);
			}
		
		}
		public GridCell PointedCell { get; private set; }

		public event EventHandler<SelectedCellChangedEventArgs> SelectedCellChanged;

		private bool resentlyPointed;
		private int lastCaretIndex;
		private (string Prefix, string Suffix) textBeforePointing;
		private bool settingPointedCell;

		private Dictionary<GridCoordinates, GridCell> watchedCells;
		private HashSet<GridCell> includedCells;
	}
}
