using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.ModelCovers.TextEditor {
	public class SimpleHtmlParsedFile {
		public SimpleHtmlParsedFile (string targetText) {
			this.targetText = targetText;
			ParseSelf();
		}

		private void ParseSelf () {
			var elementList = new List<IHtmlElement>(16);
			WriteElementsToList(elementList);
			HtmlElements = elementList.ToArray();
		}

		public void OnContentUpdated () {
			ContentUpdated?.Invoke(this, new EventArgs());
		}

		public ObservableCollection<TagTreeNode> GenerateTagsTree () {
			var roots = new ObservableCollection<TagTreeNode>();
			var tagsDict = new Dictionary<string, ObservableCollection<SimpleHtmlTag>>();

			foreach (var element in HtmlElements) {
				if (element is SimpleHtmlTag tag) {
					if (!tag.isOpening && tag.SecondTag != null)
						continue;

					if (!tagsDict.ContainsKey(tag.LowerName)) {
						tagsDict.Add(tag.LowerName, new ObservableCollection<SimpleHtmlTag>());
					}
					tagsDict[tag.LowerName].Add(tag);
				}
			}

			foreach (var tagName in tagsDict.Keys) {
				var nwNode = new TagTreeNode() { CommonName = tagName, Tags = tagsDict[tagName] };
				roots.Add(nwNode);
			}

			return roots;
		}

		public override String ToString () {
			var sb = new StringBuilder();
			for (int i = 0; i < HtmlElements.Length; i++) {
				if (HtmlElements[i] != null) {
					sb.Append(HtmlElements[i].ToString());
				}
			}
			return sb.ToString();
		}

		public void RemoveAt (int i = 0) {
			HtmlElements[i] = null;
		}

		private void WriteElementsToList (List<IHtmlElement> elementList) {
			var tagStack = new Stack<SimpleHtmlTag>();

			bool searchingTag = false;
			int lastEventPosition = 0;

			bool skippingComment = false;

			bool escapingQuote = false;
			char quoteType = '\'';

			for (int i = 0; i < targetText.Length; i++) {
				char curChar = targetText[i];

				if (skippingComment) {
					if (curChar == '>' && targetText[i - 1] == '-' && targetText[i - 2] == '-') {
						skippingComment = false;
					}
					continue;
				}

				if (escapingQuote) {
					if (curChar == quoteType && targetText[i - 1] != '\\') {
						escapingQuote = false;
					}
					continue;
				}

				if (curChar == '<' && targetText.Length > i + 3 && targetText[i+1] == '!' && 
					targetText[i+2] == '-' && targetText[i+3] == '-') {
					skippingComment = true;
					continue;
				}

				if (searchingTag && (curChar == '"' || curChar == '\'')) {
					escapingQuote = true;
					quoteType = curChar;
					continue;
				}

				if (searchingTag && curChar == '>') {
					var nwTag = new SimpleHtmlTag(this, targetText.Substring(lastEventPosition, i - lastEventPosition + 1));
					elementList.Add(nwTag);

					searchingTag = false;
					lastEventPosition = i + 1;

					if (nwTag.isOpening) {
						tagStack.Push(nwTag);
					} else {
						while (tagStack.Count > 0 && nwTag.LowerName != tagStack.Peek().LowerName
							&& AllExistingVoidElements.Contains(tagStack.Peek().LowerName)) {
							tagStack.Pop().tagType = SimpleHtmlTag.TagType.Void;
						}
						
						if (tagStack.Count > 0 && tagStack.Peek().LowerName == nwTag.LowerName) {
							var openingTag = tagStack.Pop();
							openingTag.SecondTag = nwTag;
							nwTag.SecondTag = openingTag;
						}
					}
				}

				if (curChar == '-' && targetText.Length > i + 2 &&
					targetText[i + 1] != '-' && targetText[i + 2] != '>') {
					skippingComment = false;
				}

				if (curChar == '<') {
					if (i != lastEventPosition) {
						elementList.Add(new SimpleHtmlText(targetText.Substring(lastEventPosition, i - lastEventPosition)));
					}

					lastEventPosition = i;
					searchingTag = true;
				}
			}

			if (lastEventPosition < targetText.Length) {
				elementList.Add(new SimpleHtmlText(targetText.Substring(lastEventPosition, targetText.Length - lastEventPosition)));
			}
		}

		public IHtmlElement[] HtmlElements { get; private set; }
		public event EventHandler ContentUpdated;

		private string targetText;

		public static readonly HashSet<string> AllExistingHtmlElements = new HashSet<string> () {
			"!--", "!doctype", "a", "abbr", "acronym", "address", "applet", "area", "article", "aside", "audio", "b", "base",
			"basefont", "bdo", "big", "blockquote", "body", "br", "button", "canvas", "caption", "center", "cite", "code",
			"col", "colgroup", "datalist", "dd", "del", "dfn", "div", "dl", "dt", "em", "embed", "fieldset", "figcaption",
			"figure", "font", "footer", "form", "frame", "frameset", "head", "header", "h1", "", "h6", "hr", "html", "i",
			"iframe", "img", "input", "ins", "kbd", "label", "legend", "li", "link", "main", "map", "mark", "meta", "meter",
			"nav", "noscript", "object", "ol", "optgroup", "option", "p", "param", "pre", "progress", "q", "s", "samp", "script",
			"section", "select", "small", "source", "span", "strike", "strong", "style", "sub", "sup", "table", "tbody", "td",
			"textarea", "tfoot", "th", "thead", "time", "title", "tr", "u", "ul", "var", "video", "wbr"
		};

		public static readonly HashSet<string> AllExistingVoidElements = new HashSet<string>() {
			"area", "base", "br", "col", "command", "embed", "hr", "img", "input", "keygen", "link", "meta",
			"param", "source", "track", "wbr"
		};
	}
}
