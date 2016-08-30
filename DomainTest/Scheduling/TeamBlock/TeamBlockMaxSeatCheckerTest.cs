using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class TeamBlockMaxSeatCheckerTest
    {
        private ITeamBlockMaxSeatChecker _target;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private ISchedulingOptions _schedulingOption;
        private MockRepository _mock;
        private DateOnly _dateOnly;
        private ISkill _skill1;
        private ISkillDay _skillDay;
        private Dictionary<ISkill, IEnumerable<ISkillDay>> _skillDaysPair;
        private ISkillType _skillType;
        private ReadOnlyCollection<ISkillStaffPeriod> _skillStaffPeriodCollection;
        private ISkillStaffPeriod _skillStaffPeriod1;
        private ISkillStaffPeriod _skillStaffPeriod2;
        private ISkillStaff _skillStaff1;
        private ISkillStaff _skillStaff2;
        private ISkill _skill2;
        private ISkillType _skillType2;
	    private ITeamInfo _teamInfo;
	    private IScheduleMatrixPro _scheduleMatrixPro;
	    private IList<IScheduleMatrixPro> _scheduleMatrixPros;
	    private IPerson _person;
	    private IPersonPeriod _personPeriod;
	    private ITeam _team;
	    private ISite _site;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
            _schedulingOption = new SchedulingOptions();
	         _schedulingOption.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;
            _target=new TeamBlockMaxSeatChecker(()=>_schedulingResultStateHolder);
            _dateOnly = DateOnly.Today;
            _skill1 = _mock.StrictMock<ISkill>();
            _skill2 = _mock.StrictMock<ISkill>();
            _skillDay = _mock.StrictMock<ISkillDay>(); 
            
            _skillDaysPair = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
            _skillDaysPair.Add(_skill1, new List<ISkillDay>() { _skillDay });
            _skillDaysPair.Add(_skill2, new List<ISkillDay>() { _skillDay });

            _skillType = _mock.StrictMock<ISkillType>();
            _skillType2 = _mock.StrictMock<ISkillType>();

            _skillStaffPeriod1 = _mock.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriod2 = _mock.StrictMock<ISkillStaffPeriod>();
            _skillStaffPeriodCollection =
                new ReadOnlyCollection<ISkillStaffPeriod>(new List<ISkillStaffPeriod>()
                    {
                        _skillStaffPeriod1,
                        _skillStaffPeriod2
                    });

            _skillStaff1 = _mock.StrictMock<ISkillStaff>();
            _skillStaff2 = _mock.StrictMock<ISkillStaff>();
			//_teamInfo = new TeamInfo(new Group(), new List<IList<IScheduleMatrixPro>>() );
	        _teamInfo = _mock.StrictMock<ITeamInfo>();
	        _scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPros = new List<IScheduleMatrixPro> {_scheduleMatrixPro};
	        _person = _mock.StrictMock<IPerson>();
	        _personPeriod = _mock.StrictMock<IPersonPeriod>();
	        _team = _mock.StrictMock<ITeam>();
	        _site = _mock.StrictMock<ISite>();
        }

		[Test]
		public void ShouldContinueWithSkillUnderMaxSeat()
		{

			using (_mock.Record())
			{
				commonExpectCall();

				Expect.Call(_skillStaffPeriod1.Payload).Return(_skillStaff1).Repeat.AtLeastOnce();
				Expect.Call(_skillStaff1.CalculatedUsedSeats).Return(5.0);
				Expect.Call(_skillStaff1.MaxSeats).Return(6);
				Expect.Call(_skillStaffPeriod2.Payload).Return(_skillStaff2).Repeat.AtLeastOnce();
				Expect.Call(_skillStaff2.CalculatedUsedSeats).Return(4.0);
				Expect.Call(_skillStaff2.MaxSeats).Return(6);

				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_dateOnly)).Return(_scheduleMatrixPros);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person);
				Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
				Expect.Call(_personPeriod.Team).Return(_team);
				Expect.Call(_team.Site).Return(_site);
				Expect.Call(_site.MaxSeatSkill).Return(_skill1);
			}
			Assert.IsTrue(_target.CheckMaxSeat(_dateOnly, _schedulingOption, _teamInfo));
		}

		[Test]
		public void ShouldContinueWithSkillWhenSeatsAreEqual()
		{

			using (_mock.Record())
			{
				commonExpectCall();

				Expect.Call(_skillStaffPeriod1.Payload).Return(_skillStaff1).Repeat.AtLeastOnce();
				Expect.Call(_skillStaff1.CalculatedUsedSeats).Return(5.0);
				Expect.Call(_skillStaff1.MaxSeats).Return(5);
				Expect.Call(_skillStaffPeriod2.Payload).Return(_skillStaff2).Repeat.AtLeastOnce();
				Expect.Call(_skillStaff2.CalculatedUsedSeats).Return(6.0);
				Expect.Call(_skillStaff2.MaxSeats).Return(6);


				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_dateOnly)).Return(_scheduleMatrixPros);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person);
				Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
				Expect.Call(_personPeriod.Team).Return(_team);
				Expect.Call(_team.Site).Return(_site);
				Expect.Call(_site.MaxSeatSkill).Return(_skill1);
			}
			Assert.IsTrue(_target.CheckMaxSeat(_dateOnly, _schedulingOption, _teamInfo));
		}

		[Test]
		public void ShouldNotContinueWithSkillWhenSeatsAreMore()
		{

			using (_mock.Record())
			{
				commonExpectCall();

				Expect.Call(_skillStaffPeriod1.Payload).Return(_skillStaff1).Repeat.AtLeastOnce();
				Expect.Call(_skillStaff1.CalculatedUsedSeats).Return(6.0);
				Expect.Call(_skillStaff1.MaxSeats).Return(5);
				Expect.Call(_skillStaffPeriod2.Payload).Return(_skillStaff2).Repeat.AtLeastOnce();
				Expect.Call(_skillStaff2.CalculatedUsedSeats).Return(7.0);
				Expect.Call(_skillStaff2.MaxSeats).Return(6);
				Expect.Call(_teamInfo.MatrixesForGroupAndDate(_dateOnly)).Return(_scheduleMatrixPros);
				Expect.Call(_scheduleMatrixPro.Person).Return(_person);
				Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
				Expect.Call(_personPeriod.Team).Return(_team);
				Expect.Call(_team.Site).Return(_site);
				Expect.Call(_site.MaxSeatSkill).Return(_skill1);
			}
			Assert.IsFalse(_target.CheckMaxSeat(_dateOnly, _schedulingOption, _teamInfo));
		}



		private void commonExpectCall()
        {
            Expect.Call(_schedulingResultStateHolder.SkillDays).Return(_skillDaysPair);
            Expect.Call(_skill1.SkillType).Return(_skillType);
            Expect.Call(_skillType.ForecastSource).Return(ForecastSource.MaxSeatSkill);
            Expect.Call(_skill2.SkillType).Return(_skillType2);
            Expect.Call(_skillType2.ForecastSource).Return(ForecastSource.Email );
            Expect.Call(_skillDay.CurrentDate).Return(_dateOnly);
            Expect.Call(_skillDay.SkillStaffPeriodCollection).Return(_skillStaffPeriodCollection);
        }
    }

    
}
