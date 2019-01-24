using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Grouping
{
	public class FindPersonsPresenter
	{
		private readonly IFindPersonsView _view;
		private readonly FindPersonsModel _model;
		private readonly PersonFinderService _personFinderService;
		private readonly FindPersonPeriodHandler _findPersonPeriodHandler;

		public FindPersonsPresenter(IFindPersonsView view, FindPersonsModel model, IApplicationFunction applicationFunction, IComponentContext container)
		{
			if (model == null)
				throw new ArgumentNullException("model");


			_view = view;
			_model = model;
			var personIndexBuilder = new PersonIndexBuilder(applicationFunction, _model.Persons, container.Resolve<ITenantLogonDataManagerClient>());
			_personFinderService = new PersonFinderService(personIndexBuilder);
			_findPersonPeriodHandler = new FindPersonPeriodHandler(_model, _view);
		}

		public void Initialize()
		{
			_view.FromDate = _model.FromDate.Date;
			_view.ToDate = _model.ToDate.Date;
		}

		public void ToDateChanged()
		{
			_findPersonPeriodHandler.ToDateChanged();
			if (_findPersonPeriodHandler.CheckPeriod())
			{
				RefreshResult();
			}
		}

		public void FromDateChanged()
		{
			_findPersonPeriodHandler.FromDateChanged();
			if (_findPersonPeriodHandler.CheckPeriod())
			{
				RefreshResult();
			}
		}

		public void RefreshResult()
		{
			IList<TreeNodeAdv> nodes = new List<TreeNodeAdv>();
			var commonNameDescription = new CommonNameDescriptionSetting();
			foreach (var person in _personFinderService.Find(_view.FindText, new DateOnlyPeriod(_model.FromDate, _model.ToDate)))
			{
				var personNode = new TreeNodeAdv(commonNameDescription.BuildFor(person));
				personNode.TagObject = person;
				personNode.Tag = GroupingConstants.NodeTypePerson;
				personNode.LeftImageIndices = new[] { 0 };
				nodes.Add(personNode);
			}
			_view.Result.Clear();
			_view.Result.AddRange(nodes.ToArray());
		}
	}
}