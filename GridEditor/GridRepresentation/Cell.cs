using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.GridRepresentation {
	public class Cell : INotifyPropertyChanged {

		public Cell () {
			spreadErrorSources = new HashSet<GridCoordinates>();
		}

		private void OnPropertyChanged ([CallerMemberName] string name="") {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		private void OnChangedByUser () {
			PreChangedByUser?.Invoke(this, new EventArgs());
			ChangedByUser?.Invoke(this, new EventArgs());
		}

		private object SpaceIsOnlyWhiteSymbol (object valueObj) {
			var value = valueObj as string;
			if (value == null) {
				return valueObj;
			}

			return new string(value.Where(c => c == ' ' || !Char.IsWhiteSpace(c)).ToArray());
		}

		public bool IsEmpty () {
			return String.IsNullOrEmpty(ExpressionStr);
		}

		public bool ExpressionIsFormula () {
			return _ExpressionStr != null && _ExpressionStr.Length > 0 && _ExpressionStr[0] == '=';
		}

		public bool Calculable {
			get => !HasSpreadError && !HasError;
		}

		#region Spread error
		public void AddSpreadErrorSource (GridCoordinates nwSource) {
			spreadErrorSources.Add(nwSource);

			HasSpreadError = true;

			(string x, string y) = nwSource.GetStringCoords();
			SpreadErrorMessage = $"Error in {x + y}";
		}

		public void RemoveSpreadErrorSource (GridCoordinates oldSource) {
			if (!spreadErrorSources.Contains(oldSource)) return;
			spreadErrorSources.Remove(oldSource);

			if (spreadErrorSources.Count == 0) {
				HasSpreadError = false;
				SpreadErrorMessage = null;
			} else {
				foreach (var src in spreadErrorSources) {
					(string x, string y) = src.GetStringCoords();
					SpreadErrorMessage = $"Error in {x + y}";

					break;
				}
			}
		}

		private bool _HasSpreadError;
		public bool HasSpreadError {
			get => _HasSpreadError;
			set {
				_HasSpreadError = value;
				if (!value) {
					spreadErrorSources.Clear();
				}
				OnPropertyChanged();
				OnPropertyChanged("HasError");
				OnPropertyChanged("Calculable");
			}
		}

		private string _SpreadErrorMessage;
		public string SpreadErrorMessage {
			get => _SpreadErrorMessage;
			set {
				_SpreadErrorMessage = value;
				OnPropertyChanged();
				OnPropertyChanged("ErrorMessage");
			}
		}
		#endregion

		#region Error property
		private bool _HasError;
		public bool HasError {
			get => _HasError;
			set {
				_HasError = value;
				OnPropertyChanged();
				OnPropertyChanged("Calculable");
			}
		}

		private string _ErrorMessage;
		public string ErrorMessage {
			get => _ErrorMessage;
			set {
				_ErrorMessage = value;
				HasError = !string.IsNullOrEmpty(_ErrorMessage);

				OnPropertyChanged();
			}
		}
		#endregion

		#region Value property
		private object _Value;
		public object Value {
			get => _Value;
			set {
				if (_Value == value) return;
				_Value = SpaceIsOnlyWhiteSymbol(value);
				OnPropertyChanged();
			}
		}
		#endregion

		#region ExpressionStr property
		private string _ExpressionStr;
		public string ExpressionStr {
			get => _ExpressionStr;
			set {
				if (_ExpressionStr == value) return;

				bool prevWasFormula = ExpressionIsFormula();
				_ExpressionStr = SpaceIsOnlyWhiteSymbol(value) as string;

				if (!prevWasFormula && ExpressionIsFormula()) {
					Value = null;
				} else if (!ExpressionIsFormula()) {
					Value = ExpressionStr;
				}

				OnPropertyChanged();
				OnChangedByUser();
			}
		}

		public string ExpressionStrWithinCode {
			set {
				if (_ExpressionStr == value) return;
				_ExpressionStr = SpaceIsOnlyWhiteSymbol(value) as string;
				OnPropertyChanged("ExpressionStr");
			}
		}
		#endregion

		protected event EventHandler PreChangedByUser;

		public event EventHandler ChangedByUser;
		public event PropertyChangedEventHandler PropertyChanged;

		private HashSet<GridCoordinates> spreadErrorSources;
	}
}
