using System;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class ImportForecastsFileCommandHandlerTest
	{
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IJobResultRepository _jobResultRepository;
		private ImportForecastsFileCommandHandler _target;
		private IPerson _person;
		private ISkill _targetSkill;
		private Guid _fileId;
		private ImportForecastsFileCommandDto _importForecastsFileCommandDto;
		private IJobResult _jobResult;
		private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private ICurrentBusinessUnit _currentBu;
		private IStardustSender _stardustSender;

		[SetUp]
		public void Setup()
		{
			_unitOfWorkFactory = MockRepository.GenerateStrictMock<IUnitOfWorkFactory>();
			_currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			_jobResultRepository = MockRepository.GenerateStrictMock<IJobResultRepository>();
			_currentBu = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			_stardustSender = MockRepository.GenerateMock<IStardustSender>();
			_target = new ImportForecastsFileCommandHandler(_currentUnitOfWorkFactory, _jobResultRepository, _currentBu, _stardustSender);

			_person = PersonFactory.CreatePerson("test").WithId();
			_targetSkill = SkillFactory.CreateSkill("Test Skills").WithId();

			_fileId = Guid.NewGuid();
			_importForecastsFileCommandDto = new ImportForecastsFileCommandDto
			{
				ImportForecastsMode =
					ImportForecastsOptionsDto.ImportWorkloadAndStaffing,
				TargetSkillId = _targetSkill.Id.GetValueOrDefault(),
				UploadedFileId = _fileId
			};
			_jobResult = new JobResult(JobCategory.ForecastsImport, new DateOnlyPeriod(DateOnly.Today, DateOnly.Today),
				_person, DateTime.UtcNow);
		}

		[Test]
		public void ShouldThrowFaultExceptionIfCommandIsNull()
		{
			Assert.Throws<FaultException>(() => _target.Handle(null));
		}

		[Test]
		public void ShouldHandleImportForecastFileCommandSuccessfully()
		{
			var unitOfWork = MockRepository.GenerateStrictMock<IUnitOfWork>();

			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			_currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory).Repeat.Twice();
			_currentBu.Stub(x => x.Current()).Return(new BusinessUnit("BU"));
			_unitOfWorkFactory.Stub(x => x.Name).Return("WFM");
			_jobResultRepository.Stub(x => x.Add(_jobResult)).IgnoreArguments();
			unitOfWork.Stub(x => x.PersistAll());
			unitOfWork.Stub(x => x.Dispose());

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				_target.Handle(_importForecastsFileCommandDto);
			}
		}
		
		[Test]
		public void ShouldThrowExceptionIfNotPermitted()
		{
			using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
			{
				Assert.Throws<FaultException>(() => _target.Handle(_importForecastsFileCommandDto));
			}
		}
	}
}
