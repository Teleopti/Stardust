using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Grouping
{
    public class FindPersonPeriodHandler
    {
        private readonly IFindPersonsModel _findPersonsModel;
        private readonly IFindPersonsView _findPersonsView;
        private readonly IPersonIndexBuilder _personIndexBuilder;
        private readonly IPersonFinderService _personFinderService;

        public FindPersonPeriodHandler(IFindPersonsModel findPersonsModel, IFindPersonsView findPersonsView, IPersonIndexBuilder personIndexBuilder, IPersonFinderService personFinderService)
        {
            _findPersonsModel = findPersonsModel;
            _findPersonsView = findPersonsView;
            _personIndexBuilder = personIndexBuilder;
            _personFinderService = personFinderService;
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
                _personIndexBuilder.ChangePeriod(new DateOnlyPeriod(_findPersonsModel.FromDate, _findPersonsModel.ToDate));
                _personFinderService.RebuildIndex();
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
                _personIndexBuilder.ChangePeriod(new DateOnlyPeriod(_findPersonsModel.FromDate, _findPersonsModel.ToDate));
                _personFinderService.RebuildIndex();
            }
            else
            {
                _findPersonsView.SetErrorOnEndDate(UserTexts.Resources.StartDateMustBeSmallerThanEndDate);
                _findPersonsView.TextBoxFindEnabled = false;
            }
        }
        
    }
}
