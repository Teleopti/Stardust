using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class ShiftCategoryFairnessGroupPersonHolderTest
	{
		private MockRepository _mocks;
		private IGroupPagePerDateHolder _groupPageHolder;
		private IGroupScheduleGroupPageDataProvider _groupPageDataProvider;
		private ShiftCategoryFairnessGroupPersonHolder _target;
		private IGroupPageCreator _groupPageCreator;
		private IGroupPersonsBuilder _groupPersonBuilder;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_groupPageCreator = _mocks.DynamicMock<IGroupPageCreator>();
			_groupPageHolder = _mocks.DynamicMock<IGroupPagePerDateHolder>();
			_groupPersonBuilder = _mocks.StrictMock<IGroupPersonsBuilder>();
			_groupPageDataProvider = _mocks.DynamicMock<IGroupScheduleGroupPageDataProvider>();
			_target = new ShiftCategoryFairnessGroupPersonHolder( _groupPageCreator, _groupPageDataProvider,
																_groupPageHolder, _groupPersonBuilder);
		}

		[Test]
		public void ShouldGetGroupPersonsFromBuilder()
		{
			var person = PersonFactory.CreatePersonWithBasicPermissionInfo("per", "olsson");
			var groupPage = new GroupPageLight();
			var date = new DateOnly(2012, 9, 12);
			var groupPagePerDateDic = new Dictionary<DateOnly, IGroupPage>();
			var groupPagePerDate = new GroupPagePerDate(groupPagePerDateDic);
			var toReturn = new List<IGroupPerson>();
			var persons = new List<IPerson> {person};

			Expect.Call(_groupPageCreator.CreateGroupPagePerDate(new List<DateOnly>(), _groupPageDataProvider, groupPage)).
				IgnoreArguments().Return(groupPagePerDate);
			Expect.Call(_groupPageHolder.FairnessOptimizerGroupPagePerDate = groupPagePerDate);
			Expect.Call(_groupPersonBuilder.BuildListOfGroupPersons(date, persons, false, null)).Return(toReturn);
			_mocks.ReplayAll();
			var ret =_target.GroupPersons(new List<DateOnly>(), groupPage, date,persons);
			Assert.That(ret, Is.EqualTo(toReturn));
			_mocks.VerifyAll();
		}
	}
}