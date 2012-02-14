using System;
using System.ComponentModel;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public class AgentStateViewAdapter : INotifyPropertyChanged
    {
        private readonly IRtaStateHolder _rtaStateHolder;
        private readonly IRtaStateGroup _stateGroup;
        private int _totalPersons;

        public AgentStateViewAdapter(IRtaStateHolder rtaStateHolder, IRtaStateGroup stateGroup)
        {
            _rtaStateHolder = rtaStateHolder;
            _stateGroup = stateGroup;
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
            if (handler!=null)
            {
            	handler(this,new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Refresh(DateTime timestamp)
        {
            int totalPersons = 0;
            foreach (var keyValuePair in _rtaStateHolder.AgentStates)
            {
                IAgentState agentState = keyValuePair.Value;
                IVisualLayer currentState = agentState.FindCurrentState(timestamp);
                if (currentState==null) continue;
                if (currentState.Payload == _stateGroup)
                {
                    totalPersons++;
                }
            }
            TotalPersons = totalPersons;
        }
    }
}