using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Forecast;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Views;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Presenters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.WinCodeTest.Forecasting.ImportForecast
{
	[TestFixture]
	public class ImportForecastPresenterTest
	{
		private ImportForecastModel _model;
		private ImportForecastPresenter _target;
		private IImportForecastView _view;
		private ISkill _skill;
		private IValidateImportForecastFileCommand _validateImportForecastFileCommand;
		private ISaveImportForecastFileCommand _saveImportForecastFileCommand;
		private IUnitOfWorkFactory _uowFactory;
		private IJobResultRepository _jobResultRep;
		private IMessagePopulatingServiceBusSender _messageSender;
		private IToggleManager _toggleMan;
		private IPostHttpRequest _httpPost;

		[SetUp]
		public void Setup()
		{
			_skill = SkillFactory.CreateSkillWithWorkloadAndSources();
			_view = MockRepository.GenerateMock<IImportForecastView>();
			_model = new ImportForecastModel();
			_validateImportForecastFileCommand = MockRepository.GenerateMock<IValidateImportForecastFileCommand>();
			_saveImportForecastFileCommand = MockRepository.GenerateMock<ISaveImportForecastFileCommand>();
			_uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_jobResultRep = MockRepository.GenerateMock<IJobResultRepository>();
			_messageSender = MockRepository.GenerateMock<IMessagePopulatingServiceBusSender>();
			_toggleMan = MockRepository.GenerateMock<IToggleManager>();
			_httpPost = MockRepository.GenerateMock<IPostHttpRequest>();
			_target = new ImportForecastPresenter(_view, _model, _saveImportForecastFileCommand,
				_validateImportForecastFileCommand, _uowFactory, _jobResultRep, _messageSender,_httpPost, _toggleMan);
		}

		[Test]
		public void ShouldInitialize()
		{
			var workload = _skill.WorkloadCollection.FirstOrDefault();
			_model.SelectedSkill = _skill;
			
			_view.Stub(x => x.SetSkillName(_skill.Name));
			_view.Stub(x => x.SetWorkloadName(workload.Name));
			_view.Stub(x => x.SetVisibility(_skill.SkillType));
			
			_target.Initialize();
		}

		[Test]
		public void ShouldStartImport()
		{
			string fileName = "C:\\Test.csv";
			Guid jobId = Guid.NewGuid();
			_model.FileId = Guid.NewGuid();
			_model.HasValidationError = false;
			_model.SelectedSkill = _skill;
			var uow = MockRepository.GenerateMock<IUnitOfWork>();

			_validateImportForecastFileCommand.Stub(x => x.Execute(fileName));
			_saveImportForecastFileCommand.Stub(x => x.Execute(fileName));
			_uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			_jobResultRep.Stub(x => x.Add(null)).IgnoreArguments();
			uow.Stub(x => x.PersistAll());
			_messageSender.Stub(x => x.Send(null,true)).IgnoreArguments();
			_view.Stub(x => x.ShowStatusDialog(jobId));
			
			_target.StartImport(fileName);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void ShouldThrowInvalidOperationException()
		{
			_model.SelectedSkill = _skill;
			_view.Stub(x => x.SetSkillName(_skill.Name));
			_model.Stub(x => x.SelectedWorkload()).Return(null);
			
			_target.Initialize();
		}

		[Test]
		public void ShouldDetectValidationError()
		{
			string fileName = "test.csv";
			_model.FileId = Guid.NewGuid();
			_model.HasValidationError = true;
			_model.SelectedSkill = _skill;
			
			_validateImportForecastFileCommand.Stub(x => x.Execute(fileName));
			_view.Stub(x => x.ShowValidationException(null));
			
			_target.StartImport(fileName);
		}

		[Test]
		public void ShouldFailedWhenFileIdIsEmpty()
		{
			string fileName = "test.csv";
			_model.FileId = Guid.Empty;
			_model.HasValidationError = false;
			_model.SelectedSkill = _skill;

			_validateImportForecastFileCommand.Stub(x => x.Execute(fileName));
			_saveImportForecastFileCommand.Stub(x => x.Execute(fileName));
			_view.Stub(x => x.ShowError(null)).IgnoreArguments();

			_target.StartImport(fileName);
		}

		[Test]
		public void ShouldSetImportType()
		{
			_target.SetImportType(ImportForecastsMode.ImportWorkloadAndStaffing);
			Assert.IsNotNull(_model.ImportMode);
		}

		[Test]
		public void ShouldSendToJobMangerIfToggleOn()
		{
			string fileName = "C:\\Test.csv";
			Guid jobId = Guid.NewGuid();
			_model.FileId = Guid.NewGuid();
			_model.HasValidationError = false;
			_model.SelectedSkill = _skill;
			var uow = MockRepository.GenerateMock<IUnitOfWork>();

			_validateImportForecastFileCommand.Stub(x => x.Execute(fileName));
			_saveImportForecastFileCommand.Stub(x => x.Execute(fileName));
			_uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(uow);
			_jobResultRep.Stub(x => x.Add(null)).IgnoreArguments();
			uow.Stub(x => x.PersistAll());

			_toggleMan.Stub(x => x.IsEnabled(Toggles.Wfm_UseManagersAndNodes)).Return(true);
			_httpPost.Stub(x => x.Send<Guid>("", "")).IgnoreArguments().Return(Guid.NewGuid());
			_messageSender.AssertWasNotCalled((x => x.Send(null, true)));

			_view.Stub(x => x.ShowStatusDialog(jobId));

			_target.StartImport(fileName);
		}
	}
}
