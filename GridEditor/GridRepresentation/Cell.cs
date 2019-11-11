using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.GridRepresentation {
	public class Cell : INotifyPropertyChanged {

		private void OnPropertyChanged ([CallerMemberName] string name="") {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

		private void OnChangedByUser () {
			ChangedByUser?.Invoke(this, new EventArgs());
		}

		private string SpaceIsOnlyWhiteSymbol (string value) {
			if (value == null) {
				return null;
			}

			return new string(value.Where(c => c == ' ' || !Char.IsWhiteSpace(c)).ToArray());
		}

		public bool IsEmpty () {
			return String.IsNullOrEmpty(Value) && String.IsNullOrEmpty(ExpressionStr);
		}

		public bool ExpressionIsFormula () {
			return _ExpressionStr != null && _ExpressionStr.Length > 0 && _ExpressionStr[0] == '=';
		}

		#region Error property
		private bool _HasError;
		public bool HasError {
			get => _HasError;
			set {
				_HasError = value;
				OnPropertyChanged();
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
		private string _Value;
		public string Value {
			get => _Value;
			set {
				if (_Value == value) return;
				_Value = SpaceIsOnlyWhiteSymbol(value);
				OnPropertyChanged();
				OnChangedByUser();
			}
		}

		public string ValueWithinCode {
			set {
				if (_Value == value) return;
				_Value = SpaceIsOnlyWhiteSymbol(value);
				OnPropertyChanged("Value");
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
				_ExpressionStr = SpaceIsOnlyWhiteSymbol(value);

				if (!prevWasFormula && ExpressionIsFormula()) {
					ValueWithinCode = null;
				} else if (!ExpressionIsFormula()) {
					ValueWithinCode = ExpressionStr;
				}

				OnPropertyChanged();
				OnChangedByUser();
			}
		}

		public string ExpressionStrWithinCode {
			set {
				if (_ExpressionStr == value) return;
				_ExpressionStr = SpaceIsOnlyWhiteSymbol(value);
				OnPropertyChanged("ExpressionStr");
			}
		}
		#endregion

		public event EventHandler ChangedByUser;
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
