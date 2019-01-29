using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using DateOnlyPeriod = Teleopti.Interfaces.Domain.DateOnlyPeriod;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.PayrollTest
{
	[TestFixture]
	public class PayrollExportConsumerTest
	{
		private PayrollExportHandler target;
		readonly MockRepository mock = new MockRepository();
		private ICurrentUnitOfWork currentUnitOfWork;
		private IUnitOfWork unitOfWork;
		private IPayrollExportRepository payrollExportRepository;
		private IPayrollPeopleLoader payrollPeopleLoader;
		private IPerson exportingPerson;
		private IPayrollDataExtractor payrollDataExtractor;
		private IPersonBusAssembler personBusAssembler;
		private IPayrollResultRepository payrollResultRepository;
		private IServiceBusPayrollExportFeedback serviceBusReportProgress;
		private IDomainAssemblyResolver _resolver;
		private ITenantPeopleLoader _tenantPeopleLoader;
		private IBusinessUnit _currentBusinessUnit;
		private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private IUnitOfWorkFactory unitOfWorkFactory;
	  private FakeTenantLogonDataManager TenantLogonDataManager;
		[SetUp]
		public void Setup()
		{
			TenantLogonDataManager = new FakeTenantLogonDataManager();

			payrollExportRepository = mock.StrictMock<IPayrollExportRepository>();
			payrollPeopleLoader = mock.StrictMock<IPayrollPeopleLoader>();
			payrollResultRepository = mock.StrictMock<IPayrollResultRepository>();
			currentUnitOfWork = mock.StrictMock<ICurrentUnitOfWork>();
			unitOfWork = mock.StrictMock<IUnitOfWork>();
			payrollDataExtractor = mock.StrictMock<IPayrollDataExtractor>();
			personBusAssembler = mock.StrictMock<IPersonBusAssembler>();
			serviceBusReportProgress = mock.StrictMock<IServiceBusPayrollExportFeedback>();
			_resolver = mock.DynamicMock<IDomainAssemblyResolver>();
			_tenantPeopleLoader = new TenantPeopleLoader(TenantLogonDataManager);
		
			_currentUnitOfWorkFactory = mock.DynamicMock<ICurrentUnitOfWorkFactory>();
			unitOfWorkFactory = mock.DynamicMock<IUnitOfWorkFactory>();
			exportingPerson = new Person().WithName(new Name("Ex", "Porter"));
			_currentBusinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit().WithId();
			var fakeBuRepo = new FakeBusinessUnitRepository(null);

			fakeBuRepo.Has(_currentBusinessUnit);
			target = new PayrollExportHandler(currentUnitOfWork, payrollExportRepository, payrollResultRepository,
				payrollDataExtractor, personBusAssembler, serviceBusReportProgress, payrollPeopleLoader, _resolver,
				_tenantPeopleLoader, new FakeStardustJobFeedback(), new FakeCurrentBusinessUnit(), fakeBuRepo, _currentUnitOfWorkFactory);
		}

		[Test] 
		public void ShouldConsumePayrollExport()
		{
			var pers1 = PersonFactory.CreatePerson().WithId();
			var pers2 = PersonFactory.CreatePerson().WithId();
			var persons = new Collection<IPerson> { pers1, pers2 };
			TenantLogonDataManager.SetLogon(pers1.Id.GetValueOrDefault(), "NGT", "TILL");
			TenantLogonDataManager.SetLogon(pers2.Id.GetValueOrDefault(), "NGTAnnat", "också");

			var payrollGuid = Guid.NewGuid();
			var ownerGuid = Guid.NewGuid();
			var buGuid = Guid.NewGuid();
			var formatId = Guid.NewGuid();
			var resultId = Guid.NewGuid();

			IPayrollExport payrollExport = new PayrollExport();
			payrollExport.SetId(payrollGuid);
			payrollExport.PayrollFormatId = formatId;

			IPayrollResult payrollResult = new PayrollResult(payrollExport, exportingPerson, DateTime.UtcNow);
			payrollResult.SetId(resultId);

			var exportMessage = GetExportMessage(payrollGuid, buGuid, ownerGuid, resultId);
			exportMessage.LogOnBusinessUnitId = _currentBusinessUnit.Id.GetValueOrDefault();
			
			var assembler = new PersonBusAssembler();
			var personDtos = assembler.CreatePersonDto(persons, _tenantPeopleLoader).ToList();

			var document = new XmlDocument();
			document.AppendChild(document.CreateElement("stupid_document_for_stupid_test"));

			using (mock.Record())
			{
				Expect.Call(_currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).IgnoreArguments().Return(unitOfWork);
				prepareUnitOfWork();
				Expect.Call(payrollPeopleLoader.GetPeopleForExport(exportMessage, new DateOnlyPeriod(), unitOfWork)).Return(persons).IgnoreArguments();
				Expect.Call(payrollExportRepository.Get(payrollGuid)).Return(payrollExport);
				Expect.Call(personBusAssembler.CreatePersonDto(persons, _tenantPeopleLoader)).Return(personDtos);
				Expect.Call(payrollDataExtractor.Extract(payrollExport, exportMessage, personDtos, serviceBusReportProgress)).Return(document);
				Expect.Call(payrollResultRepository.Get(resultId)).Return(payrollResult);
				Expect.Call(() => serviceBusReportProgress.SetPayrollResult(null)).IgnoreArguments();
				Expect.Call(() => serviceBusReportProgress.ReportProgress(0, string.Empty)).IgnoreArguments().Repeat.
					 Once();
				Expect.Call(serviceBusReportProgress.Dispose);

				Expect.Call(unitOfWork.PersistAll()).IgnoreArguments();
				Expect.Call(() => unitOfWork.Dispose());
			}
			using (mock.Playback())
			{
				target.Handle(exportMessage);
				Assert.IsNotNull(payrollResult.XmlResult);
			}
		}

		private void prepareUnitOfWork()
		{
			Expect.Call(currentUnitOfWork.Current()).Return(unitOfWork);
		}


		private static RunPayrollExportEvent GetExportMessage(Guid payrollGuid, Guid buGuid, Guid ownerGuid, Guid resultId)
		{
			return new RunPayrollExportEvent
			{
				LogOnBusinessUnitId = buGuid,
				LogOnDatasource = "DS",
				OwnerPersonId = ownerGuid,
				PayrollExportId = payrollGuid,
				PayrollResultId = resultId,
				Timestamp = new DateTime(2009, 12, 12, 0, 0, 0, DateTimeKind.Utc)
			};
		}
	}
}
