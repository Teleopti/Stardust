﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class ActivityDividerTest
    {
        private ActivityDivider _target;
        private DateTimePeriod _inPeriod;
        private PersonAssignmentListContainer _testContainer;
        private ISkillSkillStaffPeriodExtendedDictionary _skillStaffPeriods;
	    private IPersonSkillProvider _personSkillProvider;
		private IResourceCalculationDataContainerWithSingleOperation _resources;

	    [SetUp]
        public void Setup()
        {
            _inPeriod = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 1, 2, 10, 00, 00, DateTimeKind.Utc), new DateTime(2008, 1, 2, 10, 15, 00, DateTimeKind.Utc));
            
            _testContainer = PersonAssignmentFactory.CreatePersonAssignmentListForActivityDividerTest();
            _skillStaffPeriods = SkillDayFactory.CreateSkillDaysForActivityDividerTest(_testContainer.ContainedSkills);
			_personSkillProvider = new PersonSkillProvider();

			_resources = new ResourceCalculationDataContainer(_personSkillProvider, 15);
			var layers = _testContainer.TestVisualLayerCollection();
			foreach (var layer in layers)
			{
				foreach (var resourceLayer in layer.ToResourceLayers(15))
				{
					_resources.AddResources(layer.Person, new DateOnly(2008, 1, 1), resourceLayer);
				}
			}

            _target = new ActivityDivider();
			
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyDivideActivity()
        {
            IDividedActivityData dividedActivity = 
                _target.DivideActivity(_skillStaffPeriods, new AffectedPersonSkillService(_testContainer.AllSkills), _testContainer.ContainedActivities["Phone"], _resources, _inPeriod);

            VerifyPersonSkillResourcesData(dividedActivity);

            VerifyRelativePersonResourcesData(dividedActivity);

            VerifyPersonSkillEfficienciesData(dividedActivity);

            VerifyPersonResourcesData(dividedActivity);

            VerifyTargetDemandsData(dividedActivity);

            VerifyRelativePersonSkillResourcesData(dividedActivity);

            VerifyRelativePersonSkillResourcesSumData(dividedActivity);
      
        }

		[Test]
		public void ShouldConsiderBothAgentsWithAndWithoutProficiencyScheduledDuringTheSameInterval()
		{
			var skill = SkillFactory.CreateSkillWithId("Direct Sales");
			var personPeriodStart = new DateOnly(2014, 1, 1);
			var periodToCalculate = new DateTimePeriod(new DateTime(2014, 1, 1, 7, 0, 0, DateTimeKind.Utc), new DateTime(2014, 1, 1, 7, 15, 0, DateTimeKind.Utc));
			var person1 = PersonFactory.CreatePersonWithPersonPeriod(personPeriodStart, new[] { skill });
			var person2 = PersonFactory.CreatePersonWithPersonPeriod(personPeriodStart, new[] { skill });
			person2.ChangeSkillProficiency(skill,new Percent(0.6), person2.Period(personPeriodStart));

			_resources.Clear();
			_resources.AddResources(person1,personPeriodStart,new ResourceLayer{PayloadId = skill.Activity.Id.GetValueOrDefault(),Period = periodToCalculate,Resource = 1});
			_resources.AddResources(person2,personPeriodStart,new ResourceLayer{PayloadId = skill.Activity.Id.GetValueOrDefault(),Period = periodToCalculate,Resource = 1});

			var skillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary
				{
					{skill, SkillDayFactory.PrepareSkillDay(skill, periodToCalculate.StartDateTime, 0)}
				};
			var dividedActivity = _target.DivideActivity(skillStaffPeriods,
			                       new AffectedPersonSkillService(new[] {skill}), skill.Activity, _resources,
								   periodToCalculate);

			var resourceMatrix = dividedActivity.KeyedSkillResourceEfficiencies;

			Assert.IsNotNull(resourceMatrix);
			Assert.AreEqual(0.8, resourceMatrix[skill.Id.Value.ToString()][skill], 0.001d);
		}

        [Test]
        public void VerifyNotSequentialSkillDaysWorks()
        {
            MockRepository mocks = new MockRepository();

            var period = _inPeriod.MovePeriod(TimeSpan.FromDays(-1));

            ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriods = new SkillSkillStaffPeriodExtendedDictionary();
            skillStaffPeriods.Add(_testContainer.AllSkills[0],
                          new SkillStaffPeriodDictionary(_testContainer.AllSkills[0]) { { period, SkillStaffPeriodFactory.CreateSkillStaffPeriod(period, new Task(), ServiceAgreement.DefaultValues()) } });

            _target = new ActivityDivider();

            mocks.ReplayAll();

			_target.DivideActivity(skillStaffPeriods, new AffectedPersonSkillService(_testContainer.AllSkills), _testContainer.ContainedActivities["Phone"], _resources, _inPeriod);
        }

        [Test]
        public void VerifyWithoutClosedSkillStaffPeriod()
        {
            _target = new ActivityDivider();

            IDividedActivityData result =
				_target.DivideActivity(_skillStaffPeriods, new AffectedPersonSkillService(_testContainer.AllSkills), _testContainer.ContainedActivities["Phone"], _resources, _inPeriod);
            Assert.AreEqual(3, result.TargetDemands.Count);
        }

        [Test]
        public void VerifyWithClosedSkillStaffPeriod()
        {
            ISkillStaffPeriodDictionary dictionary = _skillStaffPeriods[_testContainer.AllSkills[0]];
            dictionary.Clear();
            _target = new ActivityDivider();
            IDividedActivityData result =
				_target.DivideActivity(_skillStaffPeriods, new AffectedPersonSkillService(_testContainer.AllSkills), _testContainer.ContainedActivities["Phone"], _resources, _inPeriod);
            Assert.AreEqual(2, result.TargetDemands.Count);
        }

		/// <summary>
		/// Shoulds the handle overlapping skills.
		/// </summary>
		/// <remarks>
		/// This happens if visual layer collection has the same person
		/// </remarks>
		[Test]
		public void ShouldHandleOverlappingSkills()
		{
			DateOnlyPeriod datePeriod = new DateOnlyPeriod(new DateOnly(_inPeriod.StartDateTime), new DateOnly(_inPeriod.EndDateTime));
			ISkillStaffPeriodDictionary dictionary = _skillStaffPeriods[_testContainer.AllSkills[0]];
			dictionary.Clear();
			_target = new ActivityDivider();

			_resources.Clear();
			var layers = _testContainer.TestFilteredVisualLayerCollectionWithSamePerson();
			foreach (var layer in layers)
			{
				foreach (var resourceLayer in layer.ToResourceLayers(15))
				{
					_resources.AddResources(layer.Person, new DateOnly(2008, 1, 1), resourceLayer);
				}
			}


			IDividedActivityData result =
				_target.DivideActivity(_skillStaffPeriods, new AffectedPersonSkillService(_testContainer.AllSkills), _testContainer.ContainedActivities["Phone"], _resources, _inPeriod);
			Assert.AreEqual(2, result.TargetDemands.Count);
		}

    	private void VerifyTargetDemandsData(IDividedActivityData dividedActivity)
        {
            IDictionary<ISkill, double> targetDemands = dividedActivity.TargetDemands;
            Assert.IsNotNull(targetDemands);
            Assert.AreEqual(3, targetDemands.Count);
            Assert.AreEqual(5, targetDemands[_testContainer.ContainedSkills["PhoneA"]]);
            Assert.AreEqual(10, targetDemands[_testContainer.ContainedSkills["PhoneB"]]);
        }

	    private void VerifyPersonResourcesData(IDividedActivityData dividedActivity)
		{
			var person1 =
				_personSkillProvider.SkillsOnPersonDate(_testContainer.ContainedPersons["Person1"], new DateOnly(2008, 1, 1))
									.Key;
			var person2 =
				_personSkillProvider.SkillsOnPersonDate(_testContainer.ContainedPersons["Person2"], new DateOnly(2008, 1, 1))
									.Key;
			var person4 =
				_personSkillProvider.SkillsOnPersonDate(_testContainer.ContainedPersons["Person4"], new DateOnly(2008, 1, 1))
									.Key;

		    IDictionary<string, double> personResources = dividedActivity.PersonResources;
		    Assert.IsNotNull(personResources);
		    Assert.AreEqual(3, personResources.Count);
		    Assert.AreEqual(5d, personResources[person1], 0.0001);
		    Assert.AreEqual(15, personResources[person2]);
            Assert.AreEqual(10d, personResources[person4], 0.0001);
        }

        private void VerifyRelativePersonResourcesData(IDividedActivityData dividedActivity)
		{
			var person1 =
				_personSkillProvider.SkillsOnPersonDate(_testContainer.ContainedPersons["Person1"], new DateOnly(2008, 1, 1))
									.Key;
			var person2 =
				_personSkillProvider.SkillsOnPersonDate(_testContainer.ContainedPersons["Person2"], new DateOnly(2008, 1, 1))
									.Key;
			var person4 =
				_personSkillProvider.SkillsOnPersonDate(_testContainer.ContainedPersons["Person4"], new DateOnly(2008, 1, 1))
									.Key;

            // Person Relative Resources (TRAFF)
            IDictionary<string, double> personResources = dividedActivity.RelativePersonResources;
            Assert.IsNotNull(personResources);
            Assert.AreEqual(3, personResources.Count);
            Assert.AreEqual(0.3333, personResources[person1], 0.0001);
            Assert.AreEqual(1, personResources[person2], 0.0001);
            Assert.AreEqual(0.6666, personResources[person4], 0.0001);
        }

        private void VerifyRelativePersonSkillResourcesSumData(IDividedActivityData dividedActivity)
        {
            IDictionary<ISkill, double> personLoggedOnSumma = dividedActivity.RelativePersonSkillResourcesSum;
            Assert.IsNotNull(personLoggedOnSumma);
            Assert.AreEqual(4, personLoggedOnSumma.Count);
            Assert.AreEqual(1.3333, personLoggedOnSumma[_testContainer.ContainedSkills["PhoneA"]], 0.0001);
            Assert.AreEqual(1.9999, personLoggedOnSumma[_testContainer.ContainedSkills["PhoneB"]], 0.0001);
        }

        private void VerifyPersonSkillEfficienciesData(IDividedActivityData dividedActivity)
		{
			var person1 =
				_personSkillProvider.SkillsOnPersonDate(_testContainer.ContainedPersons["Person1"], new DateOnly(2008, 1, 1))
									.Key;
			var person2 =
				_personSkillProvider.SkillsOnPersonDate(_testContainer.ContainedPersons["Person2"], new DateOnly(2008, 1, 1))
									.Key;
			var person4 =
				_personSkillProvider.SkillsOnPersonDate(_testContainer.ContainedPersons["Person4"], new DateOnly(2008, 1, 1))
									.Key;

            KeyedSkillResourceDictionary skillResourceEfficiencyMatrix = dividedActivity.KeyedSkillResourceEfficiencies;
            Assert.IsNotNull(skillResourceEfficiencyMatrix);
            Assert.AreEqual(3, skillResourceEfficiencyMatrix.Count);
            Assert.AreEqual(4, skillResourceEfficiencyMatrix[person1].Count);
            Assert.AreEqual(2, skillResourceEfficiencyMatrix[person1][_testContainer.ContainedSkills["PhoneA"]]);
            Assert.AreEqual(1, skillResourceEfficiencyMatrix[person1][_testContainer.ContainedSkills["PhoneB"]]);
            Assert.AreEqual(2, skillResourceEfficiencyMatrix[person2].Count);
            Assert.AreEqual(1, skillResourceEfficiencyMatrix[person2][_testContainer.ContainedSkills["PhoneA"]]);
            Assert.AreEqual(1, skillResourceEfficiencyMatrix[person2][_testContainer.ContainedSkills["PhoneB"]]);
            Assert.AreEqual(1, skillResourceEfficiencyMatrix[person4].Count);
            Assert.AreEqual(1, skillResourceEfficiencyMatrix[person4][_testContainer.ContainedSkills["PhoneB"]]);
        }

        private void VerifyRelativePersonSkillResourcesData(IDividedActivityData dividedActivity)
		{
			var person1 =
				_personSkillProvider.SkillsOnPersonDate(_testContainer.ContainedPersons["Person1"], new DateOnly(2008, 1, 1))
									.Key;
			var person2 =
				_personSkillProvider.SkillsOnPersonDate(_testContainer.ContainedPersons["Person2"], new DateOnly(2008, 1, 1))
									.Key;
			var person4 =
				_personSkillProvider.SkillsOnPersonDate(_testContainer.ContainedPersons["Person4"], new DateOnly(2008, 1, 1))
									.Key;

            KeyedSkillResourceDictionary resourceMatrix = dividedActivity.RelativeKeyedSkillResourceResources;
            Assert.IsNotNull(resourceMatrix);
            Assert.AreEqual(3, resourceMatrix.Count);
            Assert.AreEqual(4, resourceMatrix[person1].Count);
            Assert.AreEqual(0.3333, resourceMatrix[person1][_testContainer.ContainedSkills["PhoneA"]], 0.001d);
            Assert.AreEqual(0.3333, resourceMatrix[person1][_testContainer.ContainedSkills["PhoneB"]], 0.001d);
            Assert.AreEqual(2, resourceMatrix[person2].Count);
            Assert.AreEqual(1, resourceMatrix[person2][_testContainer.ContainedSkills["PhoneA"]], 0.001d);
            Assert.AreEqual(1, resourceMatrix[person2][_testContainer.ContainedSkills["PhoneB"]], 0.001d);
            Assert.AreEqual(1, resourceMatrix[person4].Count);
            Assert.AreEqual(0.6666, resourceMatrix[person4][_testContainer.ContainedSkills["PhoneB"]], 0.001d);
        }

        private void VerifyPersonSkillResourcesData(IDividedActivityData dividedActivity)
        {
	        var person1 =
		        _personSkillProvider.SkillsOnPersonDate(_testContainer.ContainedPersons["Person1"], new DateOnly(2008, 1, 1))
		                            .Key;
	        var person2 =
		        _personSkillProvider.SkillsOnPersonDate(_testContainer.ContainedPersons["Person2"], new DateOnly(2008, 1, 1))
		                            .Key;
			var person4 =
		        _personSkillProvider.SkillsOnPersonDate(_testContainer.ContainedPersons["Person4"], new DateOnly(2008, 1, 1))
		                            .Key;

            KeyedSkillResourceDictionary resourceMatrix = dividedActivity.WeightedRelativeKeyedSkillResourceResources;
            Assert.IsNotNull(resourceMatrix);
            Assert.AreEqual(3, resourceMatrix.Count);
            Assert.AreEqual(4, resourceMatrix[person1].Count);
            Assert.AreEqual(0.3333, resourceMatrix[person1][_testContainer.ContainedSkills["PhoneA"]], 0.001d);
            Assert.AreEqual(0.3333, resourceMatrix[person1][_testContainer.ContainedSkills["PhoneB"]], 0.001d);
            Assert.AreEqual(2, resourceMatrix[person2].Count);
            Assert.AreEqual(1, resourceMatrix[person2][_testContainer.ContainedSkills["PhoneA"]], 0.001d);
            Assert.AreEqual(1, resourceMatrix[person2][_testContainer.ContainedSkills["PhoneB"]], 0.001d);
            Assert.AreEqual(1, resourceMatrix[person4].Count);
            Assert.AreEqual(0.6666, resourceMatrix[person4][_testContainer.ContainedSkills["PhoneB"]], 0.001d);
        }
    }
}
