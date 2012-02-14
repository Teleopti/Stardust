using System;
using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.UndoRedo.ViewModels
{
    public class MementoViewModelCollection : ObservableCollection<MementoViewModel>
    {
        public DateTime Earliest { get; private set; }

        protected override void ClearItems()
        {
            Earliest = DateTime.UtcNow;
            base.ClearItems();
        }
        public void AddMemento(IMementoInformation memento)
        {
            if (memento.Time < Earliest) Earliest = memento.Time;
            Add(new MementoViewModel(memento));
        }

        public MementoViewModelCollection()
        {
            Earliest = DateTime.UtcNow;
        }
    }
}