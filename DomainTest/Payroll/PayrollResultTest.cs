using System;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Payroll;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Payroll
{
    [TestFixture]
    public class PayrollResultTest
    {
        private IPayrollResult target;
        private IPerson personOwner;
        private IPayrollExport payrollExport;
        private DateTime timestamp;

        [SetUp]
        public void Setup()
        {
            personOwner = PersonFactory.CreatePerson();
            payrollExport = new PayrollExport();
            payrollExport.PayrollFormatId = Guid.NewGuid();
            payrollExport.PayrollFormatName = "test";
            payrollExport.Period = new DateOnlyPeriod();
            timestamp = DateTime.UtcNow;
            target = new PayrollResult(payrollExport, personOwner, timestamp);
        }

        [Test]
        public void VerifyIsAggregateRoot()
        {
            Assert.IsInstanceOf<IAggregateRoot>(target);
        }

        [Test]
        public void VerifyHasEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType(),true));
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(personOwner,target.Owner);
            Assert.AreEqual(timestamp,target.Timestamp);
            Assert.AreEqual(payrollExport.Period,target.Period);
            Assert.AreEqual(payrollExport.PayrollFormatId,target.PayrollFormatId);
            Assert.AreEqual(payrollExport.PayrollFormatName,target.PayrollFormatName);
        }

        [Test]
        public void VerifyCanSetResult()
        {
            Assert.IsNull(target.XmlResult.XPathNavigable);

            XmlDocument document = new XmlDocument();
            document.AppendChild(document.CreateElement("my-large-result"));
            target.XmlResult.SetResult(document);

            Assert.AreEqual(target.XmlResult.XPathNavigable,document);
            Assert.IsFalse(target.HasError());
        }

        [Test]
        public void ShouldTreatDetailedInformationAsError()
        {
            var payrollResultDetail = new PayrollResultDetail(DetailLevel.Info, "Information", timestamp, null);
            target.AddDetail(payrollResultDetail);

            Assert.IsFalse(target.HasError());
        }
        
        [Test]
        public void ShouldTreatDetailWithErrorAsError()
        {
            var payrollResultDetail = new PayrollResultDetail(DetailLevel.Error, "Nasty error!", timestamp, null);
            target.AddDetail(payrollResultDetail);

            Assert.IsTrue(target.HasError());
        }

        [Test]
        public void ShouldHaveDetailsAfterAddingDetail()
        {
            var payrollResultDetail = new PayrollResultDetail(DetailLevel.Info, "Information", timestamp, null);
            target.AddDetail(payrollResultDetail);

            Assert.AreEqual(payrollResultDetail,target.Details.Single());
        }

        [Test]
        public void ShouldTreatNoResultInTwelveHoursAsTimedOut()
        {
            target = new PayrollResult(payrollExport, personOwner, timestamp.AddHours(-12.5));
            Assert.IsTrue(target.HasError());
        }

        [Test]
        public void ShouldHaveStatusWorking()
        {
            target = new PayrollResult(payrollExport, personOwner, timestamp.AddHours(-11.5));
            target.IsWorking().Should().Be.True();
            target.FinishedOk.Should().Be.False();
            target.HasError().Should().Be.False();
        }

        [Test]
        public void ShouldHaveStatusFinishedOk()
        {
            var document = new XmlDocument();
            document.AppendChild(document.CreateElement("my-large-result"));
            target.XmlResult.SetResult(document);
            target.IsWorking().Should().Be.False();
            target.FinishedOk.Should().Be.True();
            target.HasError().Should().Be.False();
        }
    }
}
