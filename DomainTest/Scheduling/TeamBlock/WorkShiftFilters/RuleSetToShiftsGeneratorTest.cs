using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class RuleSetToShiftsGeneratorTest
	{
		private MockRepository _mocks;
		private IRuleSetProjectionEntityService _ruleSetProjectionEntityService;
		private IShiftFromMasterActivityService _shiftFromMasterActivityService;
		private IRuleSetToShiftsGenerator _target;
		private Activity _activity;
		private ShiftCategory _category;
		private WorkShift _workShift1;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_ruleSetProjectionEntityService = _mocks.StrictMock<IRuleSetProjectionEntityService>();
			_shiftFromMasterActivityService = _mocks.StrictMock<IShiftFromMasterActivityService>();
			_target = new RuleSetToShiftsGenerator(_ruleSetProjectionEntityService, _shiftFromMasterActivityService);
		}

		[Test]
		public void CanAdjustWorkShiftsFromRuleSetBag()
		{
			var ruleSet1 = _mocks.StrictMock<IWorkShiftRuleSet>();
			var workShifts = getWorkShifts();
			var workShiftsInfo = getWorkShiftsInfo();
			using (_mocks.Record())
			{
				Expect.Call(_ruleSetProjectionEntityService.ProjectionCollection(ruleSet1, null)).IgnoreArguments().Return(workShiftsInfo);
				Expect.Call(_shiftFromMasterActivityService.Generate(_workShift1)).Return(workShifts).Repeat.Twice();
			}

			using (_mocks.Playback())
			{
				var ret = _target.Generate(ruleSet1);
				Assert.That(ret.Count(), Is.EqualTo(2));
			}
		}

		private IList<IWorkShift> getWorkShifts()
		{
			_activity = ActivityFactory.CreateActivity("sd");
			_category = ShiftCategoryFactory.CreateShiftCategory("dv");
			_workShift1 = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
														   _activity, _category);

			return new List<IWorkShift> { _workShift1 };
		}

		private IEnumerable<IWorkShiftVisualLayerInfo> getWorkShiftsInfo()
		{
			_activity = ActivityFactory.CreateActivity("sd");
			_category = ShiftCategoryFactory.CreateShiftCategory("dv");

			IWorkShiftVisualLayerInfo info1 = new WorkShiftVisualLayerInfo(_workShift1, null);
			IWorkShiftVisualLayerInfo info2 = new WorkShiftVisualLayerInfo(_workShift1, null);
			return new List<IWorkShiftVisualLayerInfo> { info1, info2 };
		}
	}
}
