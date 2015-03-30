using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Grouping
{
    public class FindPersonsPresenter
    {
        private readonly IFindPersonsView _view;
        private readonly FindPersonsModel _model;
        private readonly PersonFinderService _personFinderService;
        private readonly PersonIndexBuilder _personIndexBuilder;
        private readonly FindPersonPeriodHandler _findPersonPeriodHandler;

        public FindPersonsPresenter(IFindPersonsView view, FindPersonsModel model, IApplicationFunction applicationFunction)
        {
            if(model == null)
                throw new ArgumentNullException("model");

            _view = view;
            _model = model;
            _personIndexBuilder = new PersonIndexBuilder(applicationFunction, _model.Persons, new DateOnlyPeriod(_model.FromDate, _model.ToDate));
            _personFinderService = new PersonFinderService(_personIndexBuilder);
            _findPersonPeriodHandler = new FindPersonPeriodHandler(_model, _view, _personIndexBuilder, _personFinderService);
        }

        public void Initialize()
        {
            _view.FromDate = _model.FromDate.Date;
            _view.ToDate = _model.ToDate.Date;
        }

        public void ToDateChanged()
        {
            _findPersonPeriodHandler.ToDateChanged();
        }

        public void FromDateChanged()
        {
            _findPersonPeriodHandler.FromDateChanged();
        }

        public void RefreshResult()
        {
            IList<TreeNodeAdv> nodes = new List<TreeNodeAdv>();
        	var commonNameDescription = new CommonNameDescriptionSetting();
            foreach (var person in _personFinderService.Find(_view.FindText))
            {
                var personNode = new TreeNodeAdv(commonNameDescription.BuildCommonNameDescription(person));
                personNode.TagObject = person;
                personNode.Tag = GroupingConstants.NodeTypePerson;
                personNode.LeftImageIndices = new[] {0};
                nodes.Add(personNode);
            }
            _view.Result.Clear();
            _view.Result.AddRange(nodes.ToArray());
        }
    }
}