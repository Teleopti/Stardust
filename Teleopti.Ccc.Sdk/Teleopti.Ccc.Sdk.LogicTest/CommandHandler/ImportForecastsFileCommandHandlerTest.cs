using System;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Client;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class ImportForecastsFileCommandHandlerTest
	{
		private IMessagePopulatingServiceBusSender _busSender;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IJobResultRepository _jobResultRepository;
		private ImportForecastsFileCommandHandler _target;
		private IPerson _person;
		private ISkill _targetSkill;
		private Guid _fileId;
		private ImportForecastsFileCommandDto _importForecastsFileCommandDto;
		private IJobResult _jobResult;
		private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private IPostHttpRequest _postHttpRequest;
		private IToggleManager _toggleManager;
		private IJsonSerializer _jsonSer;
		private ICurrentBusinessUnit _currentBu;

		[SetUp]
		public void Setup()
		{
			_busSender = MockRepository.GenerateStrictMock<IMessagePopulatingServiceBusSender>();
			_unitOfWorkFactory = MockRepository.GenerateStrictMock<IUnitOfWorkFactory>();
			_currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			_jobResultRepository = MockRepository.GenerateStrictMock<IJobResultRepository>();
			_postHttpRequest = MockRepository.GenerateMock<IPostHttpRequest>();
			_toggleManager = MockRepository.GenerateMock<IToggleManager>();
			_jsonSer = MockRepository.GenerateMock<IJsonSerializer>();
			_currentBu = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			_target = new ImportForecastsFileCommandHandler(_busSender, _currentUnitOfWorkFactory, _jobResultRepository,
				_postHttpRequest, _toggleManager, _jsonSer, _currentBu);

			_person = PersonFactory.CreatePerson("test");
			_person.SetId(Guid.NewGuid());

			_targetSkill = SkillFactory.CreateSkill("Test Skills");
			_targetSkill.SetId(Guid.NewGuid());

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
		[ExpectedException(typeof (FaultException))]
		public void ShouldThrowFaultExceptionIfCommandIsNull()
		{
			_target.Handle(null);
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
			_busSender.Stub(x => x.Send(new ImportForecastsFileToSkill(), true)).IgnoreArguments();

			_target.Handle(_importForecastsFileCommandDto);
		}
		
		[Test]
		[ExpectedException(typeof (FaultException))]
		public void ShouldThrowExceptionIfNotPermitted()
		{
			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
			{
				_target.Handle(_importForecastsFileCommandDto);
			}
		}
	}
}
