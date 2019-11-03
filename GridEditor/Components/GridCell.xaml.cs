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

			IsEditable = false;
			this.cellPosition = cellPosition;
		}

		public class GridCellInteractionEventArgs : EventArgs {
			public int clickNumber;
			public GridCoordinates cellCoordinates;
		}

		private void OnGridCellInteraction (int clickNumber) {
			var eventArgs = new GridCellInteractionEventArgs() {
				clickNumber = clickNumber,
				cellCoordinates = cellPosition
			};

			GridCellInteraction?.Invoke(this, eventArgs);
		}

		private void TextBox_MouseDown (Object sender, MouseButtonEventArgs e) {
			if (e.ClickCount == 1) {
				OnGridCellInteraction(1);
			}
		}

		private void TextBox_MouseDoubleClick (Object sender, MouseButtonEventArgs e) {
			OnGridCellInteraction(2);
		}

		public event EventHandler<GridCellInteractionEventArgs> GridCellInteraction;

		private bool _IsEditable;
		public bool IsEditable {
			get => _IsEditable;
			set {
				_IsEditable = value;

				if (_IsEditable) {
					ContentBox.IsReadOnly = false;
					ContentBox.HorizontalContentAlignment = HorizontalAlignment.Left;
				} else {
					ContentBox.IsReadOnly = true;
					ContentBox.HorizontalContentAlignment = HorizontalAlignment.Right;
				}
			}
		}

		private GridCoordinates cellPosition;
	}
}
