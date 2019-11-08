﻿using SimpleFM.Common.Commands;
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
			GridRepresentation = new HistoryCalculatingGrid(30, 30);
		}

		private HistoryCellGrid _GridRepresentation;
		public HistoryCellGrid GridRepresentation {
			get => _GridRepresentation;
			set => SetProperty(ref _GridRepresentation, value);
		}

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
	}
}
