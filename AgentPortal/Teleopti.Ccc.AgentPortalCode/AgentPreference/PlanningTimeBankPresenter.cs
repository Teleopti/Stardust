using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.AgentPortalCode.AgentPreference
{
    
    public interface IPlanningTimeBankView
    {
        void SetBalanceInText(string balanceInText);
        TimeSpan BalanceOut { get; set; }
        void SetMaxBalanceOut(TimeSpan maxBalanceOut);
        void SetInfoMinMaxBalance(string infoText);
        void SetEnableStateOnSave(bool enabled);
        void SetErrorText(string errorText);
    }

    public class PlanningTimeBankPresenter
    {
        private readonly IPlanningTimeBankView _planningTimeBankView;
        private readonly IPlanningTimeBankModel _planningTimeBankModel;
        private readonly IPlanningTimeBankHelper _planningTimeBankHelper;
        private readonly CultureInfo _cultureInfo;

        public PlanningTimeBankPresenter(IPlanningTimeBankView planningTimeBankView, IPlanningTimeBankModel planningTimeBankModel,
            IPlanningTimeBankHelper planningTimeBankHelper, CultureInfo cultureInfo)
        {
            _planningTimeBankView = planningTimeBankView;
            _planningTimeBankModel = planningTimeBankModel;
            _planningTimeBankHelper = planningTimeBankHelper;
            _cultureInfo = cultureInfo;
        }

        public void Initialize()
        {
            _planningTimeBankView.SetBalanceInText(TimeHelper.GetLongHourMinuteTimeString(_planningTimeBankModel.BalanceIn, _cultureInfo));
            _planningTimeBankView.BalanceOut = _planningTimeBankModel.BalanceOut;
            _planningTimeBankView.SetMaxBalanceOut(_planningTimeBankModel.TimeBankMax);
            _planningTimeBankView.SetEnableStateOnSave(_planningTimeBankModel.CanSetBalanceOut);
            
            var minString = TimeHelper.GetLongHourMinuteTimeString(_planningTimeBankModel.TimeBankMin, _cultureInfo);
            var maxString = TimeHelper.GetLongHourMinuteTimeString(_planningTimeBankModel.TimeBankMax, _cultureInfo);
            _planningTimeBankView.SetInfoMinMaxBalance(string.Format(_cultureInfo, UserTexts.Resources.PlanningTimeBankMinMaxInfo, minString, maxString));
        }

        public bool Save()
        {
            if (!Validate())
            {
                return false;
            }
            _planningTimeBankHelper.SaveWantedBalanceOut(_planningTimeBankView.BalanceOut);
            return true;
        }

        private bool Validate()
        {
            _planningTimeBankView.SetErrorText("");
            var balanceOut = _planningTimeBankView.BalanceOut;
            if (balanceOut < _planningTimeBankModel.TimeBankMin)
            {
                _planningTimeBankView.SetErrorText(UserTexts.Resources.PlanningTimeBankLimitError);
                return false;
            }
            if (balanceOut > _planningTimeBankModel.TimeBankMax)
            {
                _planningTimeBankView.SetErrorText(UserTexts.Resources.PlanningTimeBankLimitError);
                return false;
            }
            return true;
        }
    }

    
}