using SimpleFM.Commands;
using SimpleFM.ModelCovers;
using SimpleFM.ModelCovers.TextEditor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SimpleFM.ViewModels {
	public class TextEditorViewModel : ViewModelBase {
		public TextEditorViewModel (SFMFile targetFile) {
			this.targetFile = targetFile;
			UpdateFileContent();
			UpdateFileTags();

			TagsAlteringIsEnabled = false;
		}

		private void UpdateFileContent () {
			try {
				string currentFileContent = File.ReadAllText(targetFile.ElementPath);
				FileContent = currentFileContent;
				CanSave = false;
			} catch (IOException) { } catch (UnauthorizedAccessException) { }
		}

		private void UpdateFileTags () {
			if (FileContent == null) return;
			CurParsedFile = new SimpleHtmlParsedFile(FileContent);
			FileTags = CurParsedFile.GenerateTagsTree();
		}

		private void ContentUpdatedHandler (object sender, EventArgs e) {
			SetFileContentInvisibleForFileTags = CurParsedFile.ToString();
		}

		private void RemoveNonexistingTagsTask () {
			var parsedHtml = new SimpleHtmlParsedFile(FileContent);

			for (int i = 0; i < parsedHtml.HtmlElements.Length; i++) {
				if (parsedHtml.HtmlElements[i] is SimpleHtmlTag tag) {
					if (!SimpleHtmlParsedFile.AllExistingHtmlElements.Contains(tag.LowerName)) {
						parsedHtml.RemoveAt(i);
					}
				}
			}

			string s = parsedHtml.ToString();

			App.Current.Dispatcher.Invoke(() => {
				FileContent = s;
				ProcessingFile = false;
			});
		}


		private void CapitializeLeadingLettersTask () {
			char[] currentText = FileContent.ToCharArray();
			CapitalizeLeadingLettersOfCharArray(currentText);

			string modifiedText = new string(currentText);

			App.Current.Dispatcher.Invoke(() => {
				FileContent = modifiedText;
				ProcessingFile = false;
			});
		}

		private void CapitalizeLeadingLettersOfCharArray (char[] textArray) {
			bool insideTagDeclaration = false;
			bool afterSentence = true;
			bool atWordBegining = true;

			for (int i = 0; i < textArray.Length; i++) {
				char curChar = textArray[i];

				if (curChar == '<') {
					insideTagDeclaration = true;
					atWordBegining = true;
					afterSentence = true;
					continue;
				} else if (curChar == '>') {
					insideTagDeclaration = false;
					atWordBegining = true;
					afterSentence = true;
					continue;
				} else if (curChar == '.' || curChar == '!' || curChar == '?') {
					atWordBegining = true;
					afterSentence = true;
					continue;
				} else if (char.IsWhiteSpace(curChar)) {
					atWordBegining = true;
					continue;
				}

				if (!insideTagDeclaration && afterSentence && atWordBegining && char.IsLetter(curChar)) {
					textArray[i] = char.ToUpper(curChar);
				}

				if (char.IsLetterOrDigit(curChar)) {
					atWordBegining = false;
					afterSentence = false;
				}
			}
		}

		#region Properties

		private string _FileContent;
		public string FileContent {
			get => _FileContent;
			set {
				if (value == _FileContent)
					return;
				CanSave = true;
				SetProperty(ref _FileContent, value);
				UpdateFileTags();
			}
		}

		private string SetFileContentInvisibleForFileTags {
			set {
				_FileContent = value;
				CanSave = true;
				OnPropertyChanged("FileContent");
			}
		}

		private bool _CanSave;
		public bool CanSave {
			get => _CanSave;
			private set => SetProperty(ref _CanSave, value);
		}

		private bool _ProcessingFile;
		public bool ProcessingFile {
			get => _ProcessingFile;
			set => SetProperty(ref _ProcessingFile, value);
		}

		private ObservableCollection<TagTreeNode> _FileTags;
		public ObservableCollection<TagTreeNode> FileTags {
			get => _FileTags;
			set => SetProperty(ref _FileTags, value);
		}

		private bool _TagsAlteringIsEnabled;
		public bool TagsAlteringIsEnabled {
			get => _TagsAlteringIsEnabled;
			set {
				if (value == TagsAlteringIsEnabled) return;
				SetProperty(ref _TagsAlteringIsEnabled, value);
				UpdateFileTags();

				if (TagsAlteringIsEnabled) {
					TreeModeButtonContent = "Stop altering tags";
				} else {
					TreeModeButtonContent = "Start altering tags";
				}
			}
		}

		private string _TreeModeButtonContent;
		public string TreeModeButtonContent {
			get => _TreeModeButtonContent ?? "Start altering tags";
			set => SetProperty(ref _TreeModeButtonContent, value);
		}

		public string SyntaxHighlightingTarget { get => "HTML"; }
		#endregion

		#region Commands
		public ICommand SaveCommand {
			get => new ViewModelCommand(
				(arg) => {
					try {
						File.WriteAllText(targetFile.ElementPath, FileContent);
						CanSave = false;
					} catch (IOException) { }
				},
				(arg) => CanSave && !ProcessingFile
			);
		}

		public ICommand CapitalizeLeadingLetters {
			get => new ViewModelCommand(
				(arg) => {
					ProcessingFile = true;
					Task.Run(() => CapitializeLeadingLettersTask());
				},
				(arg) => !ProcessingFile
			);
		}

		public ICommand RemoveNonexistingTags {
			get => new ViewModelCommand(
				(arg) => {
					ProcessingFile = true;
					try {
						RemoveNonexistingTagsTask();
					} catch (Exception e) {
						MessageBox.Show($"Something went wrong \n {e.Message}");
						ProcessingFile = false;
					}
					
				},
				(arg) => !ProcessingFile
			);
		}

		public ICommand UpdateTagTree {
			get => new ViewModelCommand(
				(arg) => {
					UpdateFileTags();
				}
			);
		}

		public ICommand RemoveTag {
			get => new ViewModelCommand(
				(arg) => {
					if (arg is SimpleHtmlTag tag) {
						try {
							tag.RemoveSelf();
						} catch (Exception) { }
					}
				},
				(arg) => TagsAlteringIsEnabled
			);
		}

		public ICommand SwitchTagsAlteringMode {
			get => new ViewModelCommand(
				(arg) => {
					TagsAlteringIsEnabled = !TagsAlteringIsEnabled;
				}
			);
		}
		#endregion

		private SimpleHtmlParsedFile _CurParsedFile;
		private SimpleHtmlParsedFile CurParsedFile {
			get => _CurParsedFile;
			set {
				if (_CurParsedFile != null) {
					_CurParsedFile.ContentUpdated -= ContentUpdatedHandler;
				}
				
				_CurParsedFile = value;

				if (_CurParsedFile != null) {
					_CurParsedFile.ContentUpdated += ContentUpdatedHandler;
				}
			}
		}
		private SFMFile targetFile;
	}
}
