using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture ]
    [Category("BucketB")]
    public class PayrollExportRepositoryTest : RepositoryTest<IPayrollExport>
    {
        private readonly Guid _formatId = Guid.NewGuid();
        private string _formatName = "hej";
        private IPerson _person1;
        private IPerson _person2;
        private IPerson _person3;
        private IPerson _person4;

        protected override void ConcreteSetup()
        {
            _person1 = PersonFactory.CreatePerson("din", "mamma");
            PersistAndRemoveFromUnitOfWork(_person1);
            _person2 = PersonFactory.CreatePerson("a", "b");
            PersistAndRemoveFromUnitOfWork(_person2);
            _person3 = PersonFactory.CreatePerson("c", "d");
            PersistAndRemoveFromUnitOfWork(_person3);
            _person4 = PersonFactory.CreatePerson("e", "f");
            PersistAndRemoveFromUnitOfWork(_person4);
        }

        protected override IPayrollExport CreateAggregateWithCorrectBusinessUnit()
        {
             IPayrollExport payrollExport = new PayrollExport();
            payrollExport.FileFormat = ExportFormat.CommaSeparated;
            payrollExport.Name = "TestPE";
            payrollExport.Period = new DateOnlyPeriod(2000, 1, 1, 2001, 1, 1);
            payrollExport.PayrollFormatId = _formatId;
            payrollExport.PayrollFormatName = _formatName;
            
            IList<IPerson> persons = new List<IPerson>
                                         {
                                            _person1,_person2,_person3,_person4
                                         };
            payrollExport.ClearPersons();
            payrollExport.AddPersons(persons);
            return payrollExport;
        }


        protected override void VerifyAggregateGraphProperties(IPayrollExport loadedAggregateFromDatabase)
        {
            IPayrollExport export = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(export.FileFormat , loadedAggregateFromDatabase.FileFormat);
            Assert.AreEqual(export.Name, loadedAggregateFromDatabase.Name);
            Assert.AreEqual(export.Period, loadedAggregateFromDatabase.Period);
            Assert.AreEqual(export.Persons.Count, loadedAggregateFromDatabase.Persons.Count);         
            IPerson oPerson =export.Persons.FirstOrDefault(x=>x.Name.LastName == "mamma" )   ;
            Assert.IsNotNull(oPerson);
            Assert.AreEqual(export.PayrollFormatId , loadedAggregateFromDatabase.PayrollFormatId);
            Assert.AreEqual(export.PayrollFormatName , loadedAggregateFromDatabase.PayrollFormatName );
        }

        protected override Repository<IPayrollExport> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new PayrollExportRepository(currentUnitOfWork);
        }
    }
}
