using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
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
        private Dictionary<ISkill, IList<ISkillDay>> _skillDaysPair;
        private ISkillType _skillType;
        private ReadOnlyCollection<ISkillStaffPeriod> _skillStaffPeriodCollection;
        private ISkillStaffPeriod _skillStaffPeriod1;
        private ISkillStaffPeriod _skillStaffPeriod2;
        private ISkillStaff _skillStaff1;
        private ISkillStaff _skillStaff2;
        private ISkill _skill2;
        private ISkillType _skillType2;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
            _schedulingOption = new SchedulingOptions();
	         _schedulingOption.UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeatsAndDoNotBreak;
            _target=new TeamBlockMaxSeatChecker(_schedulingResultStateHolder);
            _dateOnly = DateOnly.Today;
            _skill1 = _mock.StrictMock<ISkill>();
            _skill2 = _mock.StrictMock<ISkill>();
            _skillDay = _mock.StrictMock<ISkillDay>(); 
            
            _skillDaysPair = new Dictionary<ISkill, IList<ISkillDay>>();
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
            }
            Assert.IsTrue( _target.CheckMaxSeat(_dateOnly, _schedulingOption));
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
            }
			Assert.IsTrue(_target.CheckMaxSeat(_dateOnly, _schedulingOption));
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
            }
			Assert.IsFalse(_target.CheckMaxSeat(_dateOnly, _schedulingOption));
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
