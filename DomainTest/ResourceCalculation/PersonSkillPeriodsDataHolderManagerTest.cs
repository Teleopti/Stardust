using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class PersonSkillPeriodsDataHolderManagerTest
    {
        private IPersonSkillPeriodsDataHolderManager _target;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private ISkillStaffPeriodHolder _skillStafPeriodHolder;
        private IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> _theDictionary;
        private IPerson _person;
        private IVirtualSchedulePeriod _schedulePeriod;
	    private ISkillPriorityProvider _skillPriorityProvider;
        
        [SetUp]
        public void Setup()
        {
            _schedulingResultStateHolder = MockRepository.GenerateMock<ISchedulingResultStateHolder>();
			_skillPriorityProvider = new SkillPriorityProvider();
            _target = new PersonSkillPeriodsDataHolderManager(()=>_schedulingResultStateHolder, _skillPriorityProvider);
			_skillStafPeriodHolder = MockRepository.GenerateMock<ISkillStaffPeriodHolder>();
			_schedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			_theDictionary = MockRepository.GenerateMock<IDictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>>>();
        }

	    [Test]
	    public void CanGetPersonSkillPeriodsDataHolderDictionary()
	    {
		    var dateOnly = new DateOnly(2009, 2, 2);
		    var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time");
		    var skill = SkillFactory.CreateSkill("Skill");
		    var skills = new List<ISkill> {skill};

		    _person = PersonFactory.CreatePersonWithPersonPeriod(dateOnly, new List<ISkill>());
		    _person.AddSkill(new PersonSkill(skill, new Percent(1)), _person.Period(dateOnly));
		    _person.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);

		    _schedulePeriod.Stub(x => x.Person).Return(_person);
		    _schedulingResultStateHolder.Stub(x => x.SkillStaffPeriodHolder).Return(_skillStafPeriodHolder);
		    _skillStafPeriodHolder.Stub(x => x.SkillStaffDataPerActivity(new DateTimePeriod(), skills, _skillPriorityProvider)).Return(_theDictionary).IgnoreArguments();

		    var personSkillDay = new PersonSkillDayCreator(new PersonalSkillsProvider()).Create(dateOnly, _schedulePeriod);
		    var ret = _target.GetPersonSkillPeriodsDataHolderDictionary(personSkillDay);
		    
			Assert.IsNotNull(ret);
	    }

	    [Test]
	    public void CanGetPersonNonBlendSkillSkillStaffPeriods()
	    {
		    var dateOnly = new DateOnly(2009, 2, 2);
		    var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time");
		    var skill = SkillFactory.CreateNonBlendSkill("noneBlendSkill");
		    var skills = new List<ISkill> {skill};

		    _person = PersonFactory.CreatePersonWithPersonPeriodFromTeam(dateOnly, TeamFactory.CreateTeam("Team 1", "Paris"));
			_person.Period(dateOnly).AddPersonNonBlendSkill(new PersonSkill(skill, new Percent(1)));
		    _person.PermissionInformation.SetDefaultTimeZone(timeZoneInfo);

		    _schedulePeriod.Stub(x => x.Person).Return(_person);
		    _schedulingResultStateHolder.Stub(x => x.SkillStaffPeriodHolder).Return(_skillStafPeriodHolder);
		    _skillStafPeriodHolder.Stub(x => x.SkillStaffPeriodDictionary(skills, new DateTimePeriod()))
			    .Return(new Dictionary<ISkill, ISkillStaffPeriodDictionary>())
			    .IgnoreArguments();

		    var personSkillDay = new PersonSkillDayCreator(new PersonalSkillsProvider()).Create(dateOnly, _schedulePeriod);
		    var ret = _target.GetPersonNonBlendSkillSkillStaffPeriods(personSkillDay);
		    Assert.IsNotNull(ret);
	    }
    }
}
