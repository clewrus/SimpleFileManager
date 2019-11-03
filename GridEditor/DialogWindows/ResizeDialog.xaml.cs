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
using System.Windows.Shapes;

namespace SimpleFM.GridEditor.DialogWindows {

	public partial class ResizeDialog : Window {
		public ResizeDialog (int initWidth, int initHeight, Predicate<int> IsValidWidth, Predicate<int> IsValidHeight) {
			InitializeComponent();

			this.initWidth = initWidth;
			this.initHeight = initHeight;

			this.IsValidWidth = IsValidWidth;
			this.IsValidHeight = IsValidHeight;

			HeightField.Text = initHeight.ToString();
			WidthField.Text = initWidth.ToString();
		}

		private void BtnDialogOk_Click (Object sender, RoutedEventArgs e) {
			this.DialogResult = true;
		}

		private void WidthField_LostFocus (Object sender, RoutedEventArgs e) {
			WidthField.Text = ResultWidth.ToString();
		}

		private void HeightField_LostFocus (Object sender, RoutedEventArgs e) {
			HeightField.Text = ResultHeight.ToString();
		}

		public int ResultWidth {
			get {
				if (!int.TryParse(WidthField.Text, out int enteredWidth)) {
					return initWidth;
				}

				if (!IsValidWidth(enteredWidth)) {
					return initWidth;
				}

				return enteredWidth;
			}
		}

		public int ResultHeight {
			get {
				if (!int.TryParse(HeightField.Text, out int enteredHeight)) {
					return initHeight;
				}

				if (!IsValidHeight(enteredHeight)) {
					return initHeight;
				}

				return enteredHeight;
			}
		}

		private Predicate<int> IsValidWidth;
		private Predicate<int> IsValidHeight;

		private int initWidth;
		private int initHeight;
	}
}
