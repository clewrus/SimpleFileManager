using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.GridRepresentation {
	public abstract class HistoryCellGrid {
		public HistoryCellGrid (int width, int height) {
			Grid = CreateGrid(width, height);
			gridCaretaker = new SequenceCaretaker();

			isListeningGridUpdates = true;
			Grid.UpdatedByUser += GridUpdatedByUserHandler;
		}

		protected abstract CellGrid CreateGrid (int width, int height);

		private void GridUpdatedByUserHandler (object sender, UpdatedByUserEventArgs e) {
			if (isListeningGridUpdates) {
				SaveGridState();
			}
		}

		private void SaveGridState () {
			var curMemento = Grid.GenerateMemento();
			gridCaretaker.SetNextMemento(curMemento);
			gridCaretaker.MoveToNext();
		}

		public ObservableCollection<ObservableCollection<Cell>> GridData {
			get => Grid.Cells;
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

		public int Width {
			get => Grid.Width;
			set => Grid.Width = Math.Max(1, Math.Min(value, MAX_GRID_WIDTH));
		}

		public int Height {
			get => Grid.Height;
			set => Grid.Height = Math.Max(1, Math.Min(value, MAX_GRID_HEIGHT));
		}

		public static readonly int MAX_GRID_WIDTH = 1000;
		public static readonly int MAX_GRID_HEIGHT = 2000;

		public CellGrid Grid {get; private set;}

		private bool isListeningGridUpdates;
		private SequenceCaretaker gridCaretaker;
	}
}
