using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public class StaffingEffectViewAdapter : INotifyPropertyChanged
    {
        private readonly IDayLayerViewModel _dayLayerViewModel;

        public StaffingEffectViewAdapter(IDayLayerViewModel dayLayerViewModel)
        {
            _dayLayerViewModel = dayLayerViewModel;
            ((ObservableCollection<DayLayerModel>)_dayLayerViewModel.Models).CollectionChanged += _layerViewAdapters_CollectionChanged;
            CalculateEffects();
            foreach (var model in _dayLayerViewModel.Models)
            {
                model.PropertyChanged += adapter_PropertyChanged;
            }
        }

        void _layerViewAdapters_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CalculateEffects();
            if (e.OldItems != null)
            {
                foreach (DayLayerModel model in e.OldItems)
                {
                    model.PropertyChanged -= adapter_PropertyChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (DayLayerModel model in e.NewItems)
                {
                    model.PropertyChanged += adapter_PropertyChanged;
                }
            }
        }

        void adapter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AlarmLayer")
                CalculateEffects();
        }

        private void CalculateEffects()
        {
            double poseff=0d;
            double negeff=0d;
            int poscount = 0;
            int negCount = 0;
            foreach (var model in _dayLayerViewModel.Models)
            {
                if (model.AlarmLayer == null)
                    continue;

                IAlarmType type = model.AlarmLayer.Payload as IAlarmType;
                if (type.StaffingEffect > 0)
                {
                    poseff += type.StaffingEffect;
                    poscount++;
                }
                if (type.StaffingEffect < 0)
                {
                    negCount++;
                    negeff += type.StaffingEffect;
                }
            }
            PositiveEffect = poseff;
            NegativeEffect = negeff;
            PositiveEffectPercent =(double) poscount /_dayLayerViewModel.Models.Count;
            NegativeEffectPercent = (double)negCount/_dayLayerViewModel.Models.Count;
            Total = poseff + negeff;
            TotalPercent = PositiveEffectPercent + NegativeEffectPercent;
        }

        private double _negativeEffect;
        public double NegativeEffect
        {
            get { return _negativeEffect; }
            set 
            {
                if (_negativeEffect != value)
                {
                    _negativeEffect = value;
                    NotifyPropertyChanged("NegativeEffect");
                }
            }
        }
        private double _total;
        public double Total
        {
            get { return _total; }
            set
            {
                if (_total != value)
                {
                    _total = value;
                    NotifyPropertyChanged("Total");
                }
            }
        }      
        
        private double _totalPercent;
        public double TotalPercent
        {
            get { return _totalPercent; }
            set
            {
                if (_totalPercent != value)
                {
                    _totalPercent = value;
                    NotifyPropertyChanged("TotalPercent");
                }
            }
        }
        private double _positiveEffectPercent;
        public double PositiveEffectPercent
        {
            get { return _positiveEffectPercent; }
            set
            {
                if (_positiveEffectPercent != value)
                {
                    _positiveEffectPercent = value;
                    NotifyPropertyChanged("PositiveEffectPercent");
                }
            }
        }

        private double _negativeEffectPercent;
        public double NegativeEffectPercent
        {
            get { return _negativeEffectPercent; }
            set
            {
                if (_negativeEffectPercent != value)
                {
                    _negativeEffectPercent = value;
                    NotifyPropertyChanged("NegativeEffectPercent");
                }
            }
        }

        private double _positiveEffect;

        public double PositiveEffect
        {
            get { return _positiveEffect; }
            set
            {
                if (_positiveEffect != value)
                {
                    _positiveEffect = value;
                    NotifyPropertyChanged("PositiveEffect");
                }
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
        	var handler = PropertyChanged;
            if (handler!= null)
            {
            	handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
