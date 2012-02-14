using System;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.WinCode.Common.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.UndoRedo.ViewModels
{
    public class MementoViewModel :DataModel
    {
        public ObservableCollection<MementoViewModel> Children { get; private set;} 
        private DateTime _dateTime;
        private string _description;
        private bool _isUndoing;


        public DateTime DateTime
        {
            get { return _dateTime; }
            set
            {
                if (value!=_dateTime)
                {
                    _dateTime = value;
                    SendPropertyChanged("DateTime");
                }
            }
        }
        

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value )
                {
                    _description = value;
                    SendPropertyChanged("Description");
                }
            }
        }

        public bool IsUndoing
        {
            get { return _isUndoing; }
            set
            { 
                _isUndoing = value;
                SendPropertyChanged("IsUndoing");
                if (Children != null)
                {
                    foreach (MementoViewModel m in Children)
                    {
                        m.IsUndoing = value;
                    }
                }
            }
        }

        public MementoViewModel(IMementoInformation memento)
        {
            Description = memento.Description;
            BatchMemento batchMemento = memento as BatchMemento;
            if (batchMemento != null) 
                initChildren(batchMemento); 
            DateTime = memento.Time;
        }

        private void initChildren(BatchMemento memento)
        {
            Children = new ObservableCollection<MementoViewModel>();
            foreach (IMemento m in memento.MementoCollection)
            {
                MementoViewModel mem = new MementoViewModel(m);
                mem.Description =  m.Description;
                Children.Add(mem);
            }
        }

        
    }
}