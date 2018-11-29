using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Payroll;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll;
using Teleopti.Ccc.TestCommon.FakeData;
using DateOnlyPeriod = Teleopti.Interfaces.Domain.DateOnlyPeriod;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.PayrollTest
{
    [TestFixture]
    public class PayrollPeopleLoaderTest
    {
        private MockRepository mocks;
        private IRepositoryFactory repositoryFactory;
        private IPersonRepository personRepository;
        private IUnitOfWork unitOfWork;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            repositoryFactory = mocks.StrictMock<IRepositoryFactory>();
            personRepository = mocks.StrictMock<IPersonRepository>();
            unitOfWork = mocks.StrictMock<IUnitOfWork>();
        }

        [Test]
        public void ShouldGetOwningPerson()
        {
            IPerson person = PersonFactory.CreatePerson();
            person.SetId(Guid.NewGuid());

            RunPayrollExportEvent message = new RunPayrollExportEvent {OwnerPersonId = person.Id.GetValueOrDefault()};

            using(mocks.Record())
            {
                Expect.Call(repositoryFactory.CreatePersonRepository(unitOfWork)).Return(personRepository);
                Expect.Call(personRepository.Get(person.Id.GetValueOrDefault())).Return(person);
            }
            using (mocks.Playback())
            {
                var target = new PayrollPeopleLoader(repositoryFactory);
                var owningPerson = target.GetOwningPerson(message, unitOfWork);
                Assert.AreEqual(person,owningPerson);
            }
        }

        [Test]
        public void ShouldGetPeopleForExport()
        {
            Guid includeInExport = Guid.NewGuid();
            Guid notInExport = Guid.NewGuid();

            IPerson personInExport = PersonFactory.CreatePerson();
            personInExport.SetId(includeInExport);

            IPerson personNotInExport = PersonFactory.CreatePerson();
            personNotInExport.SetId(notInExport);

            RunPayrollExportEvent message = new RunPayrollExportEvent { ExportPersonIdCollection = new Collection<Guid> {includeInExport}};
            var payrollExportPeriod = new DateOnlyPeriod();

            using (mocks.Record())
            {
                Expect.Call(repositoryFactory.CreatePersonRepository(unitOfWork)).Return(personRepository);
                Expect.Call(personRepository.FindAllAgentsLight(payrollExportPeriod.Convert())).Return(new List<IPerson>{personInExport,personNotInExport});
            }
            using (mocks.Playback())
            {
                var target = new PayrollPeopleLoader(repositoryFactory);
                var peopleForExport = target.GetPeopleForExport(message, payrollExportPeriod, unitOfWork);
                Assert.AreEqual(personInExport, peopleForExport.Single());
            }
        }
    }
}
