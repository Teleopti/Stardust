using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Secrets.Furness;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

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
        private MockRepository _mocks;
        private IActivityDivider _activityDivider;
	    private IPersonSkillProvider _personSkillProvider;
		private IResourceCalculationDataContainerWithSingleOperation _resources;

	    [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _startTime = new DateTime(2008, 1, 2, 10, 00, 00, DateTimeKind.Utc);
            _inPeriod = DateTimeFactory.CreateDateTimePeriod(_startTime, new DateTime(2008, 1, 2, 10, 15, 00, DateTimeKind.Utc));
            DateOnlyPeriod datePeriod = new DateOnlyPeriod(new DateOnly(_inPeriod.StartDateTime), new DateOnly(_inPeriod.EndDateTime));
            _personAssignmentListContainer = PersonAssignmentFactory.CreatePersonAssignmentListForActivityDividerTest();
            _personSkillService = new AffectedPersonSkillService(datePeriod, _personAssignmentListContainer.AllSkills);
            _skillStaffPeriods = SkillDayFactory.CreateSkillDaysForActivityDividerTest(_personAssignmentListContainer.ContainedSkills);
            _activityDivider = _mocks.StrictMock<IActivityDivider>();
			_personSkillProvider = new PersonSkillProvider();

			_resources = new ResourceCalculationDataContainer(_personSkillProvider, 15);
			var layers = _personAssignmentListContainer.TestVisualLayerCollection();
			foreach (var layer in layers)
			{
				foreach (var resourceLayer in layer.ToResourceLayers(15))
				{
					_resources.AddResources(layer.Person, new DateOnly(2008, 1, 1), resourceLayer);
				}
			}

            _target = new ScheduleResourceOptimizer(_resources, _skillStaffPeriods, _personSkillService, true, _activityDivider);
        }

        [Test]
        public void VerifyResourceOptimizingOfPhoneActivity()
        {
            var activityDivider = new ActivityDivider();
            IDividedActivityData dividedActivityData =
                activityDivider.DivideActivity(_skillStaffPeriods, _personSkillService, _personAssignmentListContainer.ContainedActivities["Phone"], _resources, _inPeriod);
            var furnessDataConverter = new FurnessDataConverter(dividedActivityData);
            IFurnessData furnessData = furnessDataConverter.ConvertDividedActivityToFurnessData();
            _furnessEvaluator = new FurnessEvaluator(furnessData);
			_furnessEvaluator.Evaluate(1, 8, Variances.StandardDeviation);
            _optimizedDivideActivity = furnessDataConverter.ConvertFurnessDataBackToActivity();

			var person1 =
				_personSkillProvider.SkillsOnPersonDate(_personAssignmentListContainer.ContainedPersons["Person1"], new DateOnly(2008, 1, 1))
									.Key;
            
            // Iteration
            Assert.AreEqual(8, _furnessEvaluator.InnerIteration);

            // WeightedRelativeKeyedSkillResourceResources
            KeyedSkillResourceDictionary resourceMatrix = _optimizedDivideActivity.WeightedRelativeKeyedSkillResourceResources;
            Assert.IsNotNull(resourceMatrix);
            Assert.AreEqual(3, resourceMatrix.Count);
            Assert.AreEqual(4, resourceMatrix[person1].Count);

            Assert.AreEqual(12.49, _optimizedDivideActivity.WeightedRelativePersonSkillResourcesSum[_personAssignmentListContainer.ContainedSkills["PhoneA"]], 0.1);
            Assert.AreEqual(20, _optimizedDivideActivity.WeightedRelativePersonSkillResourcesSum[_personAssignmentListContainer.ContainedSkills["PhoneB"]], 0.1);
        }

        [Test]
        public void VerifyResourceOptimizing()
        {
            var activityDivider = new ActivityDivider();
            IDividedActivityData dividedActivityData =
                activityDivider.DivideActivity(_skillStaffPeriods, _personSkillService, _personAssignmentListContainer.ContainedActivities["Phone"], _resources, _inPeriod);

            Expect.Call(_activityDivider.DivideActivity(_skillStaffPeriods, _personSkillService,
                                                        _personAssignmentListContainer.ContainedActivities["Phone"],
                                                        _resources,
                                                        _inPeriod)).Return(dividedActivityData).IgnoreArguments().Repeat.AtLeastOnce();
            _mocks.ReplayAll();
            _target.Optimize(_inPeriod, false);
            ISkillSkillStaffPeriodExtendedDictionary resultSkillStaffPeriods = _skillStaffPeriods;
            double totalMinutes = _inPeriod.ElapsedTime().TotalMinutes;
            Assert.AreEqual(12.3/totalMinutes,
                            resultSkillStaffPeriods[_personAssignmentListContainer.ContainedSkills["PhoneA"]].First(
                                k => k.Key.StartDateTime == _startTime).Value.Payload.CalculatedResource, 0.1);
            Assert.AreEqual(19.95/totalMinutes,
                            resultSkillStaffPeriods[_personAssignmentListContainer.ContainedSkills["PhoneB"]].First(
                                k => k.Key.StartDateTime == _startTime).Value.Payload.CalculatedResource, 0.1);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyResourceOptimizingWithOccupancyAdjustment()
        {
            var activityDivider = new ActivityDivider();
            IDividedActivityData dividedActivityData =
                activityDivider.DivideActivity(_skillStaffPeriods, _personSkillService, _personAssignmentListContainer.ContainedActivities["Phone"], _resources, _inPeriod);

            Expect.Call(_activityDivider.DivideActivity(_skillStaffPeriods, _personSkillService,
                                                        _personAssignmentListContainer.ContainedActivities["Phone"],
                                                        _resources,
                                                        _inPeriod)).Return(dividedActivityData).IgnoreArguments().Repeat.AtLeastOnce();
            _mocks.ReplayAll();
            _target.Optimize(_inPeriod, false);

            Assert.AreEqual(0.833, _skillStaffPeriods[_personAssignmentListContainer.ContainedSkills["PhoneA"]].First(
                    s => s.Key.StartDateTime == _startTime).Value.Payload.CalculatedResource, 0.001);
            Assert.AreEqual(1.333, _skillStaffPeriods[_personAssignmentListContainer.ContainedSkills["PhoneB"]].First(
                                s => s.Key.StartDateTime == _startTime).Value.Payload.CalculatedResource, 0.001);

            _mocks.VerifyAll();

        }
    }
}
