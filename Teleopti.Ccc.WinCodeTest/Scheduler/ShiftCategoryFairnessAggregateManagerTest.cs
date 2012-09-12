using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class ShiftCategoryFairnessAggregateManagerTest
	{
		private MockRepository _mocks;
		private ISchedulingResultStateHolder _resultStateHolder;
		private IGroupPagePerDateHolder _groupPagePerDateHolder;
		private IGroupScheduleGroupPageDataProvider _groupPageDataProvider;
		private ShiftCategoryFairnessAggregateManager _target;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_resultStateHolder = _mocks.DynamicMock<ISchedulingResultStateHolder>();
			_groupPagePerDateHolder = _mocks.DynamicMock<IGroupPagePerDateHolder>();
			_groupPageDataProvider = _mocks.DynamicMock<IGroupScheduleGroupPageDataProvider>();
			_target = new ShiftCategoryFairnessAggregateManager(_resultStateHolder, _groupPagePerDateHolder,
			                                                    _groupPageDataProvider);
		}

		[Test]
		public void ShouldReturnComparisonOnPersonAndTheGroup()
		{
			var person = PersonFactory.CreatePerson("per");
			var groupPage = new GroupPageLight();
			var date = new DateOnly(2012, 9, 12);
			var result = _target.GetPerPersonAndGroup(person, groupPage, date);
		}
	}

	public class ShiftCategoryFairnessAggregateManager
	{
		private readonly ISchedulingResultStateHolder _resultStateHolder;
		private readonly IGroupPagePerDateHolder _groupPagePerDateHolder;
		private readonly IGroupScheduleGroupPageDataProvider _groupScheduleGroupPageDataProvider;

		public ShiftCategoryFairnessAggregateManager(ISchedulingResultStateHolder resultStateHolder, IGroupPagePerDateHolder groupPagePerDateHolder,
			IGroupScheduleGroupPageDataProvider groupScheduleGroupPageDataProvider)
		{
			_resultStateHolder = resultStateHolder;
			_groupPagePerDateHolder = groupPagePerDateHolder;
			_groupScheduleGroupPageDataProvider = groupScheduleGroupPageDataProvider;
		}

		public ShiftCategoryFairnessCompareResult GetPerPersonAndGroup(IPerson person, IGroupPageLight groupPage, DateOnly date)
		{
			throw new System.NotImplementedException();

		}
	}
}