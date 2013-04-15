﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Payroll;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Payroll
{
    [TestFixture]
    public class PayrollExportConsumerTest
    {
        private PayrollExportConsumer target;
        readonly MockRepository mock = new MockRepository();
        private IRepositoryFactory repositoryFactory;
				private ICurrentUnitOfWorkFactory unitOfWorkFactory;
        private IUnitOfWork unitOfWork;
        private IPayrollExportRepository payrollExportRepository;
        private IPayrollPeopleLoader payrollPeopleLoader;
        private IPerson exportingPerson;
        private IPayrollDataExtractor payrollDataExtractor;
        private IPersonBusAssembler personBusAssembler;
        private IPayrollResultRepository payrollResultRepository;
        private IServiceBusPayrollExportFeedback serviceBusReportProgress;
    	private IDomainAssemblyResolver _resolver;

    	[SetUp]
        public void Setup()
        {
            repositoryFactory = mock.StrictMock<IRepositoryFactory>();
            payrollExportRepository = mock.StrictMock<IPayrollExportRepository>();
            payrollPeopleLoader = mock.StrictMock<IPayrollPeopleLoader>();
            payrollResultRepository = mock.StrictMock<IPayrollResultRepository>();
						unitOfWorkFactory = mock.StrictMock<ICurrentUnitOfWorkFactory>();
            unitOfWork = mock.StrictMock<IUnitOfWork>();
            payrollDataExtractor = mock.StrictMock<IPayrollDataExtractor>();
            personBusAssembler = mock.StrictMock<IPersonBusAssembler>();
            serviceBusReportProgress = mock.StrictMock<IServiceBusPayrollExportFeedback>();
        	_resolver = mock.DynamicMock<IDomainAssemblyResolver>();
            exportingPerson = new Person{Name = new Name("Ex", "Porter")};
			target = new PayrollExportConsumer(repositoryFactory, payrollDataExtractor, personBusAssembler, serviceBusReportProgress, payrollPeopleLoader, _resolver);
        }

        [Test] //Dont know what I am testing here anymore
        public void ShouldConsumePayrollExport()
        {
            setupDataSource();
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
            
            var persons = new Collection<IPerson> {PersonFactory.CreatePerson(), PersonFactory.CreatePerson()};
            var assembler = new PersonBusAssembler();
            var personDtos = assembler.CreatePersonDto(persons);
            
            var document = new XmlDocument();
            document.AppendChild(document.CreateElement("stupid_document_for_stupid_test"));

            using (mock.Record())
            {
                prepareUnitOfWork(1, false);
                Expect.Call(payrollPeopleLoader.GetPeopleForExport(exportMessage, new DateOnlyPeriod(), unitOfWork)).Return(persons).IgnoreArguments();
                Expect.Call(repositoryFactory.CreatePayrollExportRepository(unitOfWork)).Return(payrollExportRepository);
                Expect.Call(payrollExportRepository.Get(payrollGuid)).Return(payrollExport);
                Expect.Call(personBusAssembler.CreatePersonDto(persons)).Return(personDtos);
                Expect.Call(payrollDataExtractor.Extract(payrollExport, exportMessage, personDtos, serviceBusReportProgress)).Return(document);
                Expect.Call(repositoryFactory.CreatePayrollResultRepository(unitOfWork)).Return(payrollResultRepository);
                Expect.Call(payrollResultRepository.Get(resultId)).Return(payrollResult);
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(()=>serviceBusReportProgress.SetPayrollResult(null)).IgnoreArguments();
                Expect.Call(() => serviceBusReportProgress.ReportProgress(0, string.Empty)).IgnoreArguments().Repeat.
                    Twice();
                Expect.Call(serviceBusReportProgress.Dispose);
            }
            using (mock.Playback())
            {
                target.Consume(exportMessage);
                Assert.IsNotNull(payrollResult.XmlResult);
            }
        }

        [Test]
        public void ShouldLogErrorWhenRunningExport()
        {
            setupDataSource();
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
                prepareUnitOfWork(1, false);
                Expect.Call(payrollPeopleLoader.GetPeopleForExport(exportMessage, new DateOnlyPeriod(), unitOfWork)).Return(new List<IPerson>()).IgnoreArguments();
                Expect.Call(repositoryFactory.CreatePayrollExportRepository(unitOfWork)).Return(payrollExportRepository);
                Expect.Call(payrollExportRepository.Get(payrollGuid)).Return(payrollExport);
                Expect.Call(personBusAssembler.CreatePersonDto(new List<IPerson>())).Return(new List<PersonDto>());
                Expect.Call(payrollDataExtractor.Extract(payrollExport, exportMessage, new List<PersonDto>(), serviceBusReportProgress)).Throw(new Exception("For test"));
                Expect.Call(repositoryFactory.CreatePayrollResultRepository(unitOfWork)).Return(payrollResultRepository);
                Expect.Call(payrollResultRepository.Get(resultId)).Return(payrollResult);
                Expect.Call(() => unitOfWork.PersistAll());
                Expect.Call(() => serviceBusReportProgress.SetPayrollResult(null)).IgnoreArguments();
                Expect.Call(() => serviceBusReportProgress.ReportProgress(0, string.Empty)).IgnoreArguments().Repeat.
                    Twice();
                Expect.Call(() => serviceBusReportProgress.Error(null, null)).IgnoreArguments();
                Expect.Call(serviceBusReportProgress.Dispose);
            }
            using (mock.Playback())
            {
                target.Consume(exportMessage);
            }
        }

        [Test]
        public void ShouldNotConsumePayrollExportIfNull()
        {
            target.Consume(null);
        }

        private void setupDataSource()
        {
            UnitOfWorkFactoryContainer.Current = unitOfWorkFactory;
        }

        private void prepareUnitOfWork(int times, bool persistAll)
        {
	        var uowFactory = mock.DynamicMock<IUnitOfWorkFactory>();
	        Expect.Call(unitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
            Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork).Repeat.Times(times);
            Expect.Call(unitOfWork.Dispose).Repeat.Times(times);
            if (persistAll)
            {
                Expect.Call(unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>()).Repeat.Times(times);
            }
        }


        private static RunPayrollExport GetExportMessage(Guid payrollGuid, Guid buGuid, Guid ownerGuid, Guid resultId)
        {
            return new RunPayrollExport
            {
                BusinessUnitId = buGuid,
                Datasource = "DS",
                OwnerPersonId = ownerGuid,
                PayrollExportId = payrollGuid,
                PayrollResultId = resultId,
                Timestamp = new DateTime(2009, 12, 12, 0, 0, 0, DateTimeKind.Utc)
            };
        }
    }
}
