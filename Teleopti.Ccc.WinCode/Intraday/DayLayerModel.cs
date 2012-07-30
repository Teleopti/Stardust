using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public class DayLayerModel : DependencyObject, INotifyPropertyChanged
    {
        private readonly IPerson _person;
        private readonly DateTimePeriod _period;
        private readonly ITeam _team;
        private readonly LayerViewModelCollection _layerViewModelCollection;
        private readonly ICommonNameDescriptionSetting _commonNameDescriptionSetting;
        private ILayer<IPayload> _currentActivityLayer;
        private ILayer<IPayload> _alarmLayer;
        private ILayer<IPayload> _currentState;
        private ILayer<IPayload> _nextActivityLayer;
        private DateTime _scheduleStartDateTime = DateTime.MaxValue;
        private bool _isPinned;

        public DayLayerModel(IPerson person, DateTimePeriod period, ITeam team, LayerViewModelCollection layerViewModelCollection, ICommonNameDescriptionSetting commonNameDescriptionSetting)
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
            get
            {
                return _commonNameDescriptionSetting.BuildCommonNameDescription(_person);
            }
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

        public ILayer<IPayload> AlarmLayer
        {
            get { return _alarmLayer; }
            set
            {
                if (_alarmLayer != value)
                {
                    _alarmLayer = value;
                    notifyPropertyChanged("AlarmLayer");
                    notifyPropertyChanged("AlarmDescription");
                }
            }
        }

        public string AlarmDescription
        {
            get { return getPayloadDescription(_alarmLayer); }
        }

        //This property is only used for sorting the Time in the grid
        public TimeSpan SortTime
        {
            get
            {
                return AlarmLayer == null ?
                                              TimeSpan.Zero :
                                                                TimeSpan.FromSeconds(Math.Round(AlarmLayer.Period.ElapsedTime().TotalSeconds));
            }
        }

        public ILayer<IPayload> NextActivityLayer
        {
            get { return _nextActivityLayer; }
            set
            {
                if (_nextActivityLayer != value)
                {
                    _nextActivityLayer = value;
                    notifyPropertyChanged("NextActivityLayer");
                    notifyPropertyChanged("NextActivityDescription");
                    notifyPropertyChanged("NextActivityStartDateTime");
                    ShowNextActivity = _nextActivityLayer != null;
                }
            }
        }

        public string NextActivityDescription
        {
            get { return getPayloadDescription(_nextActivityLayer); }
        }

        public string NextActivityStartDateTime
        {
            get
            {
                if (_nextActivityLayer != null)
                    return _nextActivityLayer.Period.StartDateTime.ToShortTimeString();

                return String.Empty;
            }
        }

        public DateTime ScheduleStartDateTime
        {
            get
            {
                return _scheduleStartDateTime;
            }
            set { 
                _scheduleStartDateTime = value;
                notifyPropertyChanged("ScheduleStartDateTime");
            }
        }

        public ILayer<IPayload> CurrentActivityLayer
        {
            get { return _currentActivityLayer; }
            set
            {
                if (_currentActivityLayer != value)
                {
                    _currentActivityLayer = value;
                    notifyPropertyChanged("CurrentActivityLayer");
                    notifyPropertyChanged("CurrentActivityDescription");

                }
            }
        }

        public string CurrentActivityDescription
        {
            get { return getPayloadDescription(_currentActivityLayer); }
        }

        public ILayer<IPayload> CurrentState
        {
            get { return _currentState; }
            set
            {
                if (_currentState != value)
                {
                    _currentState = value;
                    notifyPropertyChanged("CurrentState");
                    notifyPropertyChanged("CurrentStateDescription");
                }
            }
        }

        public string CurrentStateDescription
        {
            get { return getPayloadDescription(_currentState); }
        }

        public bool ShowNextActivity
        {
            get { return (bool)GetValue(ShowNextActivityProperty); }
            set { SetValue(ShowNextActivityProperty, value); }
        }

        public DateTimePeriod Period
        {
            get {
                return _period;
            }
        }

        public static readonly DependencyProperty ShowNextActivityProperty =
            DependencyProperty.Register("ShowNextActivity", typeof(bool), typeof(DayLayerModel), new UIPropertyMetadata(false));

        private string getPayloadDescription(ILayer layer)
        {
            if (layer == null) return string.Empty;
            var payload = layer.Payload as IPayload;
            return payload.ConfidentialDescription(_person,new DateOnly(layer.Period.StartDateTimeLocal(TimeZoneHelper.CurrentSessionTimeZone))).Name;
        }

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
    }
}