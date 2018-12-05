using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday
{
    public class AgentStateViewAdapter : INotifyPropertyChanged
    {
		private readonly IDayLayerViewModel _dayLayerViewModel;
	    private readonly IEnumerable<IRtaStateGroup> _rtaStateGroups;
	    private readonly IRtaStateGroup _stateGroup;
        private int _totalPersons;

        public AgentStateViewAdapter(IRtaStateGroup stateGroup, IDayLayerViewModel dayLayerViewModel)
        {
            _stateGroup = stateGroup;
            //TODO: Not sure if this is appropriate?
            _dayLayerViewModel = dayLayerViewModel;
        }

		public AgentStateViewAdapter(IRtaStateGroup stateGroup, IDayLayerViewModel dayLayerViewModel, IEnumerable<IRtaStateGroup> rtaStateGroups)
		{
			_stateGroup = stateGroup;
			_dayLayerViewModel = dayLayerViewModel;
			_rtaStateGroups = rtaStateGroups;
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
	            if (_totalPersons == value) return;
	            _totalPersons = value;
	            NotifyPropertyChanged(nameof(TotalPersons));
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

	    public void Refresh()
	    {
		    TotalPersons = _dayLayerViewModel.Models.Count(d => d.CurrentStateDescription == _stateGroup.Name);
		    if (_stateGroup.Name == "Not defined")
			    TotalPersons += _dayLayerViewModel.Models.Count(d => d.CurrentStateDescription == null);
		    if (_stateGroup.Name == "OLAANDASADSSECRETNAME")
		    {
				var possibleLoggedInStates = _rtaStateGroups.Where(x => !x.IsLogOutState);
				foreach (var item in possibleLoggedInStates)
				{
					TotalPersons += _dayLayerViewModel.Models.Count(d => d.CurrentStateDescription != null && d.CurrentStateDescription == item.Name);
				}
			}

		    if (_stateGroup.Name == "OLAANDASADSSECRETNAMETWO")
		    {
			    var possibleLoggedOutStates = _rtaStateGroups.Where(x => x.IsLogOutState);
			    TotalPersons = _dayLayerViewModel.Models.Count(d => d.CurrentStateDescription == null);
				foreach (var item in possibleLoggedOutStates)
				{
					TotalPersons += _dayLayerViewModel.Models.Count(d => d.CurrentStateDescription != null && d.CurrentStateDescription == item.Name);
				}

			}

	    }

	    public string Group
	    {
		    get
		    {
			    if (StateGroup.Name.Equals("OLAANDASADSSECRETNAME"))
				    return "Logged on agents";
				if (StateGroup.Name.Equals("OLAANDASADSSECRETNAMETWO"))
					return "Logged off agents";

				return StateGroup.Available ? "Available" : "Not available";
			}
	    }
		public int Sort
		{
			get
			{
				if (StateGroup.Name.Equals("OLAANDASADSSECRETNAME"))
					return 0;
				if (StateGroup.Name.Equals("OLAANDASADSSECRETNAMETWO"))
					return 3;

				return StateGroup.Available ? 1 : 2;
			}
		}

		public string Name
		{
			get
			{
				if (StateGroup.Name.Equals("OLAANDASADSSECRETNAME"))
					return "Logged on agents";
				if (StateGroup.Name.Equals("OLAANDASADSSECRETNAMETWO"))
					return "Logged off agents";

				return StateGroup.Name;
			}
		}
	}
}