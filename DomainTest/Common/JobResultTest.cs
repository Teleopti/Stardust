using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class JobResultTest
    {
        private IJobResult target;
        private IPerson personOwner;
        private DateTime timestamp;
        private DateOnlyPeriod period;

        [SetUp]
        public void Setup()
        {
            personOwner = PersonFactory.CreatePerson();
            timestamp = DateTime.UtcNow;
            period = new DateOnlyPeriod(2011,8,1,2011,8,31);
            target = new JobResult(JobCategory.MultisiteExport, period, personOwner, timestamp);
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
            Assert.AreEqual(period,target.Period);
            Assert.AreEqual(JobCategory.MultisiteExport,target.JobCategory);
        }

        [Test]
        public void ShouldNotTreatDetailedInformationAsError()
        {
            var jobResultDetail = new JobResultDetail(DetailLevel.Info, "Information", timestamp, null);
            target.AddDetail(jobResultDetail);

            Assert.IsFalse(target.HasError());
        }
        
        [Test]
        public void ShouldTreatDetailWithErrorAsError()
        {
            var jobResultDetail = new JobResultDetail(DetailLevel.Error, "Nasty error!", timestamp, null);
            target.AddDetail(jobResultDetail);

            Assert.IsTrue(target.HasError());
        }

        [Test]
        public void ShouldHaveDetailsAfterAddingDetail()
        {
            var jobResultDetail = new JobResultDetail(DetailLevel.Info, "Information", timestamp, null);
            target.AddDetail(jobResultDetail);

            Assert.AreEqual(jobResultDetail,target.Details.Single());
        }

        [Test]
        public void ShouldTreatNoResultInTwelveHoursAsTimedOut()
        {
            target = new JobResult(JobCategory.MultisiteExport, period, personOwner, timestamp.AddHours(-12.5));
            Assert.IsTrue(target.HasError());
        }

        [Test]
        public void ShouldHaveStatusWorking()
        {
            target = new JobResult(JobCategory.QuickForecast, period, personOwner, timestamp.AddHours(-11.5));
            target.IsWorking().Should().Be.True();
            target.FinishedOk.Should().Be.False();
            target.HasError().Should().Be.False();
        }

        [Test]
        public void ShouldHaveStatusFinishedOk()
        {
            target.FinishedOk = true;

            target.IsWorking().Should().Be.False();
            target.FinishedOk.Should().Be.True();
            target.HasError().Should().Be.False();
        }

        [Test]
        public void ShouldHaveNewPeriod()
        {
            Assert.That(target.Period, Is.EqualTo(period));

            var newPeriod = new DateOnlyPeriod(2012, 1, 1, 2012, 1, 2);
            target.Period = newPeriod;

            Assert.That(target.Period, Is.EqualTo(newPeriod));
        }
    }
}
