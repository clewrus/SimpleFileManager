using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.GridRepresentation {
	public class CalculatingGrid : CellGrid {
		public CalculatingGrid (int width, int height) : base(width, height) {
			spreadErrorListeners = new Dictionary<GridCoordinates, HashSet<ParsedCell>>();
			cellsInCycle = new HashSet<ParsedCell>();
			cellCoords = new Dictionary<ParsedCell, GridCoordinates>();
			cellGraph = new Dictionary<ParsedCell, (HashSet<ParsedCell> children, HashSet<ParsedCell> parents)>();

			CellChanged += CellChangedHandler;
			GridStructureChanged += StructureChangedHandler;
		}

		// Factory Method
		protected override Cell CreateCell (GridCoordinates gridCoordinates) {
			return new ParsedCell(gridCoordinates);
		}

		private void StructureChangedHandler (object sender, EventArgs e) {
			GlobalRefresh();
		}

		private void GlobalRefresh () {
			ReleafeCalculatingErrors();
			ReEvaluateCellGraph();

			cellsInCycle.Clear();
			FindGraphCycles(out List<Stack<ParsedCell>> cycleNodes);

			foreach (var path in cycleNodes) {
				foreach (var cycledCell in path) {
					cycledCell.ErrorMessage = GenerateCycleErrorMessage(path, cycledCell);
					cellsInCycle.Add(cycledCell);
				}
			}

			spreadErrorListeners.Clear();
			foreach (var node in cellGraph.Keys) {
				if (!node.Calculable) {
					SpreadErrorFromCell(node, node.Coordinates);
				}
			}

			CalculateGrid();

			spreadErrorListeners.Clear();
			foreach (var node in cellGraph.Keys) {
				if (!node.Calculable) {
					SpreadErrorFromCell(node, node.Coordinates);
				}
			}
		}

		private void CellChangedHandler (object sender, UpdatedByUserEventArgs e) {
			var targetCell = e.UpdatedCell as ParsedCell;
			if (targetCell == null) return;

			if (cellsInCycle.Contains(targetCell) || targetCell.HasSpreadError) {
				GlobalRefresh();
				return;
			}

			ClearCellsSpreadErrorListeners(targetCell.Coordinates);

			UpdateGraphForCell(targetCell);
			if (!cellGraph.ContainsKey(targetCell)) {
				targetCell.Value = null;
			}

			GlobalRefresh();
			return;

			//var path = new Stack<ParsedCell>();
			//if (HasCycleWithNode(path, new HashSet<ParsedCell>(), targetCell, targetCell)) {
			//	GlobalRefresh();
			//}
			
			//if (!targetCell.Calculable) {
			//	Debug.Assert(cellCoords.ContainsKey(targetCell));
			//	SpreadErrorFromCell(targetCell, cellCoords[targetCell]);
			//} else {
			//	ListenToChildsErrors(targetCell);
			//}
		}

		#region Calculating
		private void CalculateGrid () {
			var childDictionary = new Dictionary<GridCoordinates, object>();
			var unvisited = new HashSet<ParsedCell>(cellGraph.Keys.Where((cell) => cell.Calculable));

			while (unvisited.Count > 0) {
				ParsedCell targetCell = null;
				foreach (var cell in unvisited) { targetCell = cell; break; }

				TryCalculateCell(unvisited, childDictionary, targetCell);
			}
		}

		private bool TryCalculateCell (HashSet<ParsedCell> unvisited, Dictionary<GridCoordinates, object> calculated, ParsedCell curCell) {
			unvisited.Remove(curCell);

			bool success = true;
			foreach (GridCoordinates childCoords in curCell.ChildCells) {
				if (calculated.ContainsKey(childCoords)) continue;
				var childCell = FromCoords(childCoords);

				if (childCell == null) {
					calculated.Add(childCoords, (Decimal)0);
				}

				if (unvisited.Contains(childCell)) {
					success &= TryCalculateCell(unvisited, calculated, childCell);
				}
			}

			if (success) {
				var result = curCell.Calculate(calculated);
				calculated.Add(curCell.Coordinates, curCell.Value);
				return result;
			}

			return false;
		}

		private ParsedCell FromCoords (GridCoordinates coords) {
			(int x, int y) = coords.GetNumericCoords();
			if (Height <= y || Width <= x || x < 0 || y < 0) {
				return null;
			}

			var resultCell = Cells[y][x] as ParsedCell;
			if (!cellCoords.ContainsKey(resultCell)) {
				cellCoords.Add(resultCell, coords);
			}

			return resultCell;
		}

		#endregion

		#region Error Spreading
		private void ReleafeCalculatingErrors () {
			foreach (var cell in cellGraph.Keys) {
				cell.ErrorMessage = cell.ParseErrorMessage;
				cell.HasSpreadError = false;
			}
		}

		private void ClearCellsSpreadErrorListeners (GridCoordinates cords) {
			if (!spreadErrorListeners.ContainsKey(cords)) return;

			foreach (var listener in spreadErrorListeners[cords]) {
				listener.RemoveSpreadErrorSource(cords);
			}
			spreadErrorListeners.Remove(cords);
		}

		private void SpreadErrorFromCell (ParsedCell cell, GridCoordinates cords) {
			if (!spreadErrorListeners.ContainsKey(cords)) {
				spreadErrorListeners.Add(cords, new HashSet<ParsedCell>());
			}

			SpreadToParents(new HashSet<ParsedCell> { cell }, cellGraph[cell].parents, cords);
		}

		private void SpreadToParents (HashSet<ParsedCell> visited, HashSet<ParsedCell> parents, GridCoordinates initCoords) {
			foreach (var parent in parents) {
				if (visited.Contains(parent)) continue;
				visited.Add(parent);

				parent.AddSpreadErrorSource(initCoords);
				spreadErrorListeners[initCoords].Add(parent);

				Debug.Assert(cellGraph.ContainsKey(parent));
				SpreadToParents(visited, cellGraph[parent].parents, initCoords);
			}
		}

		private void ListenToChildsErrors (ParsedCell listener) {
			Debug.Assert(cellGraph.ContainsKey(listener));
			SubscribeToChildError(new HashSet<ParsedCell>() { listener }, cellGraph[listener].children, listener);
		}

		private void SubscribeToChildError (HashSet<ParsedCell> visited, HashSet<ParsedCell> children, ParsedCell initListener) {
			foreach (var child in children) {
				if (visited.Contains(child)) continue;
				visited.Add(child);

				if (child.HasError) {
					var childCords = child.Coordinates;
					if (!spreadErrorListeners.ContainsKey(childCords)) {
						spreadErrorListeners.Add(childCords, new HashSet<ParsedCell>());
					}

					initListener.AddSpreadErrorSource(childCords);
					spreadErrorListeners[childCords].Add(initListener);
				} else {
					Debug.Assert(cellGraph.ContainsKey(child));
					SubscribeToChildError(visited, cellGraph[child].children, initListener);
				}
			}
		}
		#endregion

		#region Cycle detection
		private void FindGraphCycles (out List<Stack<ParsedCell>> cycles) {
			cycles = new List<Stack<ParsedCell>>();

			var unvisited = new HashSet<ParsedCell>(cellGraph.Keys);
			while (unvisited.Count > 0) {

				ParsedCell curNode = null;
				foreach (var node in unvisited) { curNode = node; break; }

				var path = new Stack<ParsedCell>();
				if (HasCycleWithNode(path, new HashSet<ParsedCell>(), curNode, curNode)) {
					cycles.Add(path);
					foreach (var node in path) {
						unvisited.Remove(node);
					}
				} else {
					unvisited.Remove(curNode);
				}
			}
		}

		private bool HasCycleWithNode (Stack<ParsedCell> path, HashSet<ParsedCell> visited, ParsedCell currCell, ParsedCell initCell) {
			if (visited.Contains(currCell)) return false;
			visited.Add(currCell);
			path.Push(currCell);

			foreach (var child in cellGraph[currCell].children) {
				if (child == initCell) {
					return true;
				}

				if (visited.Contains(child)) continue;

				if (HasCycleWithNode(path, visited, child, initCell)) {
					return true;
				}
			}

			path.Pop();
			return false;
		}

		private string GenerateCycleErrorMessage (Stack<ParsedCell> path, ParsedCell initCell) {
			var sb = new StringBuilder();
			sb.Append("Recursive dependency: \n");

			var pathList = new List<ParsedCell>(path);
			var shift = pathList.IndexOf(initCell);
			Debug.Assert(shift >= 0);

			for (int i = 0; i < pathList.Count; i++) {
				var cell = pathList[(shift + pathList.Count - i) % pathList.Count];
				(string x, string y) = cell.Coordinates.GetStringCoords();
				sb.Append($"{x}{y} ");
			}
			return sb.ToString();
		}
		#endregion

		#region Graph changing
		private void ReEvaluateCellGraph () {
			cellCoords.Clear();
			cellGraph.Clear();
			var nonEmptyCells = FindNonEmptyCells();

			foreach (var cell in nonEmptyCells) {
				var childrenSet = new HashSet<ParsedCell>();
				cellGraph.Add(cell, (childrenSet, new HashSet<ParsedCell>()));

				childrenSet.UnionWith(ExtractCellsFromCoords(cell.ChildCells));
			}

			AddChildAsNodes();

			foreach (var pair in cellGraph) {
				AddParentToChildren(pair.Key, pair.Value.children);
			}
		}

		private void AddChildAsNodes () {
			var allChildren = new HashSet<ParsedCell>();
			foreach (var value in cellGraph.Values) {
				allChildren.UnionWith(value.children);
			}

			foreach (var child in allChildren) {
				if (!cellGraph.ContainsKey(child)) {
					cellGraph.Add(child, (new HashSet<ParsedCell>(), new HashSet<ParsedCell>()));
				}
			}
		}

		private HashSet<ParsedCell> FindNonEmptyCells () {
			var nonEmpty = new HashSet<ParsedCell>();
			foreach (var row in Cells) {
				foreach (var cell in row) {
					if (cell is ParsedCell parsedCell && !parsedCell.IsEmpty()) {
						nonEmpty.Add(parsedCell);
					}
				}
			}
			return nonEmpty;
		}

		private void AddParentToChildren (ParsedCell parent, HashSet<ParsedCell> children) {
			foreach (var child in children) {
				if (!cellGraph.ContainsKey(child)) {
					cellGraph.Add(child, (new HashSet<ParsedCell>(), new HashSet<ParsedCell>()));
				}
				cellGraph[child].parents.Add(parent);
			}
		}

		private void UpdateGraphForCell (ParsedCell targetCell) {
			if (!cellGraph.ContainsKey(targetCell)) {
				cellGraph.Add(targetCell, (new HashSet<ParsedCell>(), new HashSet<ParsedCell>()));
			} else {
				RemoveParentFromChildren(targetCell);
				cellGraph[targetCell].children.Clear();
			}

			cellGraph[targetCell].children.UnionWith(ExtractCellsFromCoords(targetCell.ChildCells));
			AddParentToChildren(targetCell, cellGraph[targetCell].children);

			if (cellGraph[targetCell].children.Count == 0 && cellGraph[targetCell].parents.Count == 0) {
				cellGraph.Remove(targetCell);
			}
		}

		private void RemoveParentFromChildren (ParsedCell parent) {
			if (!cellGraph.ContainsKey(parent)) return;
			var children = cellGraph[parent].children;

			foreach (var child in children) {
				if (!cellGraph.ContainsKey(child)) continue;

				var childNode = cellGraph[child];
				if (!childNode.parents.Contains(parent)) continue;

				childNode.parents.Remove(parent);
				if (childNode.parents.Count == 0 && childNode.children.Count == 0) {
					cellGraph.Remove(child);
				}
			}
		}

		private HashSet<ParsedCell> ExtractCellsFromCoords (HashSet<GridCoordinates> coordsSet) {
			var cells = new HashSet<ParsedCell>();
			foreach (var coords in coordsSet) {
				(int x, int y) = coords.GetNumericCoords();

				if (Width <= x || Height <= y || x < 0 || y < 0) continue;
				if (Cells[y][x] is ParsedCell parsedChild) {
					cells.Add(parsedChild);

					if (!cellCoords.ContainsKey(parsedChild)) {
						cellCoords.Add(parsedChild, coords);
					}
				}
			}

			return cells;
		}
		#endregion

		private HashSet<ParsedCell> cellsInCycle;
		private Dictionary<GridCoordinates, HashSet<ParsedCell>> spreadErrorListeners;
		private Dictionary<ParsedCell, GridCoordinates> cellCoords;
		private Dictionary<ParsedCell, (HashSet<ParsedCell> children, HashSet<ParsedCell> parents)> cellGraph;
	}
}
