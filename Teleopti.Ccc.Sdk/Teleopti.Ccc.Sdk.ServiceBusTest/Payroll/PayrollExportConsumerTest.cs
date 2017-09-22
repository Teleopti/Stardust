using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Payroll
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

		[SetUp]
		public void Setup()
		{
			payrollExportRepository = mock.StrictMock<IPayrollExportRepository>();
			payrollPeopleLoader = mock.StrictMock<IPayrollPeopleLoader>();
			payrollResultRepository = mock.StrictMock<IPayrollResultRepository>();
			currentUnitOfWork = mock.StrictMock<ICurrentUnitOfWork>();
			unitOfWork = mock.StrictMock<IUnitOfWork>();
			payrollDataExtractor = mock.StrictMock<IPayrollDataExtractor>();
			personBusAssembler = mock.StrictMock<IPersonBusAssembler>();
			serviceBusReportProgress = mock.StrictMock<IServiceBusPayrollExportFeedback>();
			_resolver = mock.DynamicMock<IDomainAssemblyResolver>();
			_tenantPeopleLoader = mock.DynamicMock<ITenantPeopleLoader>();
			exportingPerson = new Person().WithName(new Name("Ex", "Porter"));
			target = new PayrollExportHandler(currentUnitOfWork, payrollExportRepository, payrollResultRepository,
				payrollDataExtractor, personBusAssembler, serviceBusReportProgress, payrollPeopleLoader, _resolver,
				_tenantPeopleLoader);
		}

		[Test] //Dont know what I am testing here anymore
		public void ShouldConsumePayrollExport()
		{
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

			var persons = new Collection<IPerson> { PersonFactory.CreatePerson(), PersonFactory.CreatePerson() };
			var assembler = new PersonBusAssembler();
			var personDtos = assembler.CreatePersonDto(persons, _tenantPeopleLoader);

			var document = new XmlDocument();
			document.AppendChild(document.CreateElement("stupid_document_for_stupid_test"));

			using (mock.Record())
			{
				prepareUnitOfWork();
				Expect.Call(payrollPeopleLoader.GetPeopleForExport(exportMessage, new DateOnlyPeriod(), unitOfWork)).Return(persons).IgnoreArguments();
				Expect.Call(payrollExportRepository.Get(payrollGuid)).Return(payrollExport);
				Expect.Call(personBusAssembler.CreatePersonDto(persons, _tenantPeopleLoader)).Return(personDtos);
				Expect.Call(payrollDataExtractor.Extract(payrollExport, exportMessage, personDtos, serviceBusReportProgress)).Return(document);
				Expect.Call(payrollResultRepository.Get(resultId)).Return(payrollResult);
				Expect.Call(() => serviceBusReportProgress.SetPayrollResult(null)).IgnoreArguments();
				Expect.Call(() => serviceBusReportProgress.ReportProgress(0, string.Empty)).IgnoreArguments().Repeat.
					 Twice();
				Expect.Call(serviceBusReportProgress.Dispose);
			}
			using (mock.Playback())
			{
				target.Handle(exportMessage);
				Assert.IsNotNull(payrollResult.XmlResult);
			}
		}

		[Test]
		public void ShouldLogErrorWhenRunningExport()
		{
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

			using (mock.Record())
			{
				prepareUnitOfWork();
				Expect.Call(payrollPeopleLoader.GetPeopleForExport(exportMessage, new DateOnlyPeriod(), unitOfWork)).Return(new List<IPerson>()).IgnoreArguments();
				Expect.Call(payrollExportRepository.Get(payrollGuid)).Return(payrollExport);
				Expect.Call(personBusAssembler.CreatePersonDto(new List<IPerson>(), _tenantPeopleLoader)).Return(new List<PersonDto>());
				Expect.Call(payrollDataExtractor.Extract(payrollExport, exportMessage, new List<PersonDto>(), serviceBusReportProgress)).Throw(new Exception("For test"));
				Expect.Call(payrollResultRepository.Get(resultId)).Return(payrollResult);
				Expect.Call(() => serviceBusReportProgress.SetPayrollResult(null)).IgnoreArguments();
				Expect.Call(() => serviceBusReportProgress.ReportProgress(0, string.Empty)).IgnoreArguments().Repeat.
					 Twice();
				Expect.Call(() => serviceBusReportProgress.Error(null, null)).IgnoreArguments();
				Expect.Call(serviceBusReportProgress.Dispose);
			}
			using (mock.Playback())
			{
				target.Handle(exportMessage);
			}
		}

		[Test]
		public void ShouldNotConsumePayrollExportIfNull()
		{
			target.Handle(null);
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
