using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Converters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Intraday
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
        private DateTime _adherenceStartTime;
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

	    public ITeam Team
	    {
		    get { return _team; }
	    }

	    public IPerson Person
	    {
		    get { return _person; }
	    }

	    public string CommonNameDescription
        {
            get { return _commonNameDescriptionSetting.BuildCommonNameDescription(_person); }
        }

        public bool IsPinned
        {
            get { return _isPinned; }
            set
            {
	            if (_isPinned == value) return;
	            _isPinned = value;
	            notifyPropertyChanged("IsPinned");
            }
        }

        public LayerViewModelCollection Layers
        {
            get { return _layerViewModelCollection; }
        }

        public int ColorValue
        {
            get { return _colorValue; }
            set
            {
                if (_colorValue == value) return;
                _colorValue = value;
                notifyPropertyChanged("Color");
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
                notifyPropertyChanged("AlarmDescription");
            }
        }
        public DateTime AdherenceStartTime
        {
            get { return _adherenceStartTime; }
            set { _adherenceStartTime = value; }
        }

        public string NextActivityDescription
        {
            get { return _nextActivityDescription; }
            set
            {
                if (_nextActivityDescription == value) return;
                _nextActivityDescription = value;
                notifyPropertyChanged("NextActivityDescription");
            }
        }

        public DateTime NextActivityStartDateTime
        {
            get { return _nextActivityStartDateTime; }
            set
            {
                if (_nextActivityStartDateTime == value) return;
                _nextActivityStartDateTime = value;
                notifyPropertyChanged("NextActivityStartDateTime");
            }
        }

        public DateTime ScheduleStartDateTime
        {
            get { return _scheduleStartDateTime; }
            set
            {
                if (_scheduleStartDateTime == value) return;
                _scheduleStartDateTime = value;
                notifyPropertyChanged("ScheduleStartDateTime");
            }
        }


        public string CurrentActivityDescription
        {
            get { return _currentActivityDescription; }
            set
            {
                if (_currentActivityDescription == value) return;
                _currentActivityDescription = value;
                notifyPropertyChanged("CurrentActivityDescription");
            }
        }

        public string CurrentStateDescription
        {
            get { return _currentStateDescription; }
            set
            {
                if (_currentStateDescription == value) return;
                _currentStateDescription = value;
                notifyPropertyChanged("CurrentStateDescription");
            }
        }

        public DateTime EnteredCurrentState
        {
            get { return _enteredCurrentState; }
            set
            {
                _enteredCurrentState = value;
                notifyPropertyChanged("EnteredCurrentState");
				notifyPropertyChanged("TimeInCurrentState");
            }
        }

        public double StaffingEffect
        {
            get { return _staffingEffect; }
            set
            {
                if (_staffingEffect.Equals(value)) return;
                _staffingEffect = value;
                notifyPropertyChanged("StaffingEffect");
            }
        }
		
		public bool ShowNextActivity
        {
            get { return (bool)GetValue(ShowNextActivityProperty); }
            set { SetValue(ShowNextActivityProperty, value); }
        }

        public DateTimePeriod Period
        {
            get { return _period; }
        }

	    public bool EditLayer
	    {
		    get { return _editLayer; }
		    set
		    {
			    if (_editLayer == value) return;
			    _editLayer = value;
				notifyPropertyChanged("EditLayer");
		    }
	    }

	    public static readonly DependencyProperty ShowNextActivityProperty =
            DependencyProperty.Register("ShowNextActivity", typeof(bool), typeof(DayLayerModel),
                                        new UIPropertyMetadata(false));

	    private bool _editLayer;
	    private bool _hasAlarm=false;

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
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

		public bool IsInEditMode
		{
			get { return _isInEditMode; }
			set
			{
				if (_isInEditMode == value) return;
				_isInEditMode = value;
				notifyPropertyChanged("IsInEditMode");
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

	    public object TimeInCurrentState
	    {
			get { return _elapsedTimeConverter.Convert(EnteredCurrentState,typeof(TimeSpan),null,null); }
	    }

	    public bool HasAlarm
	    {
		    get { return _hasAlarm; }
		    set
		    {
			    _hasAlarm = value;
				notifyPropertyChanged("");
		    }
	    }

    }
}