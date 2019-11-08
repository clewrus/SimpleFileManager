using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace SimpleFM.GridEditor.Behaviors {
	public class EnterUpdateTextBoxBehavior : Behavior<TextBox> {
		protected override void OnAttached () {
			base.OnAttached();

			AssociatedObject.KeyDown += KeyDownHandler;
			AssociatedObject.GotKeyboardFocus += GotKeyboardFocusHandler;
			AssociatedObject.LostKeyboardFocus += LostKeyboardFocusHandler;
		}

		protected override void OnDetaching () {
			base.OnDetaching();

			AssociatedObject.KeyDown -= KeyDownHandler;
			AssociatedObject.GotKeyboardFocus -= GotKeyboardFocusHandler;
			AssociatedObject.LostKeyboardFocus -= LostKeyboardFocusHandler;
		}

		private void LostKeyboardFocusHandler (Object sender, KeyboardFocusChangedEventArgs e) {
			if (!bindingWasUpdated) {
				AssociatedObject.Text = textAtGettingFocus;
			}
			BindingOperations.GetBindingExpression(AssociatedObject, TextBox.TextProperty)?.UpdateTarget();

			if (previousFocusedElement != null && previousFocusedElement != AssociatedObject) {
				try {
					Keyboard.Focus(previousFocusedElement);
				} catch { }
			}
		}

		private void GotKeyboardFocusHandler (Object sender, KeyboardFocusChangedEventArgs e) {
			bindingWasUpdated = false;		
			textAtGettingFocus = AssociatedObject.Text;
			previousFocusedElement = e.OldFocus;
		}

		private void KeyDownHandler (Object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter && AssociatedObject.IsKeyboardFocusWithin) {
				var binding = BindingOperations.GetBindingExpression(AssociatedObject, TextBox.TextProperty);
				if (binding == null) return;

				binding.UpdateSource();
				bindingWasUpdated = true;

				Keyboard.ClearFocus();
			}
		}

		private IInputElement previousFocusedElement;
		private bool bindingWasUpdated;
		private string textAtGettingFocus;
	}
}
