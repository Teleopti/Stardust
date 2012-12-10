using System;
using System.Linq;
using System.ComponentModel;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public class AgentStateViewAdapter : INotifyPropertyChanged
    {
        private static IDayLayerViewModel _dayLayerViewModel;
        //private readonly IRtaStateHolder _rtaStateHolder;
        private readonly IRtaStateGroup _stateGroup;
        private int _totalPersons;

        public AgentStateViewAdapter(IRtaStateGroup stateGroup, IDayLayerViewModel dayLayerViewModel)
        {
            //_rtaStateHolder = rtaStateHolder;
            _stateGroup = stateGroup;
            //TODO: Not sure if this is appropriate?
            _dayLayerViewModel = dayLayerViewModel;
        }

        public IRtaStateGroup StateGroup
        {
            get { return _stateGroup; }
        }

        public int TotalPersons
        {
            get { return _totalPersons; }
            private set
            {
                if (_totalPersons != value)
                {
                    _totalPersons = value;
                    NotifyPropertyChanged("TotalPersons");
                }
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Refresh()
        {
            TotalPersons = _dayLayerViewModel.Models.Count(d => d.CurrentStateDescription == _stateGroup.Name);
            if (_stateGroup.Name == "Not defined")
                TotalPersons += _dayLayerViewModel.Models.Count(d => d.CurrentStateDescription == null);
        }
    }
}