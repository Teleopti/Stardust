using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("BucketB")]
    public class DayOffTemplateRepositoryTest : RepositoryTest<IDayOffTemplate>
    {
        private IDayOffTemplate _dayOff;
        private IDayOffTemplate _dayOff2;
        private IDayOffTemplate _dayOff3;
        private Description _description;
        private TimeSpan _timeSpanTargetLength;
        private TimeSpan _timeSpanFlexibility;
        private TimeSpan _timeSpanAnchor;
        private DayOffTemplateRepository _dayOffRepo;

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            _timeSpanTargetLength = new TimeSpan(1, 1, 1, 1);
            _timeSpanFlexibility = new TimeSpan(2, 2, 2, 2);
            _timeSpanAnchor = new TimeSpan(3, 3, 3, 3);

            _description = new Description("Day Off Test");
            _dayOff = DayOffFactory.CreateDayOff();
            _dayOffRepo = new DayOffTemplateRepository(UnitOfWork);
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IDayOffTemplate CreateAggregateWithCorrectBusinessUnit()
        {
            DayOffTemplate dayOff = DayOffFactory.CreateDayOff();
            dayOff.ChangeDescription(_description.Name, _description.ShortName);
            dayOff.SetTargetAndFlexibility(_timeSpanTargetLength, _timeSpanFlexibility);
            dayOff.Anchor = _timeSpanAnchor;

            return dayOff;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IDayOffTemplate loadedAggregateFromDatabase)
        {
            IDayOffTemplate dayOff = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(dayOff.Description, loadedAggregateFromDatabase.Description);
            Assert.AreEqual(dayOff.TargetLength, loadedAggregateFromDatabase.TargetLength);
            Assert.AreEqual(dayOff.Flexibility, loadedAggregateFromDatabase.Flexibility);
            Assert.AreEqual(dayOff.Anchor, loadedAggregateFromDatabase.Anchor);
            Assert.AreEqual(dayOff.PayrollCode, loadedAggregateFromDatabase.PayrollCode);
        }

        protected override Repository<IDayOffTemplate> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new DayOffTemplateRepository(currentUnitOfWork);
        }


        [Test]
        public void VerifyCanLoadDayOffsSortedByDescription()
        {
            AddFewDayOffs();
            IList<IDayOffTemplate> _dayOffList = _dayOffRepo.FindAllDayOffsSortByDescription();
            Assert.AreEqual("AAA", _dayOffList[0].Description.Name);
        }

        private void AddFewDayOffs()
        {
            _dayOff = new DayOffTemplate(new Description("BBB"));
            _dayOff2 = new DayOffTemplate(new Description("AAA"));
            _dayOff3 = new DayOffTemplate(new Description("CCC"));

            PersistAndRemoveFromUnitOfWork(_dayOff);
            PersistAndRemoveFromUnitOfWork(_dayOff2);
            PersistAndRemoveFromUnitOfWork(_dayOff3);
        }
    }
}