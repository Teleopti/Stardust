using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Reporting
{
	public partial class ReportPersonsSelectionView : BaseRibbonForm, IReportPersonsSelectionView
	{
		private readonly IPersonSelectorPresenter _personSelectorPresenter;
		private ReportPersonsSelectionView(){}

		public ReportPersonsSelectionView(DateOnlyPeriod dateOnlyPeriod, IEnumerable<Guid> selectedAgentGuids, IComponentContext componentContext, IApplicationFunction applicationFunction, string selectedGroupPage)
		{
			InitializeComponent();

			_personSelectorPresenter =
				componentContext.Resolve<ILifetimeScope>().BeginLifetimeScope().Resolve<IPersonSelectorPresenter>();
			_personSelectorPresenter.ApplicationFunction = applicationFunction;
			var view = (Control)_personSelectorPresenter.View;
			panel1.Controls.Add(view);
			view.Dock = DockStyle.Fill;

			var selectorView = _personSelectorPresenter.View;
			selectorView.SelectedPeriod = dateOnlyPeriod;
			_personSelectorPresenter.ShowPersons = true;
			_personSelectorPresenter.ShowUsers = false;
			selectorView.PreselectedPersonIds = new HashSet<Guid>(selectedAgentGuids);
			selectorView.ShowCheckBoxes = true;
			
			selectorView.ShowDateSelection = false;
			selectorView.HideMenu = true;
			_personSelectorPresenter.LoadTabs();
			_personSelectorPresenter.SetSelectedTab(selectedGroupPage);
			SetTexts();
		}

		public HashSet<Guid> SelectedAgentGuids()
		{
			return _personSelectorPresenter.CheckedPersonGuids;
		}

		public string SelectedGroupPageKey
		{
			get { return _personSelectorPresenter.SelectedGroupPageKey(); }
		}
	}
}
