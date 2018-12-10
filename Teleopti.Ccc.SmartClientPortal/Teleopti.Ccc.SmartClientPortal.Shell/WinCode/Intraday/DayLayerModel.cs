using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Converters;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday
{
    public class DayLayerModel : DependencyObject, INotifyPropertyChanged, IEditableObject
    {
        private readonly IPerson _person;
        private readonly DateTimePeriod _period;
        private readonly ITeam _team;
        private readonly LayerViewModelCollection _layerViewModelCollection;
        private readonly ICommonNameDescriptionSetting _commonNameDescriptionSetting;
	    private readonly ElapsedTimeConverter _elapsedTimeConverter = new ElapsedTimeConverter();

        private DateTime _scheduleStartDateTime = DateTime.MaxValue;
        private bool _isPinned;
        private string _currentStateDescription;
        private DateTime _enteredCurrentState;
        private string _currentActivityDescription;
        private string _alarmDescription;
        private string _nextActivityDescription;
        private DateTime _nextActivityStartDateTime;
        private int _colorValue = 16777215;
        private double _staffingEffect;
        private DateTime _ruleStartTime;
		private bool _isInEditMode;

		public const int NoAlarmColorValue = 16777215;

        public DayLayerModel(IPerson person, DateTimePeriod period, ITeam team,
                             LayerViewModelCollection layerViewModelCollection,
                             ICommonNameDescriptionSetting commonNameDescriptionSetting)
        {
            _person = person;
            _layerViewModelCollection = layerViewModelCollection;
            _commonNameDescriptionSetting = commonNameDescriptionSetting;
            _team = team;
            _period = period;
			ColorValue = NoAlarmColorValue;
        }

	    public ITeam Team => _team;

		public IPerson Person => _person;

		public string CommonNameDescription => _commonNameDescriptionSetting.BuildFor(_person);

		public bool IsPinned
        {
            get { return _isPinned; }
            set
            {
	            if (_isPinned == value) return;
	            _isPinned = value;
	            notifyPropertyChanged(nameof(IsPinned));
            }
        }

        public LayerViewModelCollection Layers => _layerViewModelCollection;

		public int ColorValue
        {
            get { return _colorValue; }
            set
            {
                if (_colorValue == value) return;
                _colorValue = value;
                notifyPropertyChanged(nameof(Color));
            }
        }

        public Color Color
        {
			get
			{
				if (HasAlarm)
				{
					return Color.FromArgb(_colorValue);										
				}
				return Color.FromArgb(NoAlarmColorValue);
			}
        }

        public string AlarmDescription
        {
            get { return _alarmDescription; }
            set
            {
                if (_alarmDescription == value) return;
                _alarmDescription = value;
                notifyPropertyChanged(nameof(AlarmDescription));
            }
        }
        public DateTime RuleStartTime
        {
            get { return _ruleStartTime; }
            set { _ruleStartTime = value; }
        }

        public string NextActivityDescription
        {
            get { return _nextActivityDescription; }
            set
            {
                if (_nextActivityDescription == value) return;
                _nextActivityDescription = value;
                notifyPropertyChanged(nameof(NextActivityDescription));
            }
        }

        public DateTime NextActivityStartDateTime
        {
            get { return _nextActivityStartDateTime; }
            set
            {
                if (_nextActivityStartDateTime == value) return;
                _nextActivityStartDateTime = value;
                notifyPropertyChanged(nameof(NextActivityStartDateTime));
            }
        }

        public DateTime ScheduleStartDateTime
        {
            get { return _scheduleStartDateTime; }
            set
            {
                if (_scheduleStartDateTime == value) return;
                _scheduleStartDateTime = value;
                notifyPropertyChanged(nameof(ScheduleStartDateTime));
            }
        }


        public string CurrentActivityDescription
        {
            get { return _currentActivityDescription; }
            set
            {
                if (_currentActivityDescription == value) return;
                _currentActivityDescription = value;
                notifyPropertyChanged(nameof(CurrentActivityDescription));
            }
        }

        public string CurrentStateDescription
        {
            get { return _currentStateDescription; }
            set
            {
                if (_currentStateDescription == value) return;
                _currentStateDescription = value;
                notifyPropertyChanged(nameof(CurrentStateDescription));
            }
        }

        public DateTime EnteredCurrentState
        {
            get { return _enteredCurrentState; }
            set
            {
                _enteredCurrentState = value;
                notifyPropertyChanged(nameof(EnteredCurrentState));
				notifyPropertyChanged(nameof(TimeInCurrentState));
            }
        }

        public double StaffingEffect
        {
            get { return _staffingEffect; }
            set
            {
                if (_staffingEffect.Equals(value)) return;
                _staffingEffect = value;
                notifyPropertyChanged(nameof(StaffingEffect));
            }
        }
		
		public bool ShowNextActivity
        {
            get { return (bool)GetValue(ShowNextActivityProperty); }
            set { SetValue(ShowNextActivityProperty, value); }
        }

        public DateTimePeriod Period => _period;

		public bool EditLayer
	    {
		    get { return _editLayer; }
		    set
		    {
			    if (_editLayer == value) return;
			    _editLayer = value;
				notifyPropertyChanged(nameof(EditLayer));
		    }
	    }

	    public static readonly DependencyProperty ShowNextActivityProperty =
            DependencyProperty.Register("ShowNextActivity", typeof(bool), typeof(DayLayerModel),
                                        new UIPropertyMetadata(false));

	    private bool _editLayer;
	    private bool _hasAlarm;

	    public int HookedEvents()
        {
            var handler = PropertyChanged;
            if (handler == null)
            {
                return 0;
            }
            int i = handler.GetInvocationList().Count();
            return i;
        }

        private void notifyPropertyChanged(string propertyName)
        {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

		public bool IsInEditMode
		{
			get { return _isInEditMode; }
			set
			{
				if (_isInEditMode == value) return;
				_isInEditMode = value;
				notifyPropertyChanged(nameof(IsInEditMode));
			}
		}

		public void BeginEdit()
		{
			IsInEditMode = true;
		}

		public void EndEdit()
		{
			IsInEditMode = false;
		}

		public void CancelEdit()
		{
			IsInEditMode = false;
		}

	    public object TimeInCurrentState => _elapsedTimeConverter.Convert(EnteredCurrentState,typeof(TimeSpan),null,null);

		public bool HasAlarm
	    {
		    get { return _hasAlarm; }
		    set
		    {
			    _hasAlarm = value;
				notifyPropertyChanged(nameof(HasAlarm));
		    }
	    }

    }
}