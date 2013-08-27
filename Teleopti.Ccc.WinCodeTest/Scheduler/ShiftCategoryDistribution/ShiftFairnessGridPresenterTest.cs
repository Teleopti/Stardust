using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
	[TestFixture]
	public class ShiftFairnessGridPresenterTest
	{
		private ShiftFairnessGridPresenter _target;
		private MockRepository _mock;
		private IShiftFairnessGrid _view;
		

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_view = _mock.StrictMock<IShiftFairnessGrid>();
			_target = new ShiftFairnessGridPresenter(_view);	
		}

		[Test]
		public void ShouldCalculateTotalStandardDeviation()
		{
			var shiftCategory1 = new ShiftCategory("shiftCategory1");
			var shiftCategory2 = new ShiftCategory("shiftCategory2");
			var fairness1 = new ShiftFairness(shiftCategory1, 2, 4, 3, 1.0f);
			var fairness2 = new ShiftFairness(shiftCategory2, 2, 4, 3, 1.0f);
			var fairnessList = new List<ShiftFairness> {fairness1, fairness2};

			var total = _target.CalculateTotalStandardDeviation(fairnessList);
			Assert.AreEqual(2.0f, total);
		}
	}
}
