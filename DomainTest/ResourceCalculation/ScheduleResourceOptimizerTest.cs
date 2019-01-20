using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Secrets.Furness;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class ScheduleResourceOptimizerTest
    {
        private IDividedActivityData _optimizedDivideActivity;
        private PersonAssignmentListContainer _personAssignmentListContainer;
        private FurnessEvaluator _furnessEvaluator;
        private ScheduleResourceOptimizer _target;
        private DateTimePeriod _inPeriod;
        private ISkillSkillStaffPeriodExtendedDictionary _skillStaffPeriods;
        private DateTime _startTime;
        private IAffectedPersonSkillService _personSkillService;
        private IActivityDivider _activityDivider;
	    private IPersonSkillProvider _personSkillProvider;
		private IResourceCalculationDataContainerWithSingleOperation _resources;

	    [SetUp]
        public void Setup()
        {
            _startTime = new DateTime(2008, 1, 2, 10, 00, 00, DateTimeKind.Utc);
            _inPeriod = DateTimeFactory.CreateDateTimePeriod(_startTime, new DateTime(2008, 1, 2, 10, 15, 00, DateTimeKind.Utc));
            _personAssignmentListContainer = PersonAssignmentFactory.CreatePersonAssignmentListForActivityDividerTest();
            _personSkillService = new AffectedPersonSkillService(_personAssignmentListContainer.AllSkills);
            _skillStaffPeriods = SkillDayFactory.CreateSkillDaysForActivityDividerTest(_personAssignmentListContainer.ContainedSkills);
            _activityDivider = new ActivityDivider();
			_personSkillProvider = new PersonSkillProvider();

			_resources = new ResourceCalculationDataContainer(Enumerable.Empty<ExternalStaff>(), _personSkillProvider, 15, false, new ActivityDivider());
			var layers = _personAssignmentListContainer.TestVisualLayerCollection();
			foreach (var layer in layers)
			{
				foreach (var resourceLayer in layer.Item1.ToResourceLayers(15, TimeZoneInfo.Utc))
				{
					_resources.AddResources(layer.Item2, new DateOnly(2008, 1, 1), resourceLayer);
				}
			}

            _target = new ScheduleResourceOptimizer(_resources, new SkillResourceCalculationPeriodWrapper(_skillStaffPeriods), _personSkillService, true, _activityDivider);
        }

        [Test]
        public void VerifyResourceOptimizingOfPhoneActivity()
        {
            var activityDivider = new ActivityDivider();
            IDividedActivityData dividedActivityData =
                activityDivider.DivideActivity(new SkillResourceCalculationPeriodWrapper(_skillStaffPeriods), _personSkillService.AffectedSkills.ToLookup(s => s.Activity), _personAssignmentListContainer.ContainedActivities["Phone"], _resources, _inPeriod);
            var furnessDataConverter = new FurnessDataConverter(dividedActivityData);
            IFurnessData furnessData = furnessDataConverter.ConvertDividedActivityToFurnessData();
            _furnessEvaluator = new FurnessEvaluator(furnessData);
			_furnessEvaluator.Evaluate(1, 8, Variances.StandardDeviation);
            _optimizedDivideActivity = furnessDataConverter.ConvertFurnessDataBackToActivity();

			var person1 =
				_personSkillProvider.SkillsOnPersonDate(_personAssignmentListContainer.ContainedPersons["Person1"], new DateOnly(2008, 1, 1))
									.ForActivity(_personAssignmentListContainer.ContainedActivities["Phone"].Id.GetValueOrDefault()).MergedKey();
            
            // Iteration
            Assert.AreEqual(8, _furnessEvaluator.InnerIteration);

            // WeightedRelativeKeyedSkillResourceResources
            KeyedSkillResourceDictionary resourceMatrix = _optimizedDivideActivity.WeightedRelativeKeyedSkillResourceResources;
            Assert.IsNotNull(resourceMatrix);
            Assert.AreEqual(2, resourceMatrix.Count);
            Assert.AreEqual(2, resourceMatrix[person1].Count);

            Assert.AreEqual(15, _optimizedDivideActivity.WeightedRelativePersonSkillResourcesSum[_personAssignmentListContainer.ContainedSkills["PhoneA"]], 0.1);
            Assert.AreEqual(20, _optimizedDivideActivity.WeightedRelativePersonSkillResourcesSum[_personAssignmentListContainer.ContainedSkills["PhoneB"]], 0.1);
        }

        [Test]
        public void VerifyResourceOptimizing()
        {
			_target.Optimize(_inPeriod);
            ISkillSkillStaffPeriodExtendedDictionary resultSkillStaffPeriods = _skillStaffPeriods;
            double totalMinutes = _inPeriod.ElapsedTime().TotalMinutes;
            Assert.AreEqual(15/totalMinutes,
                            resultSkillStaffPeriods[_personAssignmentListContainer.ContainedSkills["PhoneA"]].First(
                                k => k.Key.StartDateTime == _startTime).Value.Payload.CalculatedResource, 0.1);
            Assert.AreEqual(19.95/totalMinutes,
                            resultSkillStaffPeriods[_personAssignmentListContainer.ContainedSkills["PhoneB"]].First(
                                k => k.Key.StartDateTime == _startTime).Value.Payload.CalculatedResource, 0.1);
        }

        [Test]
        public void VerifyResourceOptimizingWithOccupancyAdjustment()
        {
			_target.Optimize(_inPeriod);

            Assert.AreEqual(1, _skillStaffPeriods[_personAssignmentListContainer.ContainedSkills["PhoneA"]].First(
                    s => s.Key.StartDateTime == _startTime).Value.Payload.CalculatedResource, 0.001);
            Assert.AreEqual(1.333, _skillStaffPeriods[_personAssignmentListContainer.ContainedSkills["PhoneB"]].First(
                                s => s.Key.StartDateTime == _startTime).Value.Payload.CalculatedResource, 0.001);
        }
    }
}
