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
			SelectedCell = null;
			foreach (var someCell in watchedCells) {
				SelectedCell = someCell.Value;
				SelectedCell.IsSelected = true;
				break;
			}
		}
		#endregion

		#region ExpressionTextBox event handlers
		private void ExpressionTextBoxKeyboardFocusWithinChangedHandler (object sender, DependencyPropertyChangedEventArgs e) {
			if (sender != ExpressionTextBox) return;

			if (ExpressionTextBox.IsKeyboardFocusWithin && SelectedCell != null && !SelectedCell.IsEditable) {
				SelectedCell.IsEditable = true;
				UpdateBinding();
			}
		}

		private void ExpressionTextBoxChangedHandler (object sender, TextChangedEventArgs e) {
			if (sender != ExpressionTextBox) return;
			
			if (!settingPointedCell) {
				resentlyPointed = false;
			}
		}

		private void ExpressionTextBoxKeyDownHandler (object sender, KeyEventArgs e) {
			if (sender != ExpressionTextBox) return;
			if (SelectedCell == null) return;

			if (e.Key == Key.Enter || e.Key == Key.Return) {
				Keyboard.ClearFocus();
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

		private void AddPointedCellNameToExpressionTextBox (GridCell pointedCell, out int nwCaretIndex) {
			settingPointedCell = true;

			(string, string) targetCoords = pointedCell.CellPosition.GetStringCoords();
			string cellName = $"{targetCoords.Item1}{targetCoords.Item2}";
			ExpressionTextBox.Text = $"{textBeforePointing.Prefix}{cellName}{textBeforePointing.Suffix}";

			nwCaretIndex = textBeforePointing.Prefix.Length + cellName.Length;
			settingPointedCell = false;
		}

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

			Debug.Assert(gridCell == SelectedCell);
			Debug.Assert(gridCell.IsEditable);

			UnpointCell();

			Keyboard.ClearFocus();
			gridCell.IsEditable = false;
			gridCell.IsSelected = true;
			UpdateBinding();
		}
		#endregion

		private TextBox _ExpressionTextBox;
		public TextBox ExpressionTextBox {
			get => _ExpressionTextBox;
			set {
				if (_ExpressionTextBox != null) {
					BindingOperations.ClearBinding(_ExpressionTextBox, TextBox.TextProperty);
					_ExpressionTextBox.IsKeyboardFocusWithinChanged -= ExpressionTextBoxKeyboardFocusWithinChangedHandler;
					_ExpressionTextBox.TextChanged -= ExpressionTextBoxChangedHandler;
					_ExpressionTextBox.KeyDown -= ExpressionTextBoxKeyDownHandler;
					_ExpressionTextBox.PreviewLostKeyboardFocus -= ExpressionTextBoxLostKeyboardFocusHandler;
				}

				resentlyPointed = false;
				_ExpressionTextBox = value;

				if (_ExpressionTextBox != null) {
					_ExpressionTextBox.IsKeyboardFocusWithinChanged += ExpressionTextBoxKeyboardFocusWithinChangedHandler;
					_ExpressionTextBox.TextChanged += ExpressionTextBoxChangedHandler;
					_ExpressionTextBox.KeyDown += ExpressionTextBoxKeyDownHandler;
					_ExpressionTextBox.PreviewLostKeyboardFocus += ExpressionTextBoxLostKeyboardFocusHandler;
				}

				UpdateBinding();
			}
		}

		public GridCell SelectedCell { get; private set; }
		public GridCell PointedCell { get; private set; }

		private bool resentlyPointed;
		private int lastCaretIndex;
		private (string Prefix, string Suffix) textBeforePointing;
		private bool settingPointedCell;

		private Dictionary<GridCoordinates, GridCell> watchedCells;
	}
}
