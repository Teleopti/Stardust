using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.NonBlendSkill
{
	[TestFixture]
	public class NonBlendWorkShiftCalculatorTest
	{
        private INonBlendWorkShiftCalculator _target;
		private MockRepository _mocks;
		private IPerson _person;
		private ISkill _skill;
		private INonBlendSkillImpactOnPeriodForProjection _nonBlendImpactOnPeriodForProjection;
		private IWorkShiftCalculator _workShiftCalculator;
	    private IActivity _activity;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_skill = SkillFactory.CreateNonBlendSkill("nonBlendSkill");
		    _activity = ActivityFactory.CreateActivity("acta dej");
		    _skill.Activity = _activity;
			_person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill>());
			_person.Period(new DateOnly()).PersonNonBlendSkillCollection.Add(new PersonSkill(_skill, new Percent(1)));
            _nonBlendImpactOnPeriodForProjection = _mocks.StrictMock<INonBlendSkillImpactOnPeriodForProjection>();
			_workShiftCalculator = _mocks.StrictMock<IWorkShiftCalculator>();
            _target = new NonBlendWorkShiftCalculator(_nonBlendImpactOnPeriodForProjection, _workShiftCalculator, new SkillPriorityProvider());
		}

		[Test]
		public void ShouldReturnOneIfPersonHaveSkill()
		{
			var dateTime = new DateTime(2010, 10, 10, 8, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(dateTime, dateTime.AddHours(1));
			ISkillDay skillDay = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(dateTime));
			ISkillStaffPeriod skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill, dateTime, 60, 20, 20);
			
			skillStaffPeriod1.SetSkillDay(skillDay);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(_skill)
			                                                             {{dateTimePeriod, skillStaffPeriod1}};
		    IDictionary<ISkill, ISkillStaffPeriodDictionary> dic = new Dictionary<ISkill, ISkillStaffPeriodDictionary>
		                                                               {{_skill, skillStaffPeriodDictionary}};
		    var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

			using (_mocks.Record())
			{
				Expect.Call(visualLayerCollection.Period()).Return(dateTimePeriod).Repeat.AtLeastOnce();
                Expect.Call(_nonBlendImpactOnPeriodForProjection.CalculatePeriod(skillStaffPeriod1, visualLayerCollection, _activity)).
					Return(1).Repeat.AtLeastOnce();
				Expect.Call(_workShiftCalculator.CalculateShiftValueForPeriod(0, 0, 0, 0)).IgnoreArguments().Return(1).Repeat.AtLeastOnce();
			}

			IWorkShiftCalculationResultHolder result;

			using (_mocks.Playback())
			{
				result = _target.CalculateShiftValue(_person, visualLayerCollection, dic, WorkShiftLengthHintOption.Free, false, false);
			}
            Assert.That(result.Value, Is.EqualTo(1));
		}

        [Test]
        public void ShouldReturnTwoIfPersonHaveTwoSkills()
        {
            var dateTime = new DateTime(2010, 10, 10, 8, 0, 0, DateTimeKind.Utc);
            var dateTimePeriod = new DateTimePeriod(dateTime, dateTime.AddHours(1));
            var skill2 = SkillFactory.CreateNonBlendSkill("skillTwo");
            skill2.Activity = _activity;
            ISkillDay skillDay = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(dateTime));
            ISkillDay skillDay2 = SkillDayFactory.CreateSkillDay(skill2, new DateOnly(dateTime));
            
            ISkillStaffPeriod skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill, dateTime, 60, 20, 20);
            ISkillStaffPeriod skillStaffPeriod2 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(skill2, dateTime, 60, 10, 10);

            skillStaffPeriod1.SetSkillDay(skillDay);
			skillStaffPeriod2.SetSkillDay(skillDay2);
            ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(_skill) { { dateTimePeriod, skillStaffPeriod1 } };
            ISkillStaffPeriodDictionary skillStaffPeriodDictionary2 = new SkillStaffPeriodDictionary(skill2) { { dateTimePeriod, skillStaffPeriod2 } };

            IDictionary<ISkill, ISkillStaffPeriodDictionary> dic = new Dictionary<ISkill, ISkillStaffPeriodDictionary> { { _skill, skillStaffPeriodDictionary }, 
                                                        { skill2, skillStaffPeriodDictionary2 } };
            var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

            using (_mocks.Record())
            {
                Expect.Call(visualLayerCollection.Period()).Return(dateTimePeriod).Repeat.AtLeastOnce();
                Expect.Call(_nonBlendImpactOnPeriodForProjection.CalculatePeriod(skillStaffPeriod1, visualLayerCollection, _activity)).
                    Return(1).Repeat.AtLeastOnce();
                Expect.Call(_nonBlendImpactOnPeriodForProjection.CalculatePeriod(skillStaffPeriod2, visualLayerCollection, _activity)).
                    Return(1).Repeat.AtLeastOnce();
                
                Expect.Call(_workShiftCalculator.CalculateShiftValueForPeriod(0, 0, 0, 0)).IgnoreArguments().Return(1).Repeat.AtLeastOnce();
            }

			IWorkShiftCalculationResultHolder result;

			using (_mocks.Playback())
            {
				result = _target.CalculateShiftValue(_person, visualLayerCollection, dic, WorkShiftLengthHintOption.Free, false, false);
            }
            Assert.That(result.Value, Is.EqualTo(2));
        }

        [Test]
        public void ShouldReturnZeroIfImpactIsZero()
        {
            var dateTime = new DateTime(2010, 10, 10, 8, 0, 0, DateTimeKind.Utc);
            var dateTimePeriod = new DateTimePeriod(dateTime, dateTime.AddHours(1));
            ISkillDay skillDay = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(dateTime));
            ISkillStaffPeriod skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill, dateTime, 60, 20, 20);

			skillStaffPeriod1.SetSkillDay(skillDay);
            ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(_skill) { { dateTimePeriod, skillStaffPeriod1 } };
            IDictionary<ISkill, ISkillStaffPeriodDictionary> dic = new Dictionary<ISkill, ISkillStaffPeriodDictionary> { { _skill, skillStaffPeriodDictionary } };
            var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

            using (_mocks.Record())
            {
                Expect.Call(visualLayerCollection.Period()).Return(dateTimePeriod).Repeat.AtLeastOnce();
                
                Expect.Call(_nonBlendImpactOnPeriodForProjection.CalculatePeriod(skillStaffPeriod1, visualLayerCollection, _activity)).
                    Return(0).Repeat.AtLeastOnce();
				
				Expect.Call(_workShiftCalculator.CalculateShiftValueForPeriod(0, 0, 0, 0)).IgnoreArguments().Return(0).Repeat.AtLeastOnce();
            }

			IWorkShiftCalculationResultHolder result;

			using (_mocks.Playback())
            {
				result = _target.CalculateShiftValue(_person, visualLayerCollection, dic, WorkShiftLengthHintOption.Free, false, false);
            }
            Assert.That(result.Value, Is.EqualTo(0));
        }
		[Test]
		public void ShouldReturnZeroIfLayerPeriodIsNull()
		{
			var dateTime = new DateTime(2010, 10, 10, 8, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(dateTime, dateTime.AddHours(1));
			ISkillDay skillDay = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(dateTime));
			ISkillStaffPeriod skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill, dateTime, 60, 20, 20);

			skillStaffPeriod1.SetSkillDay(skillDay);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(_skill)
			                                                             {{dateTimePeriod, skillStaffPeriod1}};
		    IDictionary<ISkill, ISkillStaffPeriodDictionary> dic = new Dictionary<ISkill, ISkillStaffPeriodDictionary>
			                                                           {{_skill, skillStaffPeriodDictionary}};
		    var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

			using (_mocks.Record())
			{
				Expect.Call(visualLayerCollection.Period()).Return(null).Repeat.AtLeastOnce();
			}

			IWorkShiftCalculationResultHolder result;

			using (_mocks.Playback())
			{
				result = _target.CalculateShiftValue(_person, visualLayerCollection, dic, WorkShiftLengthHintOption.Free, false, false);
			}
			Assert.That(result.Value,Is.EqualTo(0));
		}

		[Test]
		public void ShouldReturnZeroIfLayersDoNotIntersect()
		{
			var dateTime = new DateTime(2010, 10, 10, 8, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(dateTime, dateTime.AddHours(1));
			ISkillDay skillDay = SkillDayFactory.CreateSkillDay(_skill, new DateOnly(dateTime));
			ISkillStaffPeriod skillStaffPeriod1 = SkillStaffPeriodFactory.CreateSkillStaffPeriod(_skill, dateTime, 60, 20, 20);

			skillStaffPeriod1.SetSkillDay(skillDay);
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary = new SkillStaffPeriodDictionary(_skill)
			                                                             {{dateTimePeriod, skillStaffPeriod1}};
		    IDictionary<ISkill, ISkillStaffPeriodDictionary> dic = new Dictionary<ISkill, ISkillStaffPeriodDictionary>
			                                                           {{_skill, skillStaffPeriodDictionary}};
		    var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();

			using (_mocks.Record())
			{
				Expect.Call(visualLayerCollection.Period()).Return(dateTimePeriod.MovePeriod(TimeSpan.FromHours(1))).Repeat.AtLeastOnce();
			}

			IWorkShiftCalculationResultHolder result;

			using (_mocks.Playback())
			{
				result = _target.CalculateShiftValue(_person, visualLayerCollection, dic, WorkShiftLengthHintOption.Free, false, false);
			}
			Assert.That(result.Value, Is.EqualTo(0));
		}
	}
}