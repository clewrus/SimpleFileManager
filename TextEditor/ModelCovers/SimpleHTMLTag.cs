using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SimpleFM.ModelCovers.TextEditor {
	public class SimpleHtmlTag : IHtmlElement, INotifyPropertyChanged {
		public SimpleHtmlTag (SimpleHtmlParsedFile parent, string initialString) {
			this.parent = parent;
			if (initialString[0] != '<' || initialString[initialString.Length - 1] != '>') {
				throw new Exception($"Wrong arguments: {initialString}");
			}

			int? nameStart = null;
			int? nameEnd = null;
			for (int i = 0; i < initialString.Length; i++) {
				if (nameStart == null && (char.IsLetterOrDigit(initialString[i]) || initialString[i] == '!')) {
					nameStart = i;
				}
				if (nameStart != null && nameEnd == null && !char.IsLetterOrDigit(initialString[i])) {
					nameEnd = i;
				}
			}

			isOpening = initialString[1] != '/';

			if (nameStart == null) {
				name = "";
				attributes = "";
			} else {
				name = initialString.Substring(nameStart.Value, nameEnd.Value - nameStart.Value);
				if (initialString[initialString.Length - 2] == '/') {
					tagType = TagType.Single;
					isOpening = false;
				}

				attributes = initialString.Substring(nameEnd.Value, initialString.Length - nameEnd.Value - ((tagType == TagType.Single)? 2: 1) );
			}
		}

		public void RemoveSelf () {
			if (removed) return;
			removed = true;

			int i = Array.IndexOf(parent.HtmlElements, this);
			if (i != -1) {
				parent.RemoveAt(i);
			}

			if (SecondTag != null) {
				SecondTag.RemoveSelf();
			}

			parent.OnContentUpdated();
		}

		private string FilterTagName (string n) {
			string filteredName = "";
			for (int i = 0; i < n.Length; i++) {
				if (char.IsLetterOrDigit(n[i])) {
					filteredName += n[i];
				}
			}
			return filteredName;
		}

		public override String ToString () {
			string tagString = "<";
			tagString += (isOpening && tagType != TagType.Single) ? "" : "/";
			tagString += $"{name}";
			tagString += (attributes.Length > 0 && !char.IsWhiteSpace(attributes[0])) ? " " : "";
			tagString += $"{attributes}";
			tagString += (tagType == TagType.Single) ? "/" : "";
			tagString += ">";

			return tagString;
		}

		private SimpleHtmlTag _SecondTag;
		public SimpleHtmlTag SecondTag {
			get => _SecondTag;
			set {
				Debug.Assert(_SecondTag == null);
				_SecondTag = value;
				tagType = TagType.Pared;
			}
		}

		public string LowerName { get => name.ToLower(); }

		public string Name { 
			get => name;
			set {
				name = FilterTagName(value);
				if (SecondTag != null) {
					SecondTag.name = name;
				}
				parent.OnContentUpdated();
			}
		}

		public string Attributes {
			get => attributes;
			set {
				attributes = value;
				parent.OnContentUpdated();
			}
		}

		public string name;
		public string attributes;
		public TagType tagType;

		public bool isOpening;

		private SimpleHtmlParsedFile parent;
		private bool removed;

		public event PropertyChangedEventHandler PropertyChanged;

		public enum TagType { Invalid, Single, Void, Pared }
	}
}
