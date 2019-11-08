using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFM.GridEditor.GridRepresentation {
	public class SequenceCaretaker {
		public SequenceCaretaker () {
			mementos = new LinkedList<IMemento>();
		}

		public IMemento CurrentMemento {
			get => (currentNode == null) ? null : currentNode.Value;
		}

		public bool HasNextState () {
			return currentNode != null && currentNode.Next != null;
		}

		public bool HasPreviousState () {
			return currentNode != null && currentNode.Previous != null;
		}

		public void SetNextMemento (IMemento nwMemento) {
			if (currentNode == null) {
				Debug.Assert(mementos.Count == 0);

				mementos.AddLast(nwMemento);
				currentNode = mementos.Last;
			} else {
				while (mementos.Last != currentNode) {
					mementos.RemoveLast();
				}
				mementos.AddLast(nwMemento);
			}
		}

		public bool MoveToNext () {
			if (currentNode == null || currentNode.Next == null) {
				return false;
			}
			currentNode = currentNode.Next;
			return true;
		}

		public bool MoveToPrev () {
			if (currentNode == null || currentNode.Previous == null) {
				return false;
			}
			currentNode = currentNode.Previous;
			return true;
		}

		private LinkedListNode<IMemento> currentNode;
		private LinkedList<IMemento> mementos;
	}
}
