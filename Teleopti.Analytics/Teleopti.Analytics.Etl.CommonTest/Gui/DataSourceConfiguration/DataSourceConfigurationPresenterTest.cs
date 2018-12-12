using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Configuration;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.ConfigTool.Code.Gui.DataSourceConfiguration;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.CommonTest.Gui.DataSourceConfiguration
{
	[TestFixture]
	public class DataSourceConfigurationPresenterTest
	{
		private DataSourceConfigurationPresenter _target;
		private IDataSourceConfigurationView _view;
		private DataSourceConfigurationModel _model;
		private IGeneralFunctions _generalFunctions;

		[SetUp]
		public void Setup()
		{
			_view = MockRepository.GenerateMock<IDataSourceConfigurationView>();
			_generalFunctions = MockRepository.GenerateMock<IGeneralFunctions>();

			_model = new DataSourceConfigurationModel(_generalFunctions, new BaseConfiguration(null, 15, "W. Europe Standard Time", false));
			_target = new DataSourceConfigurationPresenter(_view, _model);
		}

		[Test]
		public void ShouldInitializeViewState()
		{
			IDataSourceEtl etlDataSource = new DataSourceEtl(1, "name", 2, "UTC", 15, false);

			_generalFunctions.Stub(x => x.DataSourceInvalidList).Return(new List<IDataSourceEtl>());
			_generalFunctions.Stub(x => x.DataSourceValidList).Return(new List<IDataSourceEtl> { etlDataSource });
			_view.Stub(x => x.SetTimeZoneDataSource(_model.LoadTimeZones()));

			_target.Initialize();

			_view.AssertWasCalled(x => x.SetOkButtonEnabled(false));
			_view.Expect(x => x.DataSourceStatusColumnHeader);
			_view.Expect(x => x.DataSourceNameColumnHeader);
			_view.Expect(x => x.DataSourceTimeZoneColumnHeader);
			_view.Expect(x => x.DataSourceIntervalLengthColumnHeader);
			_view.Expect(x => x.DataSourceInactiveColumnHeader);

		}

		[Test]
		public void ShouldSetGridStateWhenNoDataSourcesAreAvailable()
		{
			_view.Stub(x => x.DataSource).Return(new List<DataSourceRow>());

			_target.SetGridState();

			_view.AssertWasCalled(x => x.SetToolStripState(false, "No data sources available! Data source dependent jobs cannot be executed."));
		}

		[Test]
		public void ShouldSetRowStateForValidDataSource()
		{
			IDataSourceEtl etlDataSource = new DataSourceEtl(1, "name", 2, "UTC", 15, false);
			var dataSource = new DataSourceRow(etlDataSource, 0);

			_view.Stub(x => x.DataSource).Return(new List<DataSourceRow> { dataSource });
			_view.Stub(x => x.IsEtlToolLoading).Return(false);

			_target.SetGridState();

			_view.AssertWasCalled(x => x.SetToolStripState(false, string.Empty));
			_view.AssertWasCalled(x => x.SetTimeZoneComboState(0, true));
			_view.AssertWasCalled(x => x.SetRowStateImage(dataSource));
			_view.AssertWasCalled(x => x.SetRowReadOnly(dataSource));

			dataSource.RowState.Should().Be.EqualTo(DataSourceState.Valid);
			dataSource.RowStateToolTip.Should().Be.EqualTo("Data source is valid.");
		}

		[Test]
		public void ShouldSetRowStateForInactiveDataSource()
		{
			IDataSourceEtl etlDataSource = new DataSourceEtl(1, "name", -1, string.Empty, 15, true);
			var dataSource = new DataSourceRow(etlDataSource, 0);

			_view.Stub(x => x.DataSource).Return(new List<DataSourceRow> { dataSource });
			_view.Stub(x => x.IsEtlToolLoading).Return(true);

			_target.SetGridState();

			_view.AssertWasCalled(x => x.SetToolStripState(true, "ETL Tool loading..."));
			_view.AssertWasCalled(x => x.SetTimeZoneSelected(dataSource));
			_view.AssertWasCalled(x => x.SetTimeZoneComboState(0, false));
			_view.AssertWasCalled(x => x.SetRowStateImage(dataSource));
			_view.AssertWasCalled(x => x.SetRowReadOnly(dataSource));

			dataSource.RowState.Should().Be.EqualTo(DataSourceState.Invalid);
			dataSource.RowStateToolTip.Should().Be.EqualTo("WARNING! When a data source are set to Inactive it will be supressed in the future and not used by the system.");
			dataSource.TimeZoneId.Should().Be.EqualTo("-1");
			dataSource.TimeZoneCode.Should().Be.EqualTo(string.Empty);
		}
		[Test]
		public void ShouldSetRowStateForInvalidIntervalLengthOnDataSource()
		{
			IDataSourceEtl etlDataSource = new DataSourceEtl(1, "name", 1, "UTC", 10, true);
			var dataSource = new DataSourceRow(etlDataSource, 0);

			_view.Stub(x => x.DataSource).Return(new List<DataSourceRow> { dataSource });
			_view.Stub(x => x.IsEtlToolLoading).Return(false);

			_target.SetGridState();

			_view.AssertWasCalled(x => x.SetOkButtonEnabled(false));
			_view.AssertWasCalled(x => x.SetTimeZoneComboState(0, true));
			_view.AssertWasCalled(x => x.SetRowStateImage(dataSource));
			_view.AssertWasCalled(x => x.SetRowReadOnly(dataSource));

			dataSource.RowState.Should().Be.EqualTo(DataSourceState.Error);
			dataSource.RowStateToolTip.Should().Contain("IMPORTANT! Invalid interval length.");
		}

		[Test]
		public void ShouldSetRowStateForDataSourceWithoutTimeZoneAssigned()
		{
			IDataSourceEtl etlDataSource = new DataSourceEtl(1, "name", -1, string.Empty, 15, false);
			var dataSource = new DataSourceRow(etlDataSource, 0);

			_view.Stub(x => x.DataSource).Return(new List<DataSourceRow> { dataSource });

			_target.SetGridState();

			_view.AssertWasCalled(x => x.SetTimeZoneComboState(0, true));
			_view.AssertWasCalled(x => x.SetRowStateImage(dataSource));
			_view.AssertWasCalled(x => x.SetRowReadOnly(dataSource));

			dataSource.RowState.Should().Be.EqualTo(DataSourceState.Invalid);
			dataSource.RowStateToolTip.Should().Be.EqualTo("WARNING! Time zone not assigned. This data source can not be used by the system.");
		}

		[Test]
		public void ShouldEnableSaveState()
		{
			_view.Stub(x => x.IsEtlToolLoading).Return(false);
			_target.SetSaveState();
			_view.AssertWasCalled(x => x.SetOkButtonEnabled(Arg<bool>.Is.Equal(true)));
		}

		[Test]
		public void ShouldDisableSaveState()
		{
			IDataSourceEtl etlDataSource = new DataSourceEtl(1, "name", -1, string.Empty, 10, false);
			var dataSource = new DataSourceRow(etlDataSource, 0);

			_view.Stub(x => x.DataSource).Return(new List<DataSourceRow> { dataSource });
			_view.Stub(x => x.IsEtlToolLoading).Return(false);

			_target.SetSaveState();

			_view.AssertWasCalled(x => x.SetOkButtonEnabled(Arg<bool>.Is.Equal(false)));
		}

		[Test]
		public void ShouldCloseViewWhenNoDataSourceToSave()
		{
			_view.Stub(x => x.DataSource).Return(new List<DataSourceRow>());
			_target.InitiateSave(null);
			_view.AssertWasCalled(x => x.CloseView());
		}

		[Test]
		public void ShouldStartInitialLoadJobIfNewTimeZoneIsSet()
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			IDataSourceEtl etlDataSource = new DataSourceEtl(1, "name", -1, timeZone.Id, 15, false);
			var dataSource = new DataSourceRow(etlDataSource, 0);
			var job = MockRepository.GenerateMock<IJob>();
			var jobParameters = MockRepository.GenerateMock<IJobParameters>();

			_view.Stub(x => x.DataSource).Return(new List<DataSourceRow> { dataSource });
			job.Stub(x => x.StepList).Return(new List<IJobStep> { null, null, null, new StageTimeZoneJobStep(jobParameters) });
			jobParameters.Stub(x => x.JobCategoryDates).Return(new JobMultipleDate(timeZone));

			_target.InitiateSave(job);

			_view.AssertWasCalled(x => x.SetOkButtonEnabled(false));
			_view.AssertWasCalled(x => x.SetViewEnabled(false));
			_view.AssertWasCalled(x => x.SetToolStripState(true, "'Initial' job is running in the background. This can take several minutes..."));
			_view.AssertWasCalled(x => x.IsEtlToolLoading = true);
			_view.AssertWasCalled(x => x.RunInitialJob());

			jobParameters.JobCategoryDates.Count.Should().Be.EqualTo(1);
			var dateItem = jobParameters.JobCategoryDates.GetJobMultipleDateItem(JobCategoryType.Initial);
			dateItem.Should().Not.Be.Null();
			dateItem.StartDateLocal.Should().Be.EqualTo(DateTime.Today);
			dateItem.EndDateLocal.Should().Be.EqualTo(DateTime.Today.AddYears(1));
		}

		[Test]
		public void ShouldSaveDataSourcesSetToInactive()
		{
			IDataSourceEtl etlDataSource = new DataSourceEtl(1, "name", -1, string.Empty, 15, true);
			var dataSource = new DataSourceRow(etlDataSource, 0);
			var job = MockRepository.GenerateMock<IJob>();

			_view.Stub(x => x.DataSource).Return(new List<DataSourceRow> { dataSource });

			_target.InitiateSave(job);

			_generalFunctions.AssertWasCalled(x => x.SaveDataSource(1, -1));
			_view.AssertWasCalled(x => x.SetToolStripState(true, "Saving changes..."));
			_view.AssertWasCalled(x => x.IsSaved = false);
			_view.AssertWasCalled(x => x.CloseView());

		}

		[Test]
		public void ShouldSetViewReadyToUseIfDataSourcesAvailable()
		{
			IDataSourceEtl etlDataSource = new DataSourceEtl(1, "name", 2, "W. Europe Standard Time", 15, false);
			var dataSource = new DataSourceRow(etlDataSource, 0);

			_view.Stub(x => x.DataSource).Return(new List<DataSourceRow> { dataSource });
			_view.Stub(x => x.IsEtlToolLoading).Return(true);

			_target.SetViewReadyToSave(null);

			_view.AssertWasCalled(x => x.IsEtlToolLoading = false);
			_view.AssertWasCalled(x => x.SetToolStripState(false, "ETL Tool is Ready."));
			_view.AssertWasCalled(x => x.SetInitialJob(null));
		}

		[Test]
		public void ShouldSaveWhenInitialJobSucceeded()
		{
			IDataSourceEtl etlDataSource = new DataSourceEtl(1, "name", -1, "UTC", 15, false);
			var dataSourceRow = new DataSourceRow(etlDataSource, 0);
			var timeZoneDim = new TimeZoneDim(1, "UTC", "UTC name", false, 0, 0, false);

			var initialJob = MockRepository.GenerateMock<IJob>();
			var result = MockRepository.GenerateMock<IJobResult>();

			initialJob.Stub(x => x.Result).Return(result);
			result.Stub(x => x.Success).Return(true);
			_generalFunctions.Stub(x => x.GetTimeZoneList()).Return(new List<ITimeZoneDim> { timeZoneDim });
			_view.Stub(x => x.DataSource).Return(new List<DataSourceRow> { dataSourceRow });

			_target.Save(initialJob);

			_view.AssertWasCalled(x => x.SetToolStripState(true, "Saving changes..."));
			_generalFunctions.AssertWasCalled(x => x.SetUtcTimeZoneOnRaptorDataSource());
			_generalFunctions.AssertWasCalled(x => x.SaveDataSource(1, 1));
			_view.AssertWasCalled(x => x.IsSaved = true);
		}

		[Test]
		public void ShouldNotSaveWhenInitialJobFailed()
		{
			var initialJob = MockRepository.GenerateMock<IJob>();
			var result = MockRepository.GenerateMock<IJobResult>();

			initialJob.Stub(x => x.Result).Return(result);
			result.Stub(x => x.Success).Return(false);

			_target.Save(initialJob);

			_view.AssertWasCalled(x => x.CloseApplication = true);
			_view.AssertWasCalled(x => x.IsEtlToolLoading = false);
			_view.AssertWasCalled(x => x.SetToolStripState(false, "Error"));
			_view.AssertWasCalled(
				x =>
				x.ShowErrorMessage(
					Arg<string>.Matches(
						y => y.Contains("An error occured while running the 'Initial' job. No changes to the data sources will be saved."))));
		}

		[Test]
		public void ShouldCloseViewWithoutSave()
		{
			_target.CancelView();

			_view.AssertWasCalled(x => x.IsSaved = false);
			_view.AssertWasCalled(x => x.CloseView());
		}
	}
}