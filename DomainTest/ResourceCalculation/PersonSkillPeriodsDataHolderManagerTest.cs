using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class PersonSkillPeriodsDataHolderManagerTest
    {
        private IPersonSkillPeriodsDataHolderManager _target;
        private MockRepository _mocks;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private ISkillStaffPeriodHolder _skillStafPeriodHolder;
        private IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> _theDictionary;
        private IPerson _person;
        private IVirtualSchedulePeriod _schedulePeriod;
        

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _target = new PersonSkillPeriodsDataHolderManager(()=>_schedulingResultStateHolder);
            _skillStafPeriodHolder = _mocks.StrictMock<ISkillStaffPeriodHolder>();
            _schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            _theDictionary = _mocks.StrictMock<IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>>>();    
            
        }

        [Test]
       public void CanGetPersonSkillPeriodsDataHolderDictionary()
        {
            TimeZoneInfo timeZoneInfo =
                (TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time"));
            ISkill skill = SkillFactory.CreateSkill("Skill");
            IList<ISkill> skills = new List<ISkill>{skill};

            _person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill>());
            _person.AddSkill(new PersonSkill(skill, new Percent(1)), _person.Period(new DateOnly()));
            _person.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);
            
            var dateOnly = new DateOnly(2009,2,2);
            using (_mocks.Record())
            {
                Expect.Call(_schedulePeriod.Person).Return(_person).Repeat.Twice();
                Expect.Call(_schedulingResultStateHolder.SkillStaffPeriodHolder).Return(_skillStafPeriodHolder);
                Expect.Call(_skillStafPeriodHolder.SkillStaffDataPerActivity(new DateTimePeriod(), skills)).Return(_theDictionary).IgnoreArguments();
            }

            using (_mocks.Playback())
            {
				var ret = _target.GetPersonSkillPeriodsDataHolderDictionary(dateOnly, _schedulePeriod);
                Assert.IsNotNull(ret);
            }
        }

        [Test]
        public void ShouldReturnEmptyPersonMaxSeatSkillSkillStaffPeriodsWhenMaxSeatIsNull()
        {
            var site = _mocks.StrictMock<ISite>();
            var dateOnly = new DateOnly(2009, 2, 2);
            _person = _mocks.StrictMock<IPerson>();
            var personPeriod = _mocks.StrictMock<IPersonPeriod>();
            var team = _mocks.StrictMock<ITeam>();

            using (_mocks.Record())
            {
                Expect.Call(_schedulePeriod.Person).Return(_person);
                Expect.Call(_person.Period(dateOnly)).Return(personPeriod);
                Expect.Call(personPeriod.Team).Return(team);
                Expect.Call(team.Site).Return(site);
                //Expect.Call(_schedulePeriod.Site).Return(site);
                Expect.Call(site.MaxSeatSkill).Return(null);
                }

            using (_mocks.Playback())
            {
                var ret = _target.GetPersonMaxSeatSkillSkillStaffPeriods(dateOnly, _schedulePeriod);
                Assert.IsNotNull(ret);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void CanGetPersonMaxSeatSkillSkillStaffPeriods()
        {
            var site = _mocks.StrictMock<ISite>();
            TimeZoneInfo timeZoneInfo =
                (TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time"));
            ISkill skill = SkillFactory.CreateSiteSkill("siteSkill");
            IList<ISkill> skills = new List<ISkill> { skill };
            var team = _mocks.StrictMock<ITeam>();

            _person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill>());
            _person.Period(new DateOnly()).AddPersonMaxSeatSkill(new PersonSkill(skill, new Percent(1)));
            _person.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);
            _person.Period(new DateOnly()).Team = team;

            var dateOnly = new DateOnly(2009, 2, 2);
            using (_mocks.Record())
            {
                Expect.Call(_schedulePeriod.Person).Return(_person);
                Expect.Call(team.Site).Return(site);
                //Expect.Call(_schedulePeriod.Site).Return(site);
                Expect.Call(site.MaxSeatSkill).Return(skill);
                Expect.Call(_schedulePeriod.Person).Return(_person).Repeat.Twice();
                Expect.Call(_schedulingResultStateHolder.SkillStaffPeriodHolder).Return(_skillStafPeriodHolder);
                Expect.Call(_skillStafPeriodHolder.SkillStaffPeriodDictionary(skills,new DateTimePeriod())).Return(new Dictionary<ISkill, ISkillStaffPeriodDictionary>()).IgnoreArguments();
            }

            using (_mocks.Playback())
            {
                var ret = _target.GetPersonMaxSeatSkillSkillStaffPeriods(dateOnly, _schedulePeriod);
                Assert.IsNotNull(ret);
            }
        }

        [Test]
        public void CanGetPersonNonBlendSkillSkillStaffPeriods()
        {
            TimeZoneInfo timeZoneInfo =
                (TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time"));
            ISkill skill = SkillFactory.CreateNonBlendSkill("noneBlendSkill");
            IList<ISkill> skills = new List<ISkill> { skill };

            _person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill>());
            _person.Period(new DateOnly()).AddPersonNonBlendSkill(new PersonSkill(skill, new Percent(1)));
            _person.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);

            var dateOnly = new DateOnly(2009, 2, 2);
            using (_mocks.Record())
            {
                Expect.Call(_schedulePeriod.Person).Return(_person).Repeat.Twice();
                Expect.Call(_schedulingResultStateHolder.SkillStaffPeriodHolder).Return(_skillStafPeriodHolder);
                Expect.Call(_skillStafPeriodHolder.SkillStaffPeriodDictionary(skills, new DateTimePeriod())).Return(new Dictionary<ISkill, ISkillStaffPeriodDictionary>()).IgnoreArguments();
            }

            using (_mocks.Playback())
            {
                var ret = _target.GetPersonNonBlendSkillSkillStaffPeriods(dateOnly, _schedulePeriod);
                Assert.IsNotNull(ret);
            }
        }
    }

    
}
