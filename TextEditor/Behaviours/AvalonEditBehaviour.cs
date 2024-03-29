﻿using System;
using System.Windows;
using System.Windows.Interactivity;

namespace SimpleFM.Behaviours {

	// This class was taken from https://stackoverflow.com/questions/18964176/two-way-binding-to-avalonedit-document-text-using-mvvm
	// This behaviour class allows to make two-way binding to the text field of AvalonEditor component
	public sealed class AvalonEditBehaviour : Behavior<ICSharpCode.AvalonEdit.TextEditor> {
		public static readonly DependencyProperty GiveMeTheTextProperty =
			DependencyProperty.Register("GiveMeTheText", typeof(string), typeof(AvalonEditBehaviour),
			new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback));

		public string GiveMeTheText {
			get { return (string)GetValue(GiveMeTheTextProperty); }
			set { SetValue(GiveMeTheTextProperty, value); }
		}

		protected override void OnAttached () {
			base.OnAttached();
			if (AssociatedObject != null)
				AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
		}

		protected override void OnDetaching () {
			base.OnDetaching();
			if (AssociatedObject != null)
				AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
		}

		private void AssociatedObjectOnTextChanged (object sender, EventArgs eventArgs) {
			var textEditor = sender as ICSharpCode.AvalonEdit.TextEditor;
			if (textEditor != null) {
				if (textEditor.Document != null)
					GiveMeTheText = textEditor.Document.Text;
			}
		}

		private static void PropertyChangedCallback (
			DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
			var behavior = dependencyObject as AvalonEditBehaviour;
			if (behavior.AssociatedObject != null) {
				var editor = behavior.AssociatedObject as ICSharpCode.AvalonEdit.TextEditor;
				if (editor.Document != null) {
					var caretOffset = editor.CaretOffset;
					editor.Document.Text = dependencyPropertyChangedEventArgs.NewValue.ToString();
					editor.CaretOffset = caretOffset;
				}
			}
		}
	}
}
