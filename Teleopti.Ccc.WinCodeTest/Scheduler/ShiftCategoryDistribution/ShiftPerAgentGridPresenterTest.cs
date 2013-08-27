using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
	[TestFixture]
	public class ShiftPerAgentGridPresenterTest
	{
		private ShiftPerAgentGridPresenter _target;
		private MockRepository _mock;
		private IShiftPerAgentGrid _view;
		private IPerson _person;

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_view = _mock.StrictMock<IShiftPerAgentGrid>();
			_target = new ShiftPerAgentGridPresenter(_view);
			_person = PersonFactory.CreatePerson("person");
		}

		[Test]
		public void ShouldGetShiftCategoryCount()
		{
			var shiftCategory1 = new ShiftCategory("shiftCategory1");
			var shiftCategory2 = new ShiftCategory("shiftCategory2");
			var shiftCategoryPerAgent1 = new ShiftCategoryPerAgent(_person, shiftCategory1, 1);
			var shiftCategoryPerAgent2 = new ShiftCategoryPerAgent(_person, shiftCategory2, 2);
			var shiftCategoryPerAgentList = new List<ShiftCategoryPerAgent> {shiftCategoryPerAgent1, shiftCategoryPerAgent2};
			var otherPerson = PersonFactory.CreatePerson("other");

			var count = _target.ShiftCategoryCount(_person, shiftCategory1, shiftCategoryPerAgentList);
			Assert.AreEqual(1, count);

			count = _target.ShiftCategoryCount(_person, shiftCategory2, shiftCategoryPerAgentList);
			Assert.AreEqual(2, count);

			count = _target.ShiftCategoryCount(otherPerson, shiftCategory1, shiftCategoryPerAgentList);
			Assert.AreEqual(0, count);
		}
	}
}
