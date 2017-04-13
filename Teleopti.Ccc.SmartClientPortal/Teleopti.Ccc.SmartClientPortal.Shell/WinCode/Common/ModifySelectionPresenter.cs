using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.WinCode.Common
{
    public class ModifySelectionPresenter
    {
        private readonly IModifySelectionView _view;
        private readonly ModifyCalculator _model;

        public ModifySelectionPresenter(IModifySelectionView view, ModifyCalculator model)
        {
            _view = view;
            _model = model;
        }

        public void Initialize()
        {
            _view.InputState = 0;
            _view.InputType = "0";
            _view.ChosenAmount = _model.ChosenAmount;
            _view.Sum = Math.Round(_model.Sum, 1);
            _view.InputPercent = Math.Round(_model.Sum, 1).ToString(CultureInfo.CurrentCulture);
            _view.ModifiedSum = _model.ModifiedSum;
            _view.Average = Math.Round(_model.Average, 1);
            _view.StandardDev = Math.Round(_model.StandardDev, 1);
            _view.InputSmoothValue = "0";
        }

        public void Undo()
        {
            _view.InputPercent = _view.InputType == "0" ? Math.Round(_model.Sum, 1).ToString(CultureInfo.CurrentCulture) : "0";
        }

        public void Accept()
        {
            _view.Close();
            _view.SetDialogResult(DialogResult.OK);
        }

        public void Cancel()
        {
            _view.Close();
            _view.SetDialogResult(DialogResult.Cancel);
        }

        public void UpdateInputTotal()
        {
            var s = _view.InputPercent.ToString(CultureInfo.CurrentCulture);
            if (String.IsNullOrEmpty(s)) return;
            double result;
            if (!Double.TryParse(_view.InputPercent, out result))
            {
                if (_view.InputPercent != "-")
                {
                    _view.InputPercent = "";
                }
            }
            else
            {
                if (_view.InputType == "0")
                {
                    _model.CalculateCurrentPercentage(result);
                    Save(result);
                    _view.InputState = 0;
                }
                else
                {
                    _model.CalculateCurrentTotal(result);
                    Save(_model.UpdateTotal);
                    _view.InputState = 1;

                }
            }
        }

        public void UpdateNewSum()
        {
            var s = _view.InputPercent.ToString(CultureInfo.CurrentCulture);
            if (String.IsNullOrEmpty(s)) return;
            double result;
            if (!Double.TryParse(_view.InputPercent, out result))
            {
                _view.InputPercent = "";
            }
            else
            {
                if (_view.InputType == "0")
                {
                    _model.CalculateCurrentTotal(result);
                    _view.InputPercent = Math.Round(_model.UpdateTotal, 1).ToString(CultureInfo.CurrentCulture);
                    _view.InputState = 0;
                }
                else
                {
                    _model.CalculateCurrentPercentage(result);
                    _view.InputPercent = Math.Round(_model.UpdatePercent, 0).ToString(CultureInfo.CurrentCulture);
                    _view.InputState = 1;
                }
            }
        }

        public void UpdateSmoothList()
        {
            var s = _view.InputSmoothValue.ToString(CultureInfo.CurrentCulture);
            if (String.IsNullOrEmpty(s)) return;
            int result;
            if (Int32.TryParse(_view.InputSmoothValue, out result))
            {
                _model.SmoothenValues(result);
            }
            else
            {
                _model.SmoothenValues(1);
                _view.InputSmoothValue = "0";
            }
            _view.StandardDev = Math.Round(_model.StandardDev, 1);
        }

        public IList<double> Result()
        {
            int result;
            return Int32.TryParse(_view.InputSmoothValue, out result) ? _model.SmoothenedValues : _model.ModifiedValues;
        }
        
        public IList<double> OriginalResult()
        {
            return _model.OriginalValues;
        }

        private void Save(double total)
        {
            _model.ModifyTotal(total);
            UpdateSmoothList();
            _view.ModifiedSum = total;
            _view.Average = Math.Round(_model.Average, 1);
        }
    }
}
