using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using log4net;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.ExceptionHandling;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.WFControls;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms
{

	public partial class ForecastWorkflow : BaseDialogForm, IForecastWorkflowView
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(ForecastWorkflow));
		private IList<WFBaseControl> _wfControls;
		private readonly ForecastWorkflowPresenter _presenter;
		private readonly IFinishWorkload finishWorkload;
		private IStatisticHelper _statisticHelper;

		#region Constructors

		protected ForecastWorkflow(IStatisticHelper statisticHelper)
		{
			_statisticHelper = statisticHelper;
			InitializeComponent();
		}

		public ForecastWorkflow(IWorkload workload, IStatisticHelper statisticHelper)
			: this(statisticHelper)
		{
			SetTexts();

			_presenter = new ForecastWorkflowPresenter(this, new ForecastWorkflowModel(workload, new ForecastWorkflowDataService(UnitOfWorkFactory.Current, _statisticHelper)));
		}

		public void OutlierChanged(IOutlier outlier)
		{
			_wfControls.ForEach(w => w.OutlierRootChanged(outlier));
		}

		public ForecastWorkflow(IWorkload workload, IScenario workingScenario, DateOnlyPeriod period, IList<ISkillDay> skillDays, IFinishWorkload finishWorkload, IStatisticHelper statisticHelper)
			: this(workload, statisticHelper)
		{
			_presenter.Initialize(workingScenario, period, skillDays);
			_presenter.Locked = true;
			this.finishWorkload = finishWorkload;
		}

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				_presenter.Initialize();
			}
			catch (DataSourceException dataSourceException)
			{
				if (datasourceExceptionOccurred(dataSourceException))
					return;
			}

			_wfControls = new List<WFBaseControl> { wfTemplateTabs, wfSeasonalityTabs, wfValidate };

			foreach (var wfControl in _wfControls)
			{
				wfControl.Initialize(_presenter);
			}

			if (tabControlAdv1.SelectedTab.Controls.Count > 0)
				tabControlAdv1.SelectedTab.Controls[0].Select();
		}

		private bool datasourceExceptionOccurred(Exception exception)
		{
			if (exception != null)
			{
				_logger.Error("An error occurred in the forecast workflow.", exception);

				var dataSourceException = exception as DataSourceException;
				if (dataSourceException == null)
					return false;

				using (var view = new SimpleExceptionHandlerView(dataSourceException, UserTexts.Resources.OpenTeleoptiCCC, UserTexts.Resources.ServerUnavailable))
				{
					view.ShowDialog();
				}

				Close();
				return true;
			}
			return false;
		}


		#region Navigation

		private void forwardClicked(object sender, EventArgs e)
		{
			goForward();
		}

		private void goForward()
		{
			Cursor = Cursors.WaitCursor;
			string selectedTabTagValue = tabControlAdv1.SelectedTab.Tag.ToString();
			switch (selectedTabTagValue)
			{
				case "volume":
					if (!wfSeasonalityTabs.GoToNextTab())
						tabControlAdv1.SelectedTab = TabHandler.TabForward(tabControlAdv1);
					break;
				case "templation":
					if (!wfTemplateTabs.GoToNextTab(btnForward))
						tabControlAdv1.SelectedTab = TabHandler.TabForward(tabControlAdv1);
					break;
				default:
					tabControlAdv1.SelectedTab = TabHandler.TabForward(tabControlAdv1);
					break;
			}
			Cursor = Cursors.Default;
		}

		private void backClicked(object sender, EventArgs e)
		{
			string selectedTabTagValue = tabControlAdv1.SelectedTab.Tag.ToString();
			switch (selectedTabTagValue)
			{
				case "volume":
					if (!wfSeasonalityTabs.GoToPreviousTab())
					{
						tabControlAdv1.SelectedTab = TabHandler.TabBack(tabControlAdv1);
					}
					break;
				case "templation":
					if (!wfTemplateTabs.GoToPreviousTab())
					{
						tabControlAdv1.SelectedTab = TabHandler.TabBack(tabControlAdv1);
					}
					btnForward.Enabled = true;
					break;
				default:
					tabControlAdv1.SelectedTab = TabHandler.TabBack(tabControlAdv1);
					break;
			}
		}

		#endregion

		#region Button handlers

		private void btnCancelClick(object sender, EventArgs e)
		{
			_wfControls.ForEach(w => w.Cancel());
			Close();
		}

		private void btnFinishClick(object sender, EventArgs e)
		{
			_wfControls.ForEach(w => w.PrepareSave());
			try
			{
				_presenter.SaveWorkflow();
				Close();
				if (finishWorkload!= null)
					finishWorkload.ReloadForecaster();
			}

			catch (DataSourceException dataSourceException)
			{
				using (var view = new SimpleExceptionHandlerView(dataSourceException, UserTexts.Resources.OpenTeleoptiCCC, UserTexts.Resources.ServerUnavailable))
				{
					view.ShowDialog();
				}
			}
		}

		#endregion

		private void releaseManagedResources()
		{
			if (_wfControls != null)
			{
				foreach (var control in _wfControls)
				{
					control.Dispose();
				}
				_wfControls.Clear();
				_wfControls = null;
			}
			wfSeasonalityTabs = null;
			wfValidate = null;
			if (wfTemplateTabs != null)
			{
				wfTemplateTabs.Dispose();
				wfTemplateTabs = null;
			}
			if (_presenter.Model.SkillDays != null && !_presenter.Locked) _presenter.Model.SkillDays.Clear(); //todo: move to presenter method
		}

		public void SetWorkloadName(string name)
		{
			Text = string.Format(CultureInfo.CurrentCulture, "{0} - {1}", Text, name);
		}

		private void wfTemplateTabs_Load(object sender, EventArgs e)
		{

		}

		private void wfValidate_Load(object sender, EventArgs e)
		{

		}
	}

	public interface IForecastWorkflowView
	{
		void SetWorkloadName(string name);
		void OutlierChanged(IOutlier outlier);
	}
}