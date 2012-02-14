﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using log4net;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.Win.Forecasting.Forms.WFControls;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{

    public partial class ForecastWorkflow : BaseRibbonForm, IForecastWorkflowView
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(ForecastWorkflow));
        private IList<WFBaseControl> _wfControls;
        private readonly ForecastWorkflowPresenter _presenter;
        private IFinishWorkload finishWorkload;

        #region Constructors

        protected ForecastWorkflow()
        {
            InitializeComponent();
            if (DesignMode) return;
        }

        public ForecastWorkflow(IWorkload workload)
            : this()
        {
            SetColor();
            SetTexts();

            _presenter = new ForecastWorkflowPresenter(this, new ForecastWorkflowModel(workload, new ForecastWorkflowDataService(UnitOfWorkFactory.Current)));
        }

        public void OutlierChanged(IOutlier outlier)
        {
            _wfControls.ForEach(w => w.OutlierRootChanged(outlier));
        }

        public ForecastWorkflow(IWorkload workload, IScenario workingScenario, DateOnlyPeriod period, IList<ISkillDay> skillDays, IFinishWorkload finishWorkload)
            : this(workload)
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

        private void SetColor()
        {
            tabControlAdv1.SelectedTab.TabForeColor = ColorHelper.TabBackColor();
            var panelbrush = ColorHelper.ControlGradientPanelBrush();
            gradientPanel1.BackgroundColor = panelbrush;
        }

        #region Navigation

        public void ShowWorkloadDayTemplateManager()
        {
            tabControlAdv1.SelectedTab = tabTemplation;
            tabControlAdv1.TabPages.Remove(tabSeason);
            tabControlAdv1.TabPages.Remove(tabValidation);
        }

        public void ShowValidateManager()
        {
            tabControlAdv1.SelectedTab = tabValidation;
            tabControlAdv1.TabPages.Remove(tabSeason);
            tabControlAdv1.TabPages.Remove(tabTemplation);
            btnForward.Visible = false;
            btnBack.Visible = false;
        }

        private void forward_clicked(object sender, EventArgs e)
        {
            GoForward();
        }

        private void GoForward()
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

        /// <summary>
        /// Handles the clicked event of the back button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: östenp
        /// Created date: 2/19/2008
        /// </remarks>
        private void back_clicked(object sender, EventArgs e)
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

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _wfControls.ForEach(w => w.Cancel());
            Close();
        }

        private void btnFinish_Click(object sender, EventArgs e)
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

        private void ReleaseManagedResources()
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
    }

    public interface IForecastWorkflowView
    {
        void SetWorkloadName(string name);
        void OutlierChanged(IOutlier outlier);
    }
}