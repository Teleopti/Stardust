using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Common
{
    public class ModifyCalculator
    {
        private IList<double> _modifyArray;
        private IList<double> _smoothenList = new List<double>();
        private readonly double _chosenAmount;
        private double _modifier;
        private readonly IList<double> _originalArray;

        public ModifyCalculator(IList<double> doubles)
        {
            _originalArray = doubles;//Saving original selected amounts

            _chosenAmount = _originalArray.Count;//Selected days

            CalculateSum(_originalArray);//Calculates original sum

            calculateAverage();

            calculateStdevSum(_originalArray);

            _modifyArray = _originalArray;

            ModifiedSum = Sum;
        }

        private void calculateAverage()
        {
            Average = ModifiedSum / _chosenAmount;//Average of selected days
        }

        private void calculateStdevSum(IList<double> doubles)
        {
            StandardDev = 0;
            foreach (var d in doubles)
            {
                StandardDev = StandardDev + Math.Pow((d - Average), 2);
            }
            StandardDev = Math.Sqrt(StandardDev / _chosenAmount);
        }

        public void ModifyTotal(double newTotal)
        {
            calculateAverage();
            if (Sum != 0)
            {
                _modifier = newTotal / Sum;
            }
            else
            {
                _modifier = 0;
            }
            IList<double> onesList = new List<double>();
            onesList.Clear();
            foreach (var d in _originalArray)
            {
                if (d == 0)
                {
                    onesList.Add(1);
                }
            }
            IList<double> modifyUpdateList = new List<double>();
            if (onesList.Count == _chosenAmount)
            {
                for (var i = 0; i < _chosenAmount; i++)
                {
                    modifyUpdateList.Add(newTotal / _chosenAmount);
                }
            }
            else
            {
                foreach (var d in _originalArray)
                {
                    var modifiedVal = _modifier * d;
                    modifyUpdateList.Add(modifiedVal);
                }
            }
            _modifyArray = modifyUpdateList;
            ModifiedSum = _modifyArray.Sum();
            calculateAverage();
            calculateStdevSum(_modifyArray);
        }

        public void CalculateSum(IList<double> doubles)
        {
            foreach (var i in doubles)
            {
                Sum += i;//Sum of original values in selection
            }
        }

        public double ChosenAmount
        {
            get
            {
                return _chosenAmount;
            }
        }

        public double Average { get; private set; }

        public double StandardDev { get; private set; }

        public double Sum { get; private set; }

        public IList<double> ModifiedValues
        {
            get
            {
                return _modifyArray;
            }
        }

        public IList<double> OriginalValues
        {
            get
            {
                return _originalArray;
            }
        }

        public IList<double> SmoothenedValues
        {
            get
            {
                return _smoothenList;
            }
        }

        public double ModifiedSum { get; private set; }

        public double UpdatePercent { get; private set; }

        public double UpdateTotal { get; private set; }

        public void CalculateCurrentTotal(double result)
        {
            var inputPercent = (result + 100) / 100;
            UpdateTotal = inputPercent * Sum;
        }

        public void CalculateCurrentPercentage(double result)
        {
            double inputTotal = 0;
            if (Sum != 0)
                inputTotal = result / Sum;
            UpdatePercent = (inputTotal * 100) - 100;
        }

        public void SmoothenValues(int smooth)
        {
            var smoothen = smooth;
            _smoothenList.Clear();
            for (var i = 0; i < _chosenAmount; i++)
            {
                double division = 1;
                var savedItemValue = _modifyArray[i];
                for (var j = 1; j <= smoothen/2; j++)
                {
                    if ((i - j) >= 0)
                    {
                        savedItemValue = savedItemValue + _modifyArray[i - j];
                        division++;
                    }
                    if ((i + j) >= _chosenAmount) continue;
                    savedItemValue = savedItemValue + _modifyArray[i + j];
                    division++;
                }
                var dividedValue = savedItemValue / division;
                _smoothenList.Add(dividedValue);
            }
            double smoothSum = 0;

            foreach (var i in _smoothenList)
            {
                smoothSum += i;
            }
            double factor = 0;
            if (smoothSum != 0)
            {
                factor = ModifiedSum/smoothSum;
            }
            IList<double> modifiedSmoothenList = new List<double>();

            foreach (var d in _smoothenList)
            {
                modifiedSmoothenList.Add(factor * d);
            }

            _smoothenList = modifiedSmoothenList;
            calculateStdevSum(_smoothenList);
        }
    }
}
