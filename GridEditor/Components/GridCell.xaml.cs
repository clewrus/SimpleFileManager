using SimpleFM.GridEditor.GridRepresentation;
using System;
using System.Collections.Generic;
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
	public partial class GridCell : UserControl {
		public GridCell (GridCoordinates cellPosition) {
			InitializeComponent();

			this.CellPosition = cellPosition;
			defaultStyle = new BorderStyle() {
				brush = CellBorder.BorderBrush.Clone(),
				thikness = CellBorder.BorderThickness
			};
			
			IsEditable = false;
			ChangeBinding("Value");
			ContentBox.HorizontalContentAlignment = HorizontalAlignment.Right;

			ContentBox.PreviewLostKeyboardFocus += GridCell_PreviewLostKeyboardFocus;
		}

		private void GridCell_PreviewLostKeyboardFocus (Object sender, KeyboardFocusChangedEventArgs e) {
			if (!IsEditable) return;

			var targetObject = e.NewFocus as DependencyObject;
			while (targetObject != null) {
				if (targetObject is GridCell) {
					e.Handled = true;
					return;
				}

				if (targetObject is InteractiveGrid) {
					e.Handled = true;
					return;
				}

				targetObject = VisualTreeHelper.GetParent(targetObject);
			}
		}

		public class GridCellInteractionEventArgs : EventArgs {
			public enum CellEvent { None, MouseClick, EditFinished }

			public CellEvent eventType;
			public int clickNumber;
			public GridCoordinates cellCoordinates;
		}

		private struct BorderStyle {
			public Thickness thikness;
			public Brush brush;
		}

		private void SetBorderStyle (BorderStyle style) {
			CellBorder.BorderThickness = style.thikness;
			CellBorder.BorderBrush = style.brush.Clone();
		}

		private void ChangeBinding (string bindingTarget, bool twoWay=false) {
			ContentBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
			BindingOperations.ClearBinding(ContentBox, TextBox.TextProperty);

			var nwBinding = new Binding(bindingTarget) {
				UpdateSourceTrigger = UpdateSourceTrigger.Explicit,
				Mode = (twoWay) ? BindingMode.TwoWay : BindingMode.OneWay
			};

			ContentBox.SetBinding(TextBox.TextProperty, nwBinding);
		}

		public void UncheckCell () {
			if (IsSelected) {
				IsSelected = false;
			}
			
			if (IsEditable) {
				IsEditable = false;
			}
			
			if (IsPointed) {
				IsPointed = false;
			}
		}

		#region ContentBox events calling
		private void OnGridCellFinishEditingInteractino () {
			var eventArgs = new GridCellInteractionEventArgs() {
				eventType = GridCellInteractionEventArgs.CellEvent.EditFinished
			};

			GridCellInteraction?.Invoke(this, eventArgs);
		}

		private void OnGridCellMouseClickInteraction (int clickNumber) {
			var eventArgs = new GridCellInteractionEventArgs() {
				clickNumber = clickNumber,
				cellCoordinates = CellPosition,
				eventType = GridCellInteractionEventArgs.CellEvent.MouseClick
			};

			GridCellInteraction?.Invoke(this, eventArgs);
		}

		private void TextBox_MouseDown (Object sender, MouseButtonEventArgs e) {
			if (e.ClickCount == 1) {
				OnGridCellMouseClickInteraction(1);
			}
		}

		private void TextBox_MouseDoubleClick (Object sender, MouseButtonEventArgs e) {
			OnGridCellMouseClickInteraction(2);
		}

		private void ContentBox_KeyDown (Object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter || e.Key == Key.Return) {
				OnGridCellFinishEditingInteractino();
			}
		}
		#endregion

		public event EventHandler<GridCellInteractionEventArgs> GridCellInteraction;

		private bool _IsPointed;
		public bool IsPointed {
			get => _IsPointed;
			set {
				
				if (value) {
					UncheckCell();
					SetBorderStyle(pointedStyle);
				} else {
					SetBorderStyle(defaultStyle);
				}

				_IsPointed = value;
			}
		}

		private bool _IsSelected;
		public bool IsSelected {
			get => _IsSelected;
			set {
				if (value) {
					UncheckCell();
					SetBorderStyle(selectedStyle);
				} else {
					SetBorderStyle(defaultStyle);
				}

				_IsSelected = value;
			}
		}

		private bool _IsEditable;
		public bool IsEditable {
			get => _IsEditable;
			set {
				if (value) {
					UncheckCell();
					ContentBox.IsReadOnly = false;
					ChangeBinding("ExpressionStr", true);
					ContentBox.HorizontalContentAlignment = HorizontalAlignment.Left;
					SetBorderStyle(editableStyle);
				} else {
					ContentBox.IsReadOnly = true;
					ChangeBinding("Value");
					ContentBox.HorizontalContentAlignment = HorizontalAlignment.Right;
					SetBorderStyle(defaultStyle);
				}

				_IsEditable = value;
			}
		}

		public int CaretIndex {
			get => (ContentBox == null) ? 0 : ContentBox.CaretIndex;
			set => ContentBox.CaretIndex = value;
		}

		public GridCoordinates CellPosition { get; private set; }

		private readonly BorderStyle defaultStyle;
		private readonly BorderStyle selectedStyle = new BorderStyle() { thikness = new Thickness(3), brush = new SolidColorBrush(Colors.DarkOrange) };
		private readonly BorderStyle editableStyle = new BorderStyle() { thikness = new Thickness(3), brush = new SolidColorBrush(Colors.DarkRed) };
		private readonly BorderStyle pointedStyle = new BorderStyle() { thikness = new Thickness(3), brush = new SolidColorBrush(Colors.YellowGreen) };
	}
}
