using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using Teleopti.Ccc.WinCode.Common;
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
        private DateTime _alarmStart;

	    private DayLayerModel _transactionCopy;
	    private bool _isInEditMode;

        public DayLayerModel(IPerson person, DateTimePeriod period, ITeam team,
                             LayerViewModelCollection layerViewModelCollection,
                             ICommonNameDescriptionSetting commonNameDescriptionSetting)
        {
            _person = person;
            _layerViewModelCollection = layerViewModelCollection;
            _commonNameDescriptionSetting = commonNameDescriptionSetting;
            _team = team;
            _period = period;
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
                if (_isPinned != value)
                {
                    _isPinned = value;
                    notifyPropertyChanged("IsPinned");
                }
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
            get { return Color.FromArgb(_colorValue); }
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
        public DateTime AlarmStart
        {
            get { return _alarmStart; }
            set { _alarmStart = value; }
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

        public static readonly DependencyProperty ShowNextActivityProperty =
            DependencyProperty.Register("ShowNextActivity", typeof(bool), typeof(DayLayerModel),
                                        new UIPropertyMetadata(false));

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
			_transactionCopy = new DayLayerModel(_person, _period, _team, _layerViewModelCollection, _commonNameDescriptionSetting)
				{
					AlarmDescription = _alarmDescription,
					AlarmStart = _alarmStart,
					ColorValue = _colorValue,
					CurrentActivityDescription = _currentActivityDescription,
					CurrentStateDescription = _currentStateDescription,
					EnteredCurrentState = _enteredCurrentState,
					IsPinned = _isPinned,
					NextActivityDescription = _nextActivityDescription,
					NextActivityStartDateTime = _nextActivityStartDateTime,
					ScheduleStartDateTime = _scheduleStartDateTime,
					StaffingEffect = _staffingEffect
				};
			IsInEditMode = true;
		}

		public void EndEdit()
		{
			_transactionCopy = null;
			IsInEditMode = false;
		}

		public void CancelEdit()
		{
			if (_transactionCopy != null)
			{
				AlarmDescription = _transactionCopy.AlarmDescription;
				AlarmStart = _transactionCopy.AlarmStart;
				ColorValue = _transactionCopy.ColorValue;
				CurrentActivityDescription = _transactionCopy.CurrentActivityDescription;
				CurrentStateDescription = _transactionCopy.CurrentStateDescription;
				EnteredCurrentState = _transactionCopy.EnteredCurrentState;
				IsPinned = _transactionCopy.IsPinned;
				NextActivityDescription = _transactionCopy.NextActivityDescription;
				NextActivityStartDateTime = _transactionCopy.NextActivityStartDateTime;
				ScheduleStartDateTime = _transactionCopy.ScheduleStartDateTime;
				StaffingEffect = _transactionCopy.StaffingEffect;
				_transactionCopy = null;
			}
			IsInEditMode = false;
		}
    }
}