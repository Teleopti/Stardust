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

        private void setSelectedDateFilter( object sender, IList<DateOnlyPeriod> selectedDates)
        {
            _detailViews.ForEach(dw =>
            {
                if (sender.Equals(dw))
                {
                    IList<IWorkloadDayBase> historicalWorkloadDays;
                    try
                    {
                        Presenter.SetSelectedHistoricTemplatePeriod(selectedDates);
                        Presenter.LoadWorkloadDayTemplates(selectedDates);
                        historicalWorkloadDays = Presenter.GetWorkloadDaysForTemplatesWithStatistics();
                    }
                    catch (DataSourceException dataSourceException)
                    {
                        ShowDataSourceExcetionDialog(dataSourceException);
                        return;
                    }
                    dw.RefreshWorkloadDaysForTemplatesWithStatistics(historicalWorkloadDays);

                }

				dw.ReloadWorkloadDayTemplates();
                
                dw.EnableFilterData(false);
                var selectedDatesHastSet = new HashSet<DateOnly>();
                foreach (var dateOnlyPeriod in selectedDates)
                {
                    foreach (var selectedDate in dateOnlyPeriod.DayCollection())
                    {
                        selectedDatesHastSet.Add((selectedDate));
                    }
                }
                if (selectedDatesHastSet.Any(x => (int)x.DayOfWeek == dw.TemplateIndex))
                {
                    dw.EnableFilterData(true);
                }
            });
        }

        private void detailView_DateRangeChanged(object sender, DateRangeChangedEventArgs e)
        {

            initializeDateSelections(e.SelectedDates);
            setSelectedDateFilter(sender,e.SelectedDates);
        }

        private void initializeDateSelections(IList<DateOnlyPeriod> selectedDates)
        {
            IList<IWorkloadDayBase> historicalWorkloadDays;
            try
            {
                Presenter.SetSelectedHistoricTemplatePeriod(selectedDates);
                Presenter.LoadWorkloadDayTemplates(selectedDates);
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
                    dw.SetSelectedDates(new List<DateOnlyPeriod>(selectedDates));
                    dw.ReloadWorkloadDayTemplates();
                    dw.RefreshWorkloadDaysForTemplatesWithStatistics(historicalWorkloadDays);
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
