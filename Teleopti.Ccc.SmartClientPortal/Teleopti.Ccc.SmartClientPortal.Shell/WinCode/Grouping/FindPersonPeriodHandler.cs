

using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping
{
    public class FindPersonPeriodHandler
    {
        private readonly IFindPersonsModel _findPersonsModel;
        private readonly IFindPersonsView _findPersonsView;

        public FindPersonPeriodHandler(IFindPersonsModel findPersonsModel, IFindPersonsView findPersonsView)
        {
            _findPersonsModel = findPersonsModel;
            _findPersonsView = findPersonsView;
        }

        public bool CheckPeriod()
        {
            return _findPersonsView.FromDate <= _findPersonsView.ToDate;
        }

        public void FromDateChanged()
        {
            if (CheckPeriod())
            {
                _findPersonsModel.FromDate = new DateOnly(_findPersonsView.FromDate);
                _findPersonsView.ClearDateErrors();
                _findPersonsView.TextBoxFindEnabled = true;
            }
            else
            {
                _findPersonsView.SetErrorOnStartDate(UserTexts.Resources.StartDateMustBeSmallerThanEndDate);
                _findPersonsView.TextBoxFindEnabled = false;
            }
        }

        public void ToDateChanged()
        {
            if (CheckPeriod())
            {
                _findPersonsModel.ToDate = new DateOnly(_findPersonsView.ToDate);
                _findPersonsView.ClearDateErrors();
                _findPersonsView.TextBoxFindEnabled = true;
            }
            else
            {
                _findPersonsView.SetErrorOnEndDate(UserTexts.Resources.StartDateMustBeSmallerThanEndDate);
                _findPersonsView.TextBoxFindEnabled = false;
            }
        }
        
    }
}
