using SimpleFM.FileManager.ModelCovers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.GridRepresentation {
	public abstract class HistoryCellGrid {
		public HistoryCellGrid (int width, int height) {
			Grid = CreateGrid(width, height);
			EndHistoryGridInitialization();
		}

		public HistoryCellGrid (SFMFile targetGridFile) {
			var formatter = new BinaryFormatter();
			var readFileStream = new FileStream(targetGridFile.ElementPath, FileMode.Open, FileAccess.Read);

			IMemento deserealizedMemento = formatter.Deserialize(readFileStream) as IMemento;
			if (deserealizedMemento == null) {
				throw new FileFormatException($"Can't get GridInfo from file: {targetGridFile.ElementPath}");
			}

			Grid = CreateGrid(1, 1);
			Grid.SetState(deserealizedMemento);

			EndHistoryGridInitialization();
		}

		private void EndHistoryGridInitialization () {
			gridCaretaker = new SequenceCaretaker();
			SaveGridState();

			isListeningGridUpdates = true;
			Grid.UpdatedByUser += GridUpdatedByUserHandler;
		}

		protected abstract CellGrid CreateGrid (int width, int height);

		public void SaveToFile (SFMFile targetFile) {
			var currentMemento = Grid.GenerateMemento();
			var writeFileStream = new FileStream(targetFile.ElementPath, FileMode.Create, FileAccess.Write);

			var formatter = new BinaryFormatter();
			formatter.Serialize(writeFileStream, currentMemento);
		}

		private void GridUpdatedByUserHandler (object sender, UpdatedByUserEventArgs e) {
			if (isListeningGridUpdates) {
				SaveGridState();
			}

			OnGridChanged();
		}

		private void OnGridChanged () {
			GridChanged?.Invoke(this, new EventArgs());
		}

		#region Memento handling
		private void SaveGridState () {
			var curMemento = Grid.GenerateMemento();
			gridCaretaker.SetNextMemento(curMemento);
			gridCaretaker.MoveToNext();
		}

		public bool HasPreviousState () {
			return gridCaretaker.HasPreviousState();
		}

		public bool HasNextState () {
			return gridCaretaker.HasNextState();
		}

		public bool MoveToPreviousState () {
			if (!gridCaretaker.MoveToPrev()) {
				return false;
			}

			isListeningGridUpdates = false;
			Grid.SetState(gridCaretaker.CurrentMemento);
			isListeningGridUpdates = true;
			return true;
		}

		public bool MoveToNextState () {
			if (!gridCaretaker.MoveToNext()) {
				return false;
			}

			isListeningGridUpdates = false;
			Grid.SetState(gridCaretaker.CurrentMemento);
			isListeningGridUpdates = true;
			return true;
		}
		#endregion

		public ObservableCollection<ObservableCollection<Cell>> GridData {
			get => Grid.Cells;
		}

		public (int, int) Dimentions {
			get => Grid.Dimentions;
			set {
				int validWidth = Math.Max(1, Math.Min(value.Item1, MAX_GRID_WIDTH));
				int validHeight = Math.Max(1, Math.Min(value.Item2, MAX_GRID_HEIGHT));
				Grid.Dimentions = (validWidth, validHeight);
			}
		}

		public int Width {
			get => Grid.Width;
			set => Grid.Width = Math.Max(1, Math.Min(value, MAX_GRID_WIDTH));
		}

		public int Height {
			get => Grid.Height;
			set => Grid.Height = Math.Max(1, Math.Min(value, MAX_GRID_HEIGHT));
		}

		public static readonly int MAX_GRID_WIDTH = 60;
		public static readonly int MAX_GRID_HEIGHT = 60;

		public CellGrid Grid {get; private set;}
		public event EventHandler GridChanged;

		private bool isListeningGridUpdates;
		private SequenceCaretaker gridCaretaker;
	}
}
