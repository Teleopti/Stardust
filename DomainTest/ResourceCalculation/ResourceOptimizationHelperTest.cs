using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	[TestFixture]
	public class ResourceOptimizationHelperTest
	{
		private IResourceOptimizationHelper _target;
		private ISchedulingResultStateHolder _stateHolder;
		private MockRepository _mocks;
		private IOccupiedSeatCalculator _occupiedSeatCalculator;
	    private SingleSkillMaxSeatCalculator _singleSkillMaxSeatCalculator;
		private IPersonSkillProvider _personSkillProvider;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_occupiedSeatCalculator = _mocks.StrictMock<IOccupiedSeatCalculator>();
			_personSkillProvider = _mocks.DynamicMock<IPersonSkillProvider>();
	        _singleSkillMaxSeatCalculator = new SingleSkillMaxSeatCalculator(_personSkillProvider);
			_target = new ResourceOptimizationHelper(_stateHolder, _occupiedSeatCalculator,
													 new NonBlendSkillCalculator(
														 new NonBlendSkillImpactOnPeriodForProjection()),
														 new SingleSkillDictionary(),
														 _singleSkillMaxSeatCalculator,
														 _personSkillProvider);
		}

		private void expectsForVerifyCalculateDay(ISkill skill1, ISkillStaffPeriodHolder skillStaffPeriodHolder,
												  IScheduleDictionary dictionary, IScheduleExtractor extractor,
												  DateTimePeriod period, IList<ISkill> skills, IActivity activity,
												  IProjectionService service1,
												  IVisualLayerCollection visualLayerCollection,
												  IProjectionService service2, IProjectionService service3,
												  ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriodDictionary)
		{
			Expect.Call(_stateHolder.Schedules).Return(dictionary);
			dictionary.ExtractAllScheduleData(extractor, period);
			LastCall.IgnoreArguments();

			Expect.Call(_stateHolder.Skills).Return(skills).Repeat.AtLeastOnce();
			Expect.Call(_stateHolder.SkillStaffPeriodHolder).Return(skillStaffPeriodHolder).Repeat.
				AtLeastOnce();
			Expect.Call(skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary).Return(skillStaffPeriodDictionary).
				Repeat.
				AtLeastOnce();
           
			LastCall.IgnoreArguments();
			Expect.Call(skill1.DefaultResolution).Return(15);
			Expect.Call(skill1.Activity).Return(activity).Repeat.AtLeastOnce();
			Expect.Call(service1.CreateProjection()).Return(visualLayerCollection).Repeat.Any();
			Expect.Call(service2.CreateProjection()).Return(visualLayerCollection).Repeat.Any();
			Expect.Call(service3.CreateProjection()).Return(visualLayerCollection).Repeat.Any();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyCalculateDay()
		{
			var dictionary = _mocks.StrictMock<IScheduleDictionary>();
			IScheduleExtractor extractor = new ScheduleProjectionExtractor(_personSkillProvider, 15);
			var date = new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(date, date.AddDays(1));

			IActivity activity = ActivityFactory.CreateActivity("sdf");
			var skill1 = _mocks.StrictMock<ISkill>();
			IList<ISkill> skills = new List<ISkill> { skill1 };

			var service1 = _mocks.StrictMock<IProjectionService>();
			var service2 = _mocks.StrictMock<IProjectionService>();
			var service3 = _mocks.StrictMock<IProjectionService>();

			IList<IVisualLayer> layers = new List<IVisualLayer>();
			IVisualLayerCollection visualLayerCollection = new VisualLayerCollection(null, layers, new ProjectionPayloadMerger());

			var skillStaffPeriodHolder = _mocks.StrictMock<ISkillStaffPeriodHolder>();
			ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriodDictionary =
				new SkillSkillStaffPeriodExtendedDictionary();
			var skillStaffPeriods = new SkillStaffPeriodDictionary(skill1);
			skillStaffPeriodDictionary.Add(skill1, skillStaffPeriods);
			var skillType = _mocks.StrictMock<ISkillType>();

			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.SkipResourceCalculation).Return(false).Repeat.Any();
				expectsForVerifyCalculateDay(skill1, skillStaffPeriodHolder, dictionary, extractor, period, skills,
											 activity, service1,
											 visualLayerCollection, service2, service3, skillStaffPeriodDictionary);
				Expect.Call(skill1.SkillType).Return(skillType).Repeat.AtLeastOnce();
				Expect.Call(skillType.ForecastSource).Return(ForecastSource.Email).Repeat.AtLeastOnce();
				Expect.Call(() => _occupiedSeatCalculator.Calculate(new DateOnly(), _mocks.DynamicMock<IResourceCalculationDataContainer>(), new SkillSkillStaffPeriodExtendedDictionary())).IgnoreArguments();
				Expect.Call(_stateHolder.TeamLeaderMode).Return(false).Repeat.Any();

			}

			using (_mocks.Playback())
			{
				_target.ResourceCalculateDate(new DateOnly(2009, 2, 2), true, true);
			}
		}

		[Test]
		public void VerifyCalculateDayWithSkipResourceCalculation()
		{
			var skill1 = _mocks.StrictMock<ISkill>();
			ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriodDictionary =
				new SkillSkillStaffPeriodExtendedDictionary();
			var skillStaffPeriods = new SkillStaffPeriodDictionary(skill1);
			skillStaffPeriodDictionary.Add(skill1, skillStaffPeriods);

			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.SkipResourceCalculation).Return(true).Repeat.Any();
				Expect.Call(_stateHolder.TeamLeaderMode).Return(false).Repeat.Any();
			}
			using (_mocks.Playback())
			{
				_target.ResourceCalculateDate(new DateOnly(2009, 2, 2), true, true);
			}
		}

		[Test]
		public void VerifyCreate()
		{
			Assert.IsNotNull(_target);
		}

		[Test]
		public void ShouldCreateSkillStaffPeriodsJustForListOffSkills()
		{
			var keyPeriod = new DateTimePeriod(2011, 2, 1, 2011, 2, 2);

			var skill1 = _mocks.StrictMock<ISkill>();
			var skill2 = _mocks.StrictMock<ISkill>();
			IList<ISkill> skills = new List<ISkill> { skill1 };

			var skillStaffPeriods = new SkillStaffPeriodDictionary(skill1);
			var skillStaffPeriods2 = new SkillStaffPeriodDictionary(skill2);

			var skillStaffPeriodDictionary = new SkillSkillStaffPeriodExtendedDictionary
                                                 {
                                                     {skill1, skillStaffPeriods},
                                                     {skill2, skillStaffPeriods2}
                                                 };
			_mocks.ReplayAll();

			var result = _target.CreateSkillSkillStaffDictionaryOnSkills(skillStaffPeriodDictionary, skills, keyPeriod);
			Assert.That(result.Count, Is.EqualTo(1));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnSkillStaffPeriodsJustForKeyPeriod()
		{
			var keyPeriod = new DateTimePeriod(2011, 2, 1, 2011, 2, 2);

			var skill1 = _mocks.StrictMock<ISkill>();
			IList<ISkill> skills = new List<ISkill> { skill1 };

			var skillStaffPeriod1 = _mocks.StrictMock<ISkillStaffPeriod>();
			var skillStaffPeriod2 = _mocks.StrictMock<ISkillStaffPeriod>();

			Expect.Call(skillStaffPeriod1.Period).Return(keyPeriod).Repeat.AtLeastOnce();
			Expect.Call(skillStaffPeriod2.Period).Return(keyPeriod.MovePeriod(new TimeSpan(2, 0, 0, 0))).Repeat.AtLeastOnce();

			_mocks.ReplayAll();
			var skillStaffPeriods = new SkillStaffPeriodDictionary(skill1) { skillStaffPeriod1, skillStaffPeriod2 };
			var skillStaffPeriodDictionary = new SkillSkillStaffPeriodExtendedDictionary
                                                 {
                                                     {skill1, skillStaffPeriods},
                                                    
                                                 };
			var result = _target.CreateSkillSkillStaffDictionaryOnSkills(skillStaffPeriodDictionary, skills, keyPeriod);
			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(result[skill1].Count, Is.EqualTo(1));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotAddResourceWhenVirtualPeriodIsNotValid()
		{
			var mainShift = _mocks.StrictMock<IMainShift>();
			var virtualPeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var dateOnly = new DateOnly(2011, 2, 1);
			var person = _mocks.StrictMock<IPerson>();
			Expect.Call(person.VirtualSchedulePeriod(dateOnly)).Return(virtualPeriod);
			Expect.Call(virtualPeriod.IsValid).Return(false);
			_mocks.ReplayAll();
			_target.AddResourcesToNonBlendAndMaxSeat(mainShift, person, dateOnly);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldOnlyCalculateOnePersonAndAddResources()
		{
			var skill = _mocks.StrictMock<ISkill>();
			var personSkill = _mocks.StrictMock<IPersonSkill>();
			var virtualPeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			var personPeriod = _mocks.StrictMock<IPersonPeriod>();
			var person = _mocks.StrictMock<IPerson>();
			var mainShift = _mocks.StrictMock<IMainShift>();
			var dateOnly = new DateOnly(2011, 2, 1);
			var timePeriod = new DateTimePeriod(2011, 2, 1, 2011, 2, 2);
			var layer = _mocks.StrictMock<IVisualLayer>();
			var layerCollection = new VisualLayerCollection(person, new List<IVisualLayer> { layer }, new ProjectionPayloadMerger());
			var projService = _mocks.StrictMock<IProjectionService>();
			var skillStaffPeriodHolder = _mocks.StrictMock<ISkillStaffPeriodHolder>();

			Expect.Call(person.VirtualSchedulePeriod(dateOnly)).Return(virtualPeriod);
			Expect.Call(virtualPeriod.IsValid).Return(true);
			Expect.Call(mainShift.ProjectionService()).Return(projService);
			Expect.Call(projService.CreateProjection()).Return(layerCollection);
			Expect.Call(layer.Period).Return(timePeriod);
			Expect.Call(layer.EntityClone()).Return(layer);
			Expect.Call(person.Period(dateOnly)).Return(personPeriod);
			//Expect.Call(virtualPeriod.PersonPeriod).Return(personPeriod);
			Expect.Call(personPeriod.PersonNonBlendSkillCollection).Return(new List<IPersonSkill> { personSkill });
			Expect.Call(personPeriod.PersonMaxSeatSkillCollection).Return(new List<IPersonSkill> { personSkill });
			Expect.Call(personSkill.Skill).Return(skill).Repeat.Twice();
			Expect.Call(_stateHolder.SkillStaffPeriodHolder).Return(skillStaffPeriodHolder).Repeat.Twice();
			Expect.Call(skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary).Return(
				new SkillSkillStaffPeriodExtendedDictionary()).Repeat.Twice();
			Expect.Call(() => _occupiedSeatCalculator.Calculate(new DateOnly(), _mocks.DynamicMock<IResourceCalculationDataContainer>(),
														  new SkillSkillStaffPeriodExtendedDictionary())).
				IgnoreArguments();
			_mocks.ReplayAll();
			_target.AddResourcesToNonBlendAndMaxSeat(mainShift, person, dateOnly);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldNotUseSingleSkillCalculationOnNoInputDays()
		{
			var singleSkillDictionary = _mocks.StrictMock<ISingleSkillDictionary>();
			var helper = new ResourceOptimizationHelper(_stateHolder, _occupiedSeatCalculator, new NonBlendSkillCalculator(new NonBlendSkillImpactOnPeriodForProjection()), singleSkillDictionary, _singleSkillMaxSeatCalculator, _personSkillProvider);
			var toRemove = new List<IScheduleDay>();
			var toAdd = new List<IScheduleDay>();

			var res = helper.UseSingleSkillCalculations(toRemove, toAdd);
			Assert.IsFalse(res);
		}

		[Test]
		public void ShouldNotUseSingleSkillCalculationsWhenToRemoveIsNotSingleSkilled()
		{
			var singleSkillDictionary = _mocks.StrictMock<ISingleSkillDictionary>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var helper = new ResourceOptimizationHelper(_stateHolder, _occupiedSeatCalculator, new NonBlendSkillCalculator(new NonBlendSkillImpactOnPeriodForProjection()), singleSkillDictionary, _singleSkillMaxSeatCalculator, _personSkillProvider);
			var toRemove = new List<IScheduleDay> { scheduleDay };
			var toAdd = new List<IScheduleDay>();
			var person = _mocks.StrictMock<IPerson>();
			var dateOnlyTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
			var dateOnly = new DateOnly();

			using (_mocks.Record())
			{
				Expect.Call(scheduleDay.Person).Return(person);
				Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(dateOnlyTimePeriod);
				Expect.Call(dateOnlyTimePeriod.DateOnly).Return(dateOnly);
				Expect.Call(singleSkillDictionary.IsSingleSkill(person, dateOnly)).Return(false);
			}

			using (_mocks.Playback())
			{
				var res = helper.UseSingleSkillCalculations(toRemove, toAdd);
				Assert.IsFalse(res);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "AddIs"), Test]
		public void ShouldNotUseSingleSkillCalculationsWhenToAddIsNotSingleSkilled()
		{
			var singleSkillDictionary = _mocks.StrictMock<ISingleSkillDictionary>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var helper = new ResourceOptimizationHelper(_stateHolder, _occupiedSeatCalculator, new NonBlendSkillCalculator(new NonBlendSkillImpactOnPeriodForProjection()), singleSkillDictionary, _singleSkillMaxSeatCalculator, _personSkillProvider);
			var toAdd = new List<IScheduleDay> { scheduleDay };
			var toRemove = new List<IScheduleDay>();
			var person = _mocks.StrictMock<IPerson>();
			var dateOnlyTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
			var dateOnly = new DateOnly();

			using (_mocks.Record())
			{
				Expect.Call(scheduleDay.Person).Return(person);
				Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(dateOnlyTimePeriod);
				Expect.Call(dateOnlyTimePeriod.DateOnly).Return(dateOnly);
				Expect.Call(singleSkillDictionary.IsSingleSkill(person, dateOnly)).Return(false);
			}

			using (_mocks.Playback())
			{
				var res = helper.UseSingleSkillCalculations(toRemove, toAdd);
				Assert.IsFalse(res);
			}
		}

		[Test]
		public void ShouldUseSingleSkillCalculationsWhenToAddAndToRemoveIsSingleSkilled()
		{
			var singleSkillDictionary = _mocks.StrictMock<ISingleSkillDictionary>();
			var scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
			var scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
			var helper = new ResourceOptimizationHelper(_stateHolder, _occupiedSeatCalculator, new NonBlendSkillCalculator(new NonBlendSkillImpactOnPeriodForProjection()), singleSkillDictionary, _singleSkillMaxSeatCalculator, _personSkillProvider);
			var toAdd = new List<IScheduleDay> { scheduleDay1 };
			var toRemove = new List<IScheduleDay> { scheduleDay2 };
			var person = _mocks.StrictMock<IPerson>();
			var dateOnlyTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
			var dateOnly = new DateOnly();

			using (_mocks.Record())
			{
				Expect.Call(scheduleDay1.Person).Return(person);
				Expect.Call(scheduleDay1.DateOnlyAsPeriod).Return(dateOnlyTimePeriod);
				Expect.Call(scheduleDay2.Person).Return(person);
				Expect.Call(scheduleDay2.DateOnlyAsPeriod).Return(dateOnlyTimePeriod);

				Expect.Call(dateOnlyTimePeriod.DateOnly).Return(dateOnly).Repeat.Twice();
				Expect.Call(singleSkillDictionary.IsSingleSkill(person, dateOnly)).Return(true).Repeat.Twice();
			}

			using (_mocks.Playback())
			{
				var res = helper.UseSingleSkillCalculations(toRemove, toAdd);
				Assert.IsTrue(res);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldNotUseRegularOccupiedSeatCalculatorWhenToAddOrRemove()
		{
			var skillStaffPeriodHolder = _mocks.StrictMock<ISkillStaffPeriodHolder>();
			var scheduleDay = _mocks.StrictMock<IScheduleDay>();
			var person = PersonFactory.CreatePerson();
			person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.Local));
			var dateOnlyAsDateTimeperiod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
			var pa = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, new DateTimePeriod(2009, 2, 2, 2009, 2, 2));
			var singleSkillDictionary = _mocks.StrictMock<ISingleSkillDictionary>();
			_target = new ResourceOptimizationHelper(_stateHolder, _occupiedSeatCalculator,new NonBlendSkillCalculator(new NonBlendSkillImpactOnPeriodForProjection()),singleSkillDictionary, _singleSkillMaxSeatCalculator, _personSkillProvider);

			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.SkipResourceCalculation).Return(false).Repeat.Any();
				Expect.Call(_stateHolder.TeamLeaderMode).Return(false).Repeat.Any();
				Expect.Call(scheduleDay.Person).Return(person);
				Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsDateTimeperiod);
				Expect.Call(dateOnlyAsDateTimeperiod.DateOnly).Return(new DateOnly());
				Expect.Call(scheduleDay.AssignmentHighZOrder()).Return(pa);
				Expect.Call(singleSkillDictionary.IsSingleSkill(person, new DateOnly())).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_stateHolder.Skills).Return(new List<ISkill>()).Repeat.AtLeastOnce();
				Expect.Call(_stateHolder.SkillStaffPeriodHolder).Return(skillStaffPeriodHolder).Repeat.AtLeastOnce();
				Expect.Call(skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary).Return(new SkillSkillStaffPeriodExtendedDictionary()).Repeat.AtLeastOnce();	
			}

			using (_mocks.Playback())
			{
				_target.ResourceCalculateDate(new DateOnly(2009, 2, 2), true, true, new List<IScheduleDay>{scheduleDay}, new List<IScheduleDay>());
			}
		}
	}
}
