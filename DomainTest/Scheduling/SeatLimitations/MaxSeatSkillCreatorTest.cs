using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.SeatLimitations
{
    [TestFixture]
    public class MaxSeatSkillCreatorTest
    {
        private MockRepository _mocks;
        private  IMaxSeatSitesExtractor _maxSeatSitesExtractor;
        private  ICreateSkillsFromMaxSeatSites _createSkillsFromMaxSeatSites;
        private  ICreatePersonalSkillsFromMaxSeatSites _createPersonalSkillsFromMaxSeatSites;
        private  ISchedulerSkillDayHelper _schedulerSkillDayHelper;
        private MaxSeatSkillCreator _target;
        private DateOnlyPeriod _dateOnlyPeriod;
        private IList<IPerson> _persons;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _maxSeatSitesExtractor = _mocks.StrictMock<IMaxSeatSitesExtractor>();
            _createSkillsFromMaxSeatSites = _mocks.StrictMock<ICreateSkillsFromMaxSeatSites>();
            _createPersonalSkillsFromMaxSeatSites = _mocks.StrictMock<ICreatePersonalSkillsFromMaxSeatSites>();
            _schedulerSkillDayHelper = _mocks.StrictMock<ISchedulerSkillDayHelper>();
            _persons = new List<IPerson> { new Person() };
            _target = new MaxSeatSkillCreator(_maxSeatSitesExtractor, _createSkillsFromMaxSeatSites,
                                              _createPersonalSkillsFromMaxSeatSites,
                                              _schedulerSkillDayHelper, _persons);

            _dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2011, 1, 19), new DateOnly(2011, 2, 19));
        }

        [Test]
        public void ShouldGetMaxSeatSitesAndPersonsAndCreateSkills()
        {
            var sites = new HashSet<ISite> {new Site("sajt")};
			var extendedDatePeriod = new DateOnlyPeriod(_dateOnlyPeriod.StartDate.AddDays(-8), _dateOnlyPeriod.EndDate.AddDays(8));
			
			Expect.Call(_maxSeatSitesExtractor.MaxSeatSites(_dateOnlyPeriod)).Return(sites);
            Expect.Call(() => _createSkillsFromMaxSeatSites.CreateSkillList(sites));
            
            Expect.Call(() =>_createPersonalSkillsFromMaxSeatSites.Process(_dateOnlyPeriod, _persons));
	        Expect.Call(
		        () => _schedulerSkillDayHelper.AddSkillDaysToStateHolder(
			        extendedDatePeriod,
			        ForecastSource.MaxSeatSkill, 0));
            _mocks.ReplayAll();
            _target.CreateMaxSeatSkills(_dateOnlyPeriod);
            _mocks.VerifyAll();
        }
    }
}