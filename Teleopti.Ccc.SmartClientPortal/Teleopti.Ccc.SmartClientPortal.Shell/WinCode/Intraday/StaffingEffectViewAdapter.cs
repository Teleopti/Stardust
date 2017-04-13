using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Teleopti.Ccc.WinCode.Intraday
{
    public class StaffingEffectViewAdapter : INotifyPropertyChanged
    {
        private readonly IDayLayerViewModel _dayLayerViewModel;

        public StaffingEffectViewAdapter(IDayLayerViewModel dayLayerViewModel)
        {
            _dayLayerViewModel = dayLayerViewModel;
            ((ObservableCollection<DayLayerModel>)_dayLayerViewModel.Models).CollectionChanged += LayerViewAdaptersCollectionChanged;
            CalculateEffects();
            foreach (var model in _dayLayerViewModel.Models)
            {
                model.PropertyChanged += AdapterPropertyChanged;
            }
        }

        private void LayerViewAdaptersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CalculateEffects();
            if (e.OldItems != null)
            {
                foreach (DayLayerModel model in e.OldItems)
                {
                    model.PropertyChanged -= AdapterPropertyChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (DayLayerModel model in e.NewItems)
                {
                    model.PropertyChanged += AdapterPropertyChanged;
                }
            }
        }

        protected void AdapterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
			if (e.PropertyName == "AlarmDescription")
                CalculateEffects();
        }

        private void CalculateEffects()
        {
            var poseff = 0d;
            var negeff = 0d;
            var poscount = 0;
            var negCount = 0;

            foreach (var model in _dayLayerViewModel.Models)
            {
                if (model.StaffingEffect > 0)
                {
                    poseff += model.StaffingEffect;
                    poscount++;
                }
                if (model.StaffingEffect < 0)
                {
                    negeff += model.StaffingEffect;
                    negCount++;
                }
            }
            PositiveEffect = poseff;
            NegativeEffect = negeff;
            PositiveEffectPercent = (double)poscount / _dayLayerViewModel.Models.Count;
            NegativeEffectPercent = (double)negCount / _dayLayerViewModel.Models.Count;
            Total = poseff + negeff;
            TotalPercent = PositiveEffectPercent + NegativeEffectPercent;
        }

        private double _negativeEffect;
        public double NegativeEffect
        {
            get { return _negativeEffect; }
            set
            {
	            if (_negativeEffect.Equals(value)) return;
	            _negativeEffect = value;
	            NotifyPropertyChanged(nameof(NegativeEffect));
            }
        }
        private double _total;
        public double Total
        {
            get { return _total; }
            set
            {
	            if (_total.Equals(value)) return;
	            _total = value;
	            NotifyPropertyChanged(nameof(Total));
            }
        }

        private double _totalPercent;
        public double TotalPercent
        {
            get { return _totalPercent; }
            set
            {
	            if (_totalPercent.Equals(value)) return;
	            _totalPercent = value;
	            NotifyPropertyChanged(nameof(TotalPercent));
            }
        }
        private double _positiveEffectPercent;
        public double PositiveEffectPercent
        {
            get { return _positiveEffectPercent; }
            set
            {
	            if (_positiveEffectPercent.Equals(value)) return;
	            _positiveEffectPercent = value;
	            NotifyPropertyChanged(nameof(PositiveEffectPercent));
            }
        }

        private double _negativeEffectPercent;
        public double NegativeEffectPercent
        {
            get { return _negativeEffectPercent; }
            set
            {
	            if (_negativeEffectPercent.Equals(value)) return;
	            _negativeEffectPercent = value;
	            NotifyPropertyChanged(nameof(NegativeEffectPercent));
            }
        }

        private double _positiveEffect;

        public double PositiveEffect
        {
            get { return _positiveEffect; }
            set
            {
	            if (_positiveEffect.Equals(value)) return;
	            _positiveEffect = value;
	            NotifyPropertyChanged(nameof(PositiveEffect));
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
    }
}
