using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class ShiftProjectionCachesFromAdjustedRuleSetBagShiftFilterTest
	{
		private MockRepository _mocks;
		private IShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter _target;
		private IRuleSetDeletedActivityChecker _ruleSetDeletedActivityChecker;
		private IRuleSetDeletedShiftCategoryChecker _rulesSetDeletedShiftCategoryChecker;
		private IRuleSetToShiftsGenerator _ruleSetToShiftsGenerator;
		//private IPerson _person;
		//private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_ruleSetDeletedActivityChecker = _mocks.StrictMock<IRuleSetDeletedActivityChecker>();
			_rulesSetDeletedShiftCategoryChecker = _mocks.StrictMock<IRuleSetDeletedShiftCategoryChecker>();
			_ruleSetToShiftsGenerator = _mocks.StrictMock<IRuleSetToShiftsGenerator>();
			//_person = _mocks.StrictMock<IPerson>();
			//_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
			_target = new ShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter(_ruleSetDeletedActivityChecker,
			                                                                     _rulesSetDeletedShiftCategoryChecker,
			                                                                     _ruleSetToShiftsGenerator);
		}

	}
}
