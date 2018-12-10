using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    ///<summary>
    /// Tests PayrollResultRepository
    ///</summary>
    [TestFixture]
    [Category("BucketB")]
    public class PayrollResultRepositoryTest : RepositoryTest<IPayrollResult>
    {
        private IPayrollExport payrollExport;
        private IPerson owner;

        protected override void ConcreteSetup()
        {
            owner = PersonFactory.CreatePerson("e");
            PersistAndRemoveFromUnitOfWork(owner);

            payrollExport = new PayrollExport();
            payrollExport.PayrollFormatId = Guid.NewGuid();
            payrollExport.PayrollFormatName = "Teleopti Time Export";
            payrollExport.Period = new DateOnlyPeriod(DateOnly.Today.AddDays(-14), DateOnly.Today);
            payrollExport.Name = "Tomten";
            PersistAndRemoveFromUnitOfWork(payrollExport);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IPayrollResult CreateAggregateWithCorrectBusinessUnit()
        {
            IPayrollResult payrollResult = new PayrollResult(payrollExport, owner, DateTime.UtcNow);
            
            XmlDocument result = new XmlDocument();
            result.AppendChild(result.CreateElement("PayrollExportResult").AppendChild(result.CreateElement("Person1")));

            payrollResult.XmlResult.SetResult(result);

            payrollResult.PayrollExport = payrollExport;

            payrollResult.AddDetail(new PayrollResultDetail(DetailLevel.Info,"Payroll export initiated.",DateTime.UtcNow,null));

            return payrollResult;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IPayrollResult loadedAggregateFromDatabase)
        {
            IPayrollResult org = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(org.Owner, loadedAggregateFromDatabase.Owner);
            Assert.AreEqual(org.PayrollFormatId, loadedAggregateFromDatabase.PayrollFormatId);
            Assert.AreEqual(org.PayrollFormatName, loadedAggregateFromDatabase.PayrollFormatName);
            Assert.Greater(org.Timestamp, loadedAggregateFromDatabase.Timestamp);
            Assert.AreEqual(org.FinishedOk, loadedAggregateFromDatabase.FinishedOk);
            Assert.AreEqual(org.XmlResult.XPathNavigable.CreateNavigator().OuterXml, loadedAggregateFromDatabase.XmlResult.XPathNavigable.CreateNavigator().OuterXml);
            Assert.AreEqual(org.Details.Single().Message,loadedAggregateFromDatabase.Details.Single().Message);
        }

        [Test]
        public void VerifyCanSaveWithLargeResult()
        {
            IPayrollResult result = CreateAggregateWithCorrectBusinessUnit();

            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", string.Empty));
            XmlElement rootElement = document.CreateElement("root");
            document.AppendChild(rootElement);
            
            for (int i = 0; i < 2000; i++)
            {
                XmlElement xmlElement =
                    document.CreateElement(string.Format(System.Globalization.CultureInfo.InvariantCulture, "person-{0}",
                                                         i));

                xmlElement.SetAttribute("employee-number", "100" + i.ToString(System.Globalization.CultureInfo.InvariantCulture));
                rootElement.AppendChild(xmlElement);
            }
            
            result.XmlResult.SetResult(document);

            PersistAndRemoveFromUnitOfWork(result);
        }

        [Test]
        public void CanGetPayrollResultByPayrollExport()
        {
            IPayrollResult result = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(payrollExport);
            result.PayrollExport = payrollExport;
            PersistAndRemoveFromUnitOfWork(result);
            
            ICollection<IPayrollResult> payrollExports = new PayrollResultRepository(CurrUnitOfWork).GetPayrollResultsByPayrollExport(payrollExport);
            Assert.Contains(result, (ICollection)payrollExports);
        } 

        [Test]
        public void CanGetPayrollResultByPayrollExportXmlShouldBeLazy()
        {
            IPayrollResult result = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(payrollExport);
            result.PayrollExport = payrollExport;
            PersistAndRemoveFromUnitOfWork(result);

            var read = new PayrollResultRepository(CurrUnitOfWork).GetPayrollResultsByPayrollExport(payrollExport);
            Assert.IsFalse(LazyLoadingManager.IsInitialized(read.First().XmlResult));
        }
        
        protected override Repository<IPayrollResult> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new PayrollResultRepository(currentUnitOfWork);
        }
    }
}
