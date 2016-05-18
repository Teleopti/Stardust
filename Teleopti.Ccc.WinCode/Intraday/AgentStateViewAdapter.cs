using System.Linq;
using System.ComponentModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public class AgentStateViewAdapter : INotifyPropertyChanged
    {
		private readonly IDayLayerViewModel _dayLayerViewModel;
	    private readonly IRtaStateGroup _stateGroup;
        private int _totalPersons;

        public AgentStateViewAdapter(IRtaStateGroup stateGroup, IDayLayerViewModel dayLayerViewModel)
        {
            _stateGroup = stateGroup;
            //TODO: Not sure if this is appropriate?
            _dayLayerViewModel = dayLayerViewModel;
        }
		
		public IRtaStateGroup StateGroup => _stateGroup;

	    public int TotalPersons
        {
            get { return _totalPersons; }
            private set
            {
	            if (_totalPersons == value) return;
	            _totalPersons = value;
	            NotifyPropertyChanged("TotalPersons");
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
	        handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

	    public void Refresh()
	    {
		    TotalPersons = _dayLayerViewModel.Models.Count(d => d.CurrentStateDescription == _stateGroup.Name);
		    if (_stateGroup.Name == "Not defined")
			    TotalPersons += _dayLayerViewModel.Models.Count(d => d.CurrentStateDescription == null);
	    }

	    public string Group => StateGroup.Available ? "Available" : "Not available";

	    public int Sort => StateGroup.Available ? 1 : 2;

	    public string Name => StateGroup.Name;
    }
}