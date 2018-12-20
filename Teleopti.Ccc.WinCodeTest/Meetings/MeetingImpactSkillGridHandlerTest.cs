using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Meetings.Interfaces;


namespace Teleopti.Ccc.WinCodeTest.Meetings
{
    [TestFixture]
    public class MeetingImpactSkillGridHandlerTest
    {
        private MeetingImpactSkillGridHandler _target;
        private MockRepository _mocks;
        private ISchedulerStateHolder _schedulerStateHolder;
        private IMeetingImpactView _meetingImpactView;
        private TimeZoneInfo _timeZone;
        private DateTimePeriod _requestedPeriod;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private IMeetingViewModel _meetingViewModel;
        private ISkill _skill;
        private ISet<ISkill> _skills;
        private ISkillTypePhone _skillTypePhone;
        private IScenario _scenario;
        private IPeopleAndSkillLoaderDecider _decider;
        private IUnitOfWorkFactory _uowFactory;
	    private ILoaderDeciderResult _deciderResult;


	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulerStateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _meetingImpactView = _mocks.StrictMock<IMeetingImpactView>();
            _meetingViewModel = _mocks.StrictMock<IMeetingViewModel>();
            _timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            _decider = _mocks.StrictMock<IPeopleAndSkillLoaderDecider>();
            _deciderResult = _mocks.StrictMock<ILoaderDeciderResult>();
            _uowFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            var startRequestedPeriod = new DateTime(2010, 11, 2, 23, 0, 0, DateTimeKind.Utc);
            _requestedPeriod = new DateTimePeriod(startRequestedPeriod, startRequestedPeriod.AddDays(20));
            _skill = _mocks.StrictMock<ISkill>();
            _skills = new HashSet<ISkill>{ _skill };

            _skillTypePhone = _mocks.StrictMock<ISkillTypePhone>();
            _scenario = new Scenario("scenario");

            _target = new MeetingImpactSkillGridHandler(_meetingImpactView, _meetingViewModel,_schedulerStateHolder,_uowFactory,_decider);
        }

		[Test]
        public void ShouldCallViewForEverySkill()
        {
            var meeting = _mocks.StrictMock<IMeeting>();
            Expect.Call(() => _meetingImpactView.RemoveAllSkillTabs());
            Expect.Call(_schedulerStateHolder.SchedulingResultState).Return(_schedulingResultStateHolder);
            Expect.Call(_schedulingResultStateHolder.Skills).Return(_skills);

            Expect.Call(_uowFactory.CreateAndOpenUnitOfWork()).Return(null);
            
            Expect.Call(_schedulerStateHolder.RequestedScenario).Return(_scenario);
        	Expect.Call(_schedulerStateHolder.RequestedPeriod).Return(
        		new DateOnlyPeriodAsDateTimePeriod(
        			_requestedPeriod.ToDateOnlyPeriod(TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone),
        			TeleoptiPrincipalForLegacy.CurrentPrincipal.Regional.TimeZone));
            Expect.Call(_meetingViewModel.Meeting).Return(meeting);
            Expect.Call(meeting.MeetingPersons).Return(new ReadOnlyCollection<IMeetingPerson>(new List<IMeetingPerson>{new MeetingPerson(new Person(),false)}));
            
            Expect.Call(_decider.Execute(_scenario, _requestedPeriod, new List<IPerson>())).IgnoreArguments().Return(_deciderResult);
            Expect.Call(_deciderResult.FilterSkills(_skills,null,null)).IgnoreArguments().Return(1);

            Expect.Call(_meetingImpactView.ClearTabPages);

            Expect.Call(_skill.Name).Return("name").Repeat.AtLeastOnce();
            Expect.Call(_skill.Description).Return("description").Repeat.AtLeastOnce();
            Expect.Call(_skill.SkillType).Return(_skillTypePhone).Repeat.AtLeastOnce();
            Expect.Call(_skillTypePhone.ForecastSource).Return(ForecastSource.InboundTelephony).Repeat.AtLeastOnce();

            Expect.Call(() => _meetingImpactView.AddSkillTab("name", "description", 1, _skill)).IgnoreArguments().Repeat.AtLeastOnce();

            _mocks.ReplayAll();
            _target.SetupSkillTabs();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallViewDrawIntraday()
        {
            var skillStaffPeriod = _mocks.StrictMock<ISkillStaffPeriod>();
            IList<ISkillStaffPeriod> skillStaffPeriods = new List<ISkillStaffPeriod> { skillStaffPeriod };

            var periodToFind = new DateTimePeriod(2010, 11, 01, 2010, 11, 05);

            var skillStaffPeriodHolder = _mocks.StrictMock<ISkillStaffPeriodHolder>();

            Expect.Call(_schedulerStateHolder.SchedulingResultState).Return(_schedulingResultStateHolder);
            Expect.Call(_meetingImpactView.SelectedSkill()).Return(_skill);
            Expect.Call(_meetingImpactView.StartDate).Return(new DateOnly(2010, 11, 1));
            Expect.Call(_skill.IsVirtual).Return(false);
            Expect.Call(_schedulerStateHolder.TimeZoneInfo).Return(_timeZone);

            Expect.Call(_schedulingResultStateHolder.SkillStaffPeriodHolder).Return(skillStaffPeriodHolder).Repeat.Once();
            Expect.Call(skillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { _skill }, periodToFind)).IgnoreArguments().Return(skillStaffPeriods).Repeat.Once();
            
            Expect.Call(() => _meetingImpactView.ClearTabPages());
            Expect.Call(() => _meetingImpactView.DrawIntraday(_skill, _schedulerStateHolder, skillStaffPeriods));
            Expect.Call(() => _meetingImpactView.PositionControl()).Repeat.Once();

            _mocks.ReplayAll();
            _target.DrawSkillGrid();
            _mocks.VerifyAll();
        }
    }
}