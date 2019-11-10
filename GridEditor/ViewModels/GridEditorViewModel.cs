using Microsoft.Win32;
using SimpleFM.Common.Commands;
using SimpleFM.Common.ViewModels;
using SimpleFM.FileManager.ModelCovers;
using SimpleFM.GridEditor.DialogWindows;
using SimpleFM.GridEditor.GridRepresentation;
using SimpleFM.ModelCovers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SimpleFM.GridEditor.ViewModels {
	public class GridEditorViewModel : ViewModelBase, INotifyPropertyChanged {
		public GridEditorViewModel (SFMFile targetFile) {
			GridRepresentation = new HistoryCalculatingGrid(20, 30);
		}

		private void GridChangedHandler (object sender, EventArgs e) {
			RecentlySaved = false;
		}

		#region Dialogs
		private void GetNewSize (out int width, out int height) {
			width = GridRepresentation.Width;
			height = GridRepresentation.Height;

			var resizeDialog = new ResizeDialog(
				width,
				height,
				(minWidth: 1, maxWidth: HistoryCellGrid.MAX_GRID_WIDTH),
				(minHeight: 1, maxHeight: HistoryCellGrid.MAX_GRID_HEIGHT)
			);

			if (resizeDialog.ShowDialog() == true) {
				width = resizeDialog.ResultWidth;
				height = resizeDialog.ResultHeight;
			}
		}

		private SFMFile SelectSaveFilePath () {
			var saveFileDialog = new SaveFileDialog();

			saveFileDialog.DefaultExt = ".sfmgrid";
			saveFileDialog.Title = "Chose SFM Grid file name";
			saveFileDialog.AddExtension = true;
			saveFileDialog.CheckFileExists = false;
			saveFileDialog.CheckPathExists = false;
			saveFileDialog.CreatePrompt = false;
			
			if (saveFileDialog.ShowDialog() != null) {
				return (string.IsNullOrEmpty(saveFileDialog.FileName))? null : new SFMFile(saveFileDialog.FileName);
			}

			return null;
		}

		private SFMFile SelectOpenFilePath () {
			var openFileDialog = new OpenFileDialog();
			openFileDialog.DefaultExt = ".sfmgrid";
			openFileDialog.Title = "Select SFM Grid file";
			openFileDialog.AddExtension = true;
			openFileDialog.CheckFileExists = false;
			openFileDialog.Multiselect = false;
			
			if (openFileDialog.ShowDialog() != null) {
				return (string.IsNullOrEmpty(openFileDialog.FileName)) ? null : new SFMFile(openFileDialog.FileName);
			}

			return null;
		}
		#endregion

		#region Preperties
		private SFMFile _OpenedFile;
		public SFMFile OpenedFile {
			get => _OpenedFile;
			set {
				SetProperty(ref _OpenedFile, value);
				OnPropertyChanged("OpenedFileName");
			}
		}

		public string OpenedFileName {
			get => (OpenedFile == null) ? "Unsaved File" : $"{OpenedFile.ElementName} {((RecentlySaved) ? "" : "*")}";
		}

		private bool _RecentlySaved;
		public bool RecentlySaved {
			get => _RecentlySaved;
			set {
				SetProperty(ref _RecentlySaved, value);
				OnPropertyChanged("OpenedFileName");
			}
		}

		private bool _ShowExpressionOnly;
		public bool ShowExpressionOnly {
			get => _ShowExpressionOnly;
			set => SetProperty(ref _ShowExpressionOnly, value);
		}

		private HistoryCellGrid _GridRepresentation;
		public HistoryCellGrid GridRepresentation {
			get => _GridRepresentation;
			set {
				if (_GridRepresentation != null) {
					_GridRepresentation.GridChanged -= GridChangedHandler;
				}
				SetProperty(ref _GridRepresentation, value);
				if (_GridRepresentation != null) {
					_GridRepresentation.GridChanged += GridChangedHandler;
				}
			}
		}
		#endregion

		#region Commands
		public ICommand NewCommand {
			get => new ViewModelCommand(
				(arg) => {
					GridRepresentation = new HistoryCalculatingGrid(20, 30);
					OpenedFile = null;
					RecentlySaved = false;
				}
			);
		}

		public ICommand OpenCommand {
			get => new ViewModelCommand(
				(arg) => {
					SFMFile selectedFile = SelectOpenFilePath();
					if (selectedFile == null) return;

					try {
						var nwGrid = new HistoryCalculatingGrid(selectedFile);
						GridRepresentation = nwGrid;

						OpenedFile = selectedFile;
						RecentlySaved = true;
					} catch {
						MessageBox.Show($"Can't open the file:\n {selectedFile.ElementPath}");
					}
				}
			);
		}

		public ICommand SaveCommand {
			get => new ViewModelCommand(
				(arg) => {
					try {
						GridRepresentation.SaveToFile(OpenedFile);
						RecentlySaved = true;
					} catch (Exception e) {
						MessageBox.Show($"Can't save the file: \n{e.Message}");
					}
				},

				(arg) => OpenedFile != null && !RecentlySaved
			);
		}

		public ICommand SaveAsCommand {
			get => new ViewModelCommand(
				(arg) => {
					SFMFile selectedFile = SelectSaveFilePath();
					if (selectedFile == null) return;

					try {
						GridRepresentation.SaveToFile(selectedFile);
						OpenedFile = selectedFile;
						RecentlySaved = true;
					} catch {
						MessageBox.Show($"Can't save the file as:\n {selectedFile.ElementPath}");
					}
				}
			);
		}

		public ICommand UndoCommand {
			get => new ViewModelCommand(
				(arg) => GridRepresentation.MoveToPreviousState(),
				(arg) => GridRepresentation.HasPreviousState()
			);
		}

		public ICommand RedoCommand {
			get => new ViewModelCommand(
				(arg) => GridRepresentation.MoveToNextState(),
				(arg) => GridRepresentation.HasNextState()
			);
		}

		public ICommand ChangeShowExpressionOnlyCommand {
			get => new ViewModelCommand(
				(arg) => ShowExpressionOnly = !ShowExpressionOnly
			);
		}

		public ICommand ResizeCommand {
			get => new ViewModelCommand(
				(arg) => {
					GetNewSize(out int width, out int height);

					bool widthChanged = GridRepresentation.Width != width;
					bool heightChanged = GridRepresentation.Height != height;

					if (widthChanged && heightChanged) {
						GridRepresentation.Dimentions = (width, height);
					} else if (widthChanged) {
						GridRepresentation.Width = width;
					} else if (heightChanged) {
						GridRepresentation.Height = height;
					}
				}
			);
		}

		public ICommand AddColumnCommand {
			get => new ViewModelCommand(
				(arg) => {
					if (!(arg is int index)) return;
					GridRepresentation.AddColumn(index);
				},
				(arg) => GridRepresentation.Width < HistoryCellGrid.MAX_GRID_WIDTH
			);
		}

		public ICommand RemoveColumnCommand {
			get => new ViewModelCommand(
				(arg) => {
					if (!(arg is int index)) return;
					GridRepresentation.RemoveColumn(index);
				},
				(arg) => 1 < GridRepresentation.Width
			);
		}

		public ICommand AddRowCommand {
			get => new ViewModelCommand(
				(arg) => {
					if (!(arg is int index)) return;
					GridRepresentation.AddRow(index);
				},
				(arg) => GridRepresentation.Height < HistoryCellGrid.MAX_GRID_HEIGHT
			);
		}

		public ICommand RemoveRowCommand {
			get => new ViewModelCommand(
				(arg) => {
					if (!(arg is int index)) return;
					GridRepresentation.RemoveRow(index);
				},
				(arg) => 1 < GridRepresentation.Width
			);
		}
		#endregion
	}
}
