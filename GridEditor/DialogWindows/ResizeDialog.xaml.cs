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
		public ResizeDialog (int initWidth, int initHeight) {
			InitializeComponent();

			this.initWidth = initWidth;
			this.initHeight = initHeight;

			HeightField.Text = initHeight.ToString();
			WidthField.Text = initWidth.ToString();
		}

		public ResizeDialog (
							int initWidth,
							int initHeight,
							Predicate<int> IsValidWidth,
							Predicate<int> IsValidHeight) : this(initWidth, initHeight)
		{
			this.IsValidWidth = IsValidWidth;
			this.IsValidHeight = IsValidHeight;
		}

		public ResizeDialog (
							int initWidth,
							int initHeight,
							(int minWidth, int maxWidth) widthBounds,
							(int minHeight, int maxHeight) heightBounds) : this(initWidth, initHeight)
		{
			this.widthBounds = widthBounds;
			this.heightBounds = heightBounds;
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

		private int EvaluateValidWidth (int width) {
			if (widthBounds != null) {
				var bounds = widthBounds.Value;
				return Math.Min(Math.Max(bounds.minWidth, width), bounds.maxWidth);
			}

			if (IsValidWidth != null && IsValidWidth(width)) {
				return width;
			}

			return initWidth;
		}

		private int EvaluateValidHeight (int height) {
			if (heightBounds != null) {
				var bounds = heightBounds.Value;
				return Math.Min(Math.Max(bounds.minHeight, height), bounds.maxHeight);
			}

			if (IsValidHeight != null && IsValidHeight(height)) {
				return height;
			}

			return initHeight;
		}

		public int ResultWidth {
			get {
				if (!int.TryParse(WidthField.Text, out int enteredWidth)) {
					return initWidth;
				}

				return EvaluateValidWidth(enteredWidth);
			}
		}

		public int ResultHeight {
			get {
				if (!int.TryParse(HeightField.Text, out int enteredHeight)) {
					return initHeight;
				}

				return EvaluateValidHeight(enteredHeight);
			}
		}

		private Predicate<int> IsValidWidth;
		private Predicate<int> IsValidHeight;

		private (int minWidth, int maxWidth)? widthBounds;
		private (int minHeight, int maxHeight)? heightBounds;

		private int initWidth;
		private int initHeight;
	}
}
