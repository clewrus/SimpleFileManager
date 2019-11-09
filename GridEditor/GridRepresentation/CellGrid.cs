using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.GridRepresentation {
	public abstract class CellGrid : INotifyPropertyChanged {

		public CellGrid () : this(1, 1) { }

		public CellGrid (int width, int height) {
			Cells = new ObservableCollection<ObservableCollection<Cell>>();

			this.Dimentions = (width, height);
		}

		private void OnPropertyChanged ([CallerMemberName] string name = "") {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		private void CellChangedByUserHandler (object sender, EventArgs e) {
			var senderCell = sender as Cell;
			if (senderCell == null) return;

			OnUpdatedByUser(senderCell);
		}

		private void OnUpdatedByUser (Cell changedCell) {
			UpdatedByUser?.Invoke(this, new UpdatedByUserEventArgs(changedCell));
		}

		private void OnUpdatedByUser (int width, int height) {
			UpdatedByUser?.Invoke(this, new UpdatedByUserEventArgs(width, height));
		}

		protected abstract Cell CreateCell ();

		private void AddNewCellToList (IList<Cell> targetList) {
			var nwCell = CreateCell();
			nwCell.ChangedByUser += CellChangedByUserHandler;

			targetList.Add(nwCell);
		}

		private void RemoveLastCellFromList (IList<Cell> targetList) {
			var tarCell = targetList[targetList.Count - 1];
			tarCell.ChangedByUser -= CellChangedByUserHandler;

			targetList.RemoveAt(targetList.Count - 1);
		}

		#region Memento implementation
		public void SetState (IMemento memento) {
			var gridMemento = memento as GridMemento;
			if (gridMemento == null) return;

			var info = gridMemento.Data as GridMemento.GridInfo;
			if (info == null) return;

			Dimentions = (info.width, info.height);

			SetCellsContent(info.content);
		}

		public IMemento GenerateMemento () {
			var curGridInfo = new GridMemento.GridInfo() {
				width = Width,
				height = Height,
				content = ExtractFilledCells()
			};

			return new GridMemento(curGridInfo);
		}

		private Dictionary<(int, int), GridMemento.CellInfo> ExtractFilledCells () {
			var cellsDict = new Dictionary<(int, int), GridMemento.CellInfo>();

			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width; x++) {

					Cell curCell = _Cells[y][x];
					if (!curCell.IsEmpty()) {

						var cellInfo = new GridMemento.CellInfo() {
							expressionStr = curCell.ExpressionStr,
							value = curCell.Value
						};

						cellsDict.Add((x, y), cellInfo);
					}
				}
			}

			return cellsDict;
		}

		private void SetCellsContent (Dictionary<(int, int), GridMemento.CellInfo> nwInfo) {
			int initWidth = Width;
			int initHeight = Height;

			for (int y = 0; y < initHeight; y++) {
				for (int x = 0; x < initWidth; x++) {

					Cell targetCell = Cells[y][x];
					if (nwInfo.ContainsKey((x, y))) {
						GridMemento.CellInfo cellInfo = nwInfo[(x, y)];

						targetCell.ExpressionStr = cellInfo.expressionStr;
						targetCell.Value = cellInfo.value;
					} else {
						targetCell.ExpressionStr = null;
						targetCell.Value = null;
					}
				}
			}
		}

		#endregion

		#region CellsResizing
		private void ChangeCellsHeight (int nwHeight) {
			int initH = Height;
			Debug.Assert(Cells.Count == initH);

			for (int i = 0; i < nwHeight - initH; i++) {
				var nwRow = new ObservableCollection<Cell>();
				for (int j = 0; j < Width; j++) {
					AddNewCellToList(nwRow);
				}

				Cells.Add(nwRow);
			}

			for (int i = 0; i < initH - nwHeight; i++) {
				var targetRow = Cells[Cells.Count - 1];
				while (targetRow.Count > 0) {
					RemoveLastCellFromList(targetRow);
				}

				Cells.RemoveAt(Cells.Count - 1);
			}
		}

		private void ChangeCellsWidth (int nwWidth) {
			int initW = Width;
			foreach (var row in Cells) {
				Debug.Assert(row.Count == initW);

				for (int i = 0; i < nwWidth - initW; i++) {
					AddNewCellToList(row);
				}

				for (int i = 0; i < initW - nwWidth; i++) {
					RemoveLastCellFromList(row);
				}
			}
		}

		#endregion

		public (int, int) Dimentions {
			get => (Width, Height);
			set {
				ChangeCellsWidth(value.Item1);
				_Width = value.Item1;
				OnPropertyChanged("Width");

				ChangeCellsHeight(value.Item2);
				_Height = value.Item2;
				OnPropertyChanged("Height");
				
				OnUpdatedByUser(Width, Height);
			}
		}

		private int _Height;
		public int Height {
			get => _Height;
			set {
				ChangeCellsHeight(value);
				_Height = value;
				OnPropertyChanged();
				OnUpdatedByUser(Width, Height);
			}
		}

		private int _Width;
		public int Width {
			get => _Width;
			set {
				ChangeCellsWidth(value);
				_Width = value;
				OnPropertyChanged();
				OnUpdatedByUser(Width, Height);
			}
		}

		private ObservableCollection<ObservableCollection<Cell>> _Cells;
		public ObservableCollection<ObservableCollection<Cell>> Cells {
			get => _Cells;
			set {
				_Cells = value;
				OnPropertyChanged();
			}
		}

		public event EventHandler<UpdatedByUserEventArgs> UpdatedByUser;
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
