using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Configuration;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.ConfigTool.Code.Gui.DataSourceConfiguration
{
    public class DataSourceConfigurationPresenter
    {
        private readonly IDataSourceConfigurationView _view;
        private readonly DataSourceConfigurationModel _model;

        private DataSourceConfigurationPresenter() { }

        public DataSourceConfigurationPresenter(IDataSourceConfigurationView view, DataSourceConfigurationModel model)
            : this()
        {
            _view = view;
            _model = model;
        }

        public void Initialize()
        {
            _view.SetOkButtonEnabled(false);
            _view.SetDataSource(_model.LoadDataSources());
            _view.SetTimeZoneDataSource(_model.LoadTimeZones());
            _view.DataSourceStatusColumnHeader = "Status";
            _view.DataSourceNameColumnHeader = "Name";
            _view.DataSourceTimeZoneColumnHeader = "Time Zone";
            _view.DataSourceIntervalLengthColumnHeader = "Interval Length";
            _view.DataSourceInactiveColumnHeader = "Inactive";
        }

        public void SetGridState()
        {
            if (_view.DataSource.Count == 0)
            {
                _view.SetToolStripState(false, "No data sources available! Data source dependent jobs cannot be executed.");
                return;
            }
            if (_view.IsEtlToolLoading)
                _view.SetToolStripState(true, "ETL Tool loading...");
            else
                _view.SetToolStripState(false, string.Empty);
            
            foreach (DataSourceRow dataSourceRow in _view.DataSource)
            {
                SetRowState(dataSourceRow);
            }
        }

        private bool doesDataSourcesToSaveExist()
        {
        	return _view.DataSource.Any(dataSourceRow => dataSourceRow.IsRowEnabled);
        }

    	private static bool isTimeZoneSet(DataSourceRow dataSource)
        {
            if (!string.IsNullOrEmpty(dataSource.TimeZoneCode))
            {
                return true;
            }
            return false;
        }

        public void SetRowState(DataSourceRow dataSourceRow)
        {
            bool isTimeZoneComboBoxEnabled = true;
            string info = "Data source is valid.";
            dataSourceRow.RowState = DataSourceState.Valid;

            if (isNewDataSourceInactive(dataSourceRow))
            {
                info = "WARNING! When a data source are set to Inactive it will be supressed in the future and not used by the system.";
                dataSourceRow.RowState = DataSourceState.Invalid;
                // If ds set to inactive then reset time zone
                dataSourceRow.TimeZoneId = "-1";
                dataSourceRow.TimeZoneCode = string.Empty;
                _view.SetTimeZoneSelected(dataSourceRow);
                isTimeZoneComboBoxEnabled = false;
            }
            else if (!isIntervalLengthValid(dataSourceRow))
            {
                info = "IMPORTANT! Invalid interval length.\r\a";
				info += string.Format(CultureInfo.CurrentCulture, "Valid interval length is '{0} minutes'.\r\a", _model.IntervalLengthMinutes);
                info += "One possible solution is to change the 'IntervalLengthMinutes' key in the mart.sys_setting data table.\r\a";
                info += "If you are uncertain what to do, please contact the Teleopti support.";
                dataSourceRow.RowState = DataSourceState.Error;
                if (!_view.IsEtlToolLoading)
                {
                    _view.SetOkButtonEnabled(false);
                }
            }
            else if (!isTimeZoneSet(dataSourceRow))
            {
                info = "WARNING! Time zone not assigned. This data source can not be used by the system.";
                dataSourceRow.RowState = DataSourceState.Invalid;
            }

            dataSourceRow.RowStateToolTip = info;

            _view.SetTimeZoneComboState(dataSourceRow.RowIndex, isTimeZoneComboBoxEnabled);
            _view.SetRowStateImage(dataSourceRow);
            _view.SetRowReadOnly(dataSourceRow);
        }

        private bool isIntervalLengthValid(DataSourceRow dataSourceRow)
        {
			if (_model.IntervalLengthMinutes == int.Parse(dataSourceRow.IntervalLengthText, CultureInfo.InvariantCulture))
            {
                return true;
            }
            return false;
        }

        private static bool isNewDataSourceInactive(DataSourceRow dataSourceRow)
        {
            if (dataSourceRow.IsRowEnabled && dataSourceRow.Inactive)
            {
                return true;
            }
            return false;
        }

        private bool allDataSourcesHaveCorrectIntervalLength()
        {
            if (_view.DataSource != null)
            {
            	return
            		_view.DataSource.All(
            			dataSource =>
            			dataSource.Inactive ||
            			_model.IntervalLengthMinutes == int.Parse(dataSource.IntervalLengthText, CultureInfo.InvariantCulture));
            }
            
            return true;
        }
        
        public void SetSaveState()
        {
            if (!_view.IsEtlToolLoading)
            {
                _view.SetOkButtonEnabled(allDataSourcesHaveCorrectIntervalLength());
            }
        }

        private IList<TimeZoneInfo> getUsedTimeZones()
        {
            IList<TimeZoneInfo> timeZoneList = new List<TimeZoneInfo>();

            foreach (DataSourceRow dataSourceRow in _view.DataSource)
            {
                if (!string.IsNullOrEmpty(dataSourceRow.TimeZoneCode))
                {
                    TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(dataSourceRow.TimeZoneCode);
                    if (!timeZoneList.Contains(tz))
                    {
                        timeZoneList.Add(tz);
                    }
                }
            }

            return timeZoneList;
        }

        private void prepareInitialJob(IJob initialJob)
        {
            // Get the jobparameters property from "stg_time_zone" job step
            IJobParameters jobParameters = initialJob.StepList[3].JobParameters;

            if (jobParameters != null)
            {
                // Add time zones used by data sources
                jobParameters.TimeZonesUsedByDataSources = getUsedTimeZones();

                // Set date period to load later on
                jobParameters.JobCategoryDates.Clear();
                var fromDate = DateTime.Today;
                DateTime toDate = fromDate.AddYears(1);
                IJobMultipleDateItem jobMultipleDateItem = new JobMultipleDateItem(DateTimeKind.Local, fromDate, toDate, _model.DefaultTimeZone);
                jobParameters.JobCategoryDates.Add(jobMultipleDateItem, JobCategoryType.Initial);
            }
        }

        public void InitiateSave(IJob initialJob)
        {
            if (!doesDataSourcesToSaveExist())
            {
                _view.CloseView();
                return;
            }

            _view.SetOkButtonEnabled(false);
            _view.SetViewEnabled(false);
            if (isThereAnyNewlySetTimeZonesToSave())
            {
                _view.SetToolStripState(true, "'Initial' job is running in the background. This can take several minutes...");
                _view.IsEtlToolLoading = true;
                prepareInitialJob(initialJob);
                _view.RunInitialJob();
            }
            else
            {
                //Try to save datasources set to incative
                _view.SetToolStripState(true, "Saving changes...");
                saveDataSourcesSetInactive();
                _view.IsSaved = false; // We don´t want to reload the datasources in the background after this kind of save.
                _view.CloseView();
            }
        }

        private bool isThereAnyNewlySetTimeZonesToSave()
        {
        	return _view.DataSource.Any(dataSourceRow => !string.IsNullOrEmpty(dataSourceRow.TimeZoneCode));
        }

    	public void SetViewReadyToSave(IJob initialJob)
        {
            _view.IsEtlToolLoading = false;
            if (_view.DataSource != null && _view.DataSource.Count > 0)
            {
                _view.SetToolStripState(false, "ETL Tool is Ready.");
                SetSaveState();
                _view.SetInitialJob(initialJob);
            }
        }

        public void Save(IJob job)
        {
            if (!job.Result.Success)
            {
                // Initial job failed - bale out
                _view.CloseApplication = true;
                _view.IsEtlToolLoading = false;
                _view.SetToolStripState(false, "Error");
                string message = "An error occured while running the 'Initial' job. No changes to the data sources will be saved.";
                message += "\r\n";
                message += "\r\n";
                message += "Please contact the Teleopti Support.";
                _view.ShowErrorMessage(message);
				_view.CloseView();
                return;
            }

            _view.SetToolStripState(true, "Saving changes...");
            saveUtcTimeZoneOnRaptorDataSource();
            saveDataSourcesWithTimeZoneSet();
            saveDataSourcesSetInactive();
            _view.IsSaved = true;
			_view.CloseView();
        }

        private void saveDataSourcesSetInactive()
        {
            foreach (DataSourceRow dataSourceRow in _view.DataSource)
            {
                if (dataSourceRow.Inactive && dataSourceRow.IsRowEnabled)
                {
                    _model.SaveDataSource(dataSourceRow);
                }
            }
        }

        private void saveDataSourcesWithTimeZoneSet()
        {
            // Get a list of all time zones recently saved in mart.dim_time_zone.
            // Loop through the list and map time zone id´s from the data mart.
            IList<ITimeZoneDim> martTimeZoneList = _model.TimeZonesFromMart;

            foreach (DataSourceRow dataSourceRow in _view.DataSource)
            {
                foreach (ITimeZoneDim timeZoneDim in martTimeZoneList)
                {
                    if (dataSourceRow.TimeZoneCode == timeZoneDim.TimeZoneCode)
                    {
                        // Set time zone id from data mart
                        dataSourceRow.TimeZoneId = timeZoneDim.MartId.ToString(CultureInfo.InvariantCulture);

                        // Persist time zone on data source in db
                        _model.SaveDataSource(dataSourceRow);
                    }
                }
            }
        }

        private void saveUtcTimeZoneOnRaptorDataSource()
        {
            _model.SaveUtcTimeZoneOnRaptorDataSource();
        }

        public void CancelView()
        {
            _view.IsSaved = false;
            _view.CloseView();
        }
    }
}
