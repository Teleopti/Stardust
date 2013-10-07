using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Win.Common.Controls.DateSelection;
using Teleopti.Ccc.Win.ExceptionHandling;
using Teleopti.Ccc.Win.Forecasting.Forms.WorkloadDayTemplatesPages;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.WFControls
{
    public partial class WFTemplateTabs : WFBaseControl
    {
        private IList<WorkloadDayTemplatesDetailView> _detailViews;

        public WFTemplateTabs()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
            _detailViews = new List<WorkloadDayTemplatesDetailView>();
        }

        protected override void SetColors()
        {
            tabControlAdv1.SelectedTab.TabForeColor = ColorHelper.TabForegroundColor();
            tabControlAdv1.TabPanelBackColor = ColorHelper.TabBackColor();
        }

        public bool GoToNextTab(Control forwardButton)
        {
            int i = tabControlAdv1.SelectedIndex;//old index
            tabControlAdv1.SelectedTab = TabHandler.TabForward(tabControlAdv1);
            if (i == 5)
                forwardButton.Enabled = false;
            else
                forwardButton.Enabled = true;
            return i != tabControlAdv1.SelectedIndex;//if the index has changed, focus should stay here
        }

        public bool GoToPreviousTab()
        {//it is not pretty but it works 
            int i = tabControlAdv1.SelectedIndex;//old index
            tabControlAdv1.SelectedTab = TabHandler.TabBack(tabControlAdv1);
            return i != tabControlAdv1.SelectedIndex;//if the index has changed, focus should stay here
        }

        private void InitializeTabs()
        {
            IList<DayOfWeek> weekDays = DateHelper.GetDaysOfWeek(CultureInfo.CurrentCulture);

            for (int i = 0; i < weekDays.Count; i++)
            {
                TabPageAdv theTabPage = tabControlAdv1.TabPages[i];

                var detailView = new WorkloadDayTemplatesDetailView(Presenter.Model.Workload, weekDays[i]);
                detailView.DateRangeChanged += detailView_DateRangeChanged;
                detailView.FilterDataViewClosed += detailView_FilterDataViewClosed;
                _detailViews.Add(detailView);
                theTabPage.Controls.Add(detailView);
                detailView.Dock = DockStyle.Fill;
                theTabPage.Tag = weekDays[i];
                theTabPage.Text = CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(weekDays[i]);
            }
        }

        private void detailView_FilterDataViewClosed(object sender, FilterDataViewClosedEventArgs e)
        {
            try
            {
                Presenter.ReloadFilteredWorkloadDayTemplates(e.FilteredDates);
            }
            catch (DataSourceException dataSourceException)
            {
                ShowDataSourceExcetionDialog(dataSourceException);
                return;
            }
            ReloadTemplateViews();
        }

        private void detailView_DateRangeChanged(object sender, DateRangeChangedEventArgs e)
        {
            IList<IWorkloadDayBase> historicalWorkloadDays;
            try
            {
                Presenter.SetSelectedHistoricTemplatePeriod(e.SelectedDates);
                Presenter.LoadWorkloadDayTemplates(e.SelectedDates);
                historicalWorkloadDays = Presenter.GetWorkloadDaysForTemplatesWithStatistics();
            }
            catch (DataSourceException dataSourceException)
            {
                ShowDataSourceExcetionDialog(dataSourceException);
                return;
            }

            _detailViews.ForEach(dw =>
            {
                if (!dw.HasFilteredData())
                {
                    dw.SetSelectedDates(new List<DateOnlyPeriod>(e.SelectedDates));
                    dw.ReloadWorkloadDayTemplates();
                    dw.RefreshWorkloadDaysForTemplatesWithStatistics(historicalWorkloadDays);
                }

                if (sender.Equals(dw))
                {
                    try
                    {
                        Presenter.SetSelectedHistoricTemplatePeriod(e.SelectedDates);
                        Presenter.LoadWorkloadDayTemplates(e.SelectedDates);
                        historicalWorkloadDays = Presenter.GetWorkloadDaysForTemplatesWithStatistics();
                    }
                    catch (DataSourceException dataSourceException)
                    {
                        ShowDataSourceExcetionDialog(dataSourceException);
                        return;
                    }
                    dw.ReloadWorkloadDayTemplates();
                    dw.RefreshWorkloadDaysForTemplatesWithStatistics(historicalWorkloadDays);

                }
                

                dw.EnableFilterData(false);
                var selectedDates = new HashSet<DateOnly>();
                foreach (var dateOnlyPeriod in e.SelectedDates)
                {
                    foreach (var selectedDate in dateOnlyPeriod.DayCollection())
                    {
                        selectedDates.Add((selectedDate));
                    }
                }
                if (selectedDates.Any(x => (int)x.DayOfWeek == dw.TemplateIndex))
                {
                    dw.EnableFilterData(true);
                }
            });
        }

        private static void ShowDataSourceExcetionDialog(DataSourceException dataSourceException)
        {
            using (var view = new SimpleExceptionHandlerView(dataSourceException, UserTexts.Resources.OpenTeleoptiCCC, UserTexts.Resources.ServerUnavailable))
            {
                view.ShowDialog();
            }
        }

        public void ReloadTemplateViews()
		{
			_detailViews.ForEach(dw =>dw.ReloadWorkloadDayTemplates());
		}

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            InitializeTabs();
        }
        
		public override void PrepareSave()
		{
			_detailViews.ForEach(dw => dw.RefreshUpdatedDate());
		}

        private void ReleaseManagedResources()
        {
            if (_detailViews != null)
            {
                foreach (var detailView in _detailViews)
                {
                    detailView.DateRangeChanged -= detailView_DateRangeChanged;
                    detailView.FilterDataViewClosed -= detailView_FilterDataViewClosed;
                    detailView.Dispose();
                }
                _detailViews.Clear();
                _detailViews = null;
            }
        }
    }
}
