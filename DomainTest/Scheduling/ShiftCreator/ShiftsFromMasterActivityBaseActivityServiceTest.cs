using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
	[DomainTest]
	public class ShiftsFromMasterActivityBaseActivityServiceTest
	{
		public IShiftFromMasterActivityService Target;
		public IShiftCreatorService ShiftCreatorService;

		[Test]
		public void ShouldRemoveAdditionalLayersWithSamePayloadAsBaseLayer()
		{
			var phoneActivity = new Activity("Phone") { InContractTime = true, RequiresSkill = true }.WithId();
			var emailActivity = new Activity("Email") { InContractTime = true, RequiresSkill = true }.WithId();
			var lunchActivity = new Activity("Lunch") { InContractTime = false, RequiresSkill = false }.WithId();
			var masterActivity = new MasterActivity();
			masterActivity.UpdateActivityCollection(new List<IActivity> { phoneActivity, emailActivity });
			var ruleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(masterActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15),
					new TimePeriodWithSegment(11, 0, 11, 0, 15), new ShiftCategory("_")));
			ruleSet.AddExtender(new ActivityAbsoluteStartExtender(lunchActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15),
				new TimePeriodWithSegment(9, 0, 9, 0, 15)));
			var shiftCollectionFromSpecificStart = ShiftCreatorService.Generate(ruleSet, new WorkShiftAddStopperCallback());
			var firstGenaratedShiftCollection = shiftCollectionFromSpecificStart[0];

			var workShifts = Target.ExpandWorkShiftsWithMasterActivity(firstGenaratedShiftCollection[0]);
			foreach (var workShift in workShifts)
			{
				var firstLayer = workShift.LayerCollection.First();
				var firstPayLoad = firstLayer.Payload;
				foreach (var layer in workShift.LayerCollection)
				{
					if(layer.Equals(firstLayer))
						continue;

					layer.Payload.Should().Not.Be.EqualTo(firstPayLoad);
				}
			}
		}

		[Test]
		public void ShouldWorkIfWeHaveAnActivityAndMasterOnTop()
		{
			var workActivity = new Activity("Work") { InContractTime = true, RequiresSkill = true }.WithId();
			var phoneActivity = new Activity("Phone") { InContractTime = true, RequiresSkill = true }.WithId();
			var emailActivity = new Activity("Email") { InContractTime = true, RequiresSkill = true }.WithId();
			var masterActivity = new MasterActivity();
			masterActivity.UpdateActivityCollection(new List<IActivity> { phoneActivity, emailActivity });
			var ruleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(workActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15),
					new TimePeriodWithSegment(11, 0, 11, 0, 15), new ShiftCategory("_")));
			ruleSet.AddExtender(new ActivityAbsoluteStartExtender(masterActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15),
				new TimePeriodWithSegment(9, 0, 9, 0, 15)));

			var shiftCollectionFromSpecificStart = ShiftCreatorService.Generate(ruleSet, new WorkShiftAddStopperCallback());
			var firstGenaratedShiftCollection = shiftCollectionFromSpecificStart[0];

			var workShifts = Target.ExpandWorkShiftsWithMasterActivity(firstGenaratedShiftCollection[0]);
			workShifts.Count.Should().Be.EqualTo(2);
			workShifts[0].LayerCollection[0].Period.Should().Be.EqualTo(new DateTimePeriod(1800, 1, 1, 8, 1800, 1, 1, 11));
		}

		[Test]
		public void ShouldAssignOneOfTheMasterActivitiesAsBaseActivityForTheShifts()
		{
			var phoneActivity = new Activity("Phone") { InContractTime = true, RequiresSkill = true }.WithId();
			var emailActivity = new Activity("Email") { InContractTime = true, RequiresSkill = true }.WithId();
			var lunchActivity = new Activity("Lunch") { InContractTime = false, RequiresSkill = false }.WithId();
			var masterActivity = new MasterActivity();
			masterActivity.UpdateActivityCollection(new List<IActivity> { phoneActivity, emailActivity });
			var ruleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(masterActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15),
					new TimePeriodWithSegment(11, 0, 11, 0, 15), new ShiftCategory("_")));
			ruleSet.AddExtender(new ActivityAbsoluteStartExtender(lunchActivity, new TimePeriodWithSegment(1, 0, 1, 0, 15),
				new TimePeriodWithSegment(9, 0, 9, 0, 15)));
			var shiftCollectionFromSpecificStart = ShiftCreatorService.Generate(ruleSet, new WorkShiftAddStopperCallback());
			var firstGenaratedShiftCollection = shiftCollectionFromSpecificStart[0];

			var workShifts = Target.ExpandWorkShiftsWithMasterActivity(firstGenaratedShiftCollection[0]);
			workShifts.Count.Should().Be.EqualTo(4);
			workShifts[0].LayerCollection[0].Period.Should().Be.EqualTo(new DateTimePeriod(1800, 1, 1, 8, 1800, 1, 1, 11));
		}

		[Test]
		public void ShouldAssignOneOfTheMasterActivitiesAsBaseActivityForTheShiftsBasic()
		{
			var phoneActivity = new Activity { InContractTime = true, RequiresSkill = true }.WithId();
			var emailActivity = new Activity { InContractTime = true, RequiresSkill = true }.WithId();
			var masterActivity = new MasterActivity();
			masterActivity.UpdateActivityCollection(new List<IActivity> { phoneActivity, emailActivity });
			var ruleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(masterActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15),
					new TimePeriodWithSegment(11, 0, 11, 0, 15), new ShiftCategory("_")));

			var shiftCollectionFromSpecificStart = ShiftCreatorService.Generate(ruleSet, new WorkShiftAddStopperCallback());
			var firstGenaratedShiftCollection = shiftCollectionFromSpecificStart[0];

			var workShifts = Target.ExpandWorkShiftsWithMasterActivity(firstGenaratedShiftCollection[0]);
			workShifts.Count.Should().Be.EqualTo(2);
			workShifts[0].LayerCollection[0].Period.Should().Be.EqualTo(new DateTimePeriod(1800, 1, 1, 8, 1800, 1, 1, 11));
		}

		[Test]
		public void ShouldReturnOriginalWorkShiftWhenNoMasterActivity()
		{
			var activity1 = new Activity("1") { InContractTime = true, RequiresSkill = true };
			WorkShiftActivityLayer layer = new WorkShiftActivityLayer(activity1, new DateTimePeriod());
			var workShift = new WorkShift(new ShiftCategory("shiftCategory"));
			workShift.LayerCollection.Add(layer);

			IList<IWorkShift> workShifts = Target.ExpandWorkShiftsWithMasterActivity(workShift);
			workShifts.Single().Should().Be.SameInstanceAs(workShift);
		}

		[Test]
		public void ShouldNotGenerateShiftsForActivitiesNotInContractTimeOrNotRequiresSkill()
		{
			var master = new MasterActivity();
			var activityNotInContractTime = new Activity("notInContractTime")
			{
				InContractTime = false,
				RequiresSkill = true
			};
			var activityDontRequireSkill = new Activity("dontRequireSkill")
			{
				InContractTime = true,
				RequiresSkill = false
			};
			master.UpdateActivityCollection(new List<IActivity> { activityNotInContractTime, activityDontRequireSkill });

			DateTime time8 = new DateTime(2010, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			DateTime time17 = new DateTime(2010, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var period8To17 = new DateTimePeriod(time8, time17);
			WorkShiftActivityLayer layer = new WorkShiftActivityLayer(master, period8To17);
			var workShift = new WorkShift(new ShiftCategory("shiftCategory"));
			workShift.LayerCollection.Add(layer);

			 var workShiftList = Target.ExpandWorkShiftsWithMasterActivity(workShift);

			Assert.AreEqual(0, workShiftList.Count);
		}

		[Test]
		public void ShouldNotGenerateShiftsForDeletedActivities()
		{
			var master = new MasterActivity();
			var deletedActivity = new Activity("deleted(soon)") { RequiresSkill = true, InContractTime = true };
			deletedActivity.SetDeleted();
			master.UpdateActivityCollection(new List<IActivity> { deletedActivity });

			DateTime time8 = new DateTime(2010, 1, 1, 8, 0, 0, DateTimeKind.Utc);
			DateTime time17 = new DateTime(2010, 1, 1, 17, 0, 0, DateTimeKind.Utc);
			var period8To17 = new DateTimePeriod(time8, time17);
			WorkShiftActivityLayer layer = new WorkShiftActivityLayer(master, period8To17);
			var workShift = new WorkShift(new ShiftCategory("shiftCategory"));
			workShift.LayerCollection.Add(layer);

			var workShiftList = Target.ExpandWorkShiftsWithMasterActivity(workShift);

			Assert.AreEqual(0, workShiftList.Count);
		}
	}
}