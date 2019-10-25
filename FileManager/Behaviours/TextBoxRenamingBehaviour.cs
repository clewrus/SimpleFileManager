using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace SimpleFM.FileManager.Behaviours {
	class TextBoxRenamingBehaviour : Behavior<TextBox> {
		protected override void OnAttached () {
			base.OnAttached();
			var isReadOnlyDescriptor = DependencyPropertyDescriptor.FromProperty(TextBox.IsReadOnlyProperty, typeof(TextBox));
			isReadOnlyDescriptor?.AddValueChanged(AssociatedObject, IsReadOnlyChangedHandler);

			AssociatedObject.LostFocus += LostFocusHandler;
		}

		protected override void OnDetaching () {
			base.OnDetaching();
			var isReadOnlyDescriptor = DependencyPropertyDescriptor.FromProperty(TextBox.IsReadOnlyProperty, typeof(TextBox));
			isReadOnlyDescriptor?.RemoveValueChanged(AssociatedObject, IsReadOnlyChangedHandler);

			AssociatedObject.LostFocus -= LostFocusHandler;
		}

		private void IsReadOnlyChangedHandler (object sender, EventArgs e) {
			if (AssociatedObject.IsReadOnly == false) {
				AssociatedObject.Focus();
				string currentText = AssociatedObject.Text;
				int lastDotIndex = currentText.LastIndexOf('.');

				AssociatedObject.SelectionStart = 0;
				AssociatedObject.SelectionLength = (lastDotIndex <= 0) ? currentText.Length : lastDotIndex;
			}
		}

		private void LostFocusHandler (object sender, EventArgs e) {
			AssociatedObject.IsReadOnly = true;
		}
	}
}
