using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ImportForecast.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ImportForecast.Presenters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.ImportForecast.Views;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
		private IEventPublisher _publisher;

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
			_publisher = MockRepository.GenerateMock<IEventPublisher>();
			_target = new ImportForecastPresenter(_view, _model, _saveImportForecastFileCommand,
				_validateImportForecastFileCommand, _uowFactory, _jobResultRep, _publisher);
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
			_publisher.Stub(x => x.Publish(null)).IgnoreArguments();
			_view.Stub(x => x.ShowStatusDialog(jobId));

			_target.StartImport(fileName);
		}

		[Test]
		public void ShouldThrowInvalidOperationException()
		{
			_model.SelectedSkill = _skill;
			_view.Stub(x => x.SetSkillName(_skill.Name));
			Assert.Throws<InvalidOperationException>(() =>
			{
				_model.Stub(x => x.SelectedWorkload()).Return(null);
				_target.Initialize();
			});
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
		public void ShouldSendToJobManagerIfToggleOn()
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

			_publisher.Stub(x => x.Publish(null)).IgnoreArguments();

			_view.Stub(x => x.ShowStatusDialog(jobId));

			_target.StartImport(fileName);
		}
	}
}
