using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Ccc.Domain.Security.Principal;
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
		private IPersonSkillProvider _personSkillProvider;
		private IPeriodDistributionService _periodDistributionService;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_occupiedSeatCalculator = _mocks.StrictMock<IOccupiedSeatCalculator>();
			_personSkillProvider = _mocks.DynamicMock<IPersonSkillProvider>();
			_periodDistributionService = _mocks.DynamicMock<IPeriodDistributionService>();
			_target = new ResourceOptimizationHelper(_stateHolder, _occupiedSeatCalculator,
													 new NonBlendSkillCalculator(),
														 _personSkillProvider, _periodDistributionService,
														 new CurrentTeleoptiPrincipal());
		}

		private void expectsForVerifyCalculateDay(ISkill skill1, ISkillStaffPeriodHolder skillStaffPeriodHolder,
												  IScheduleDictionary dictionary, IScheduleExtractor extractor,
												  DateTimePeriod period, IList<ISkill> skills,
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
			Expect.Call(skill1.DefaultResolution).Return(15).Repeat.AtLeastOnce();
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
				expectsForVerifyCalculateDay(skill1, skillStaffPeriodHolder, dictionary, extractor, period, skills, service1,
											 visualLayerCollection, service2, service3, skillStaffPeriodDictionary);
				Expect.Call(skill1.SkillType).Return(skillType).Repeat.AtLeastOnce();
				Expect.Call(skillType.ForecastSource).Return(ForecastSource.Email).Repeat.AtLeastOnce();
				Expect.Call(() => _occupiedSeatCalculator.Calculate(new DateOnly(), null, skillStaffPeriodDictionary)).IgnoreArguments();
				Expect.Call(_stateHolder.TeamLeaderMode).Return(false).Repeat.Any();
				Expect.Call(()=> _periodDistributionService.CalculateDay(null, skillStaffPeriodDictionary)).IgnoreArguments();
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
		public void ShouldNotCrashIfNoSkills()
		{
			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.SkipResourceCalculation).Return(false).Repeat.Any();
				Expect.Call(_stateHolder.TeamLeaderMode).Return(false).Repeat.Any();
				Expect.Call(_stateHolder.Skills).Return(new List<ISkill>());
			}
			using (_mocks.Playback())
			{
				_target.ResourceCalculateDate(new DateOnly(2009, 2, 2), true, true);
			}
		}
	}
}
