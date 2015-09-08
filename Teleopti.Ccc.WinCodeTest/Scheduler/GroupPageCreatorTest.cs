using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class GroupPageCreatorTest
	{
		private MockRepository _mocks;
		private IGroupPageFactory _groupPageFactory;
		private GroupPageCreator _target;
		private IGroupPageDataProvider _groupPageDataProvider;
        
		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_groupPageFactory = _mocks.DynamicMock<IGroupPageFactory>();
			_groupPageDataProvider = _mocks.DynamicMock<IGroupPageDataProvider>();

			_target = new GroupPageCreator(_groupPageFactory);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowIfDatesIsNull()
		{
			var grouping = new GroupPageLight();
			_target.CreateGroupPagePerDate(null, _groupPageDataProvider, grouping);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowIfProviderIsNull()
		{
			var grouping = new GroupPageLight();
			_target.CreateGroupPagePerDate(new List<DateOnly>(), null, grouping);
		}

		[Test]
		public void ShouldReturnCustomGrouping()
		{
			var id = Guid.NewGuid();
			var grouping = new GroupPageLight(string.Empty, GroupPageType.UserDefined, id.ToString());
			var date = new DateOnly(2012, 9, 12);
			var gp = new GroupPage("custom");
			gp.SetId(id);
			var lst = new List<IGroupPage> { gp };

			Expect.Call(_groupPageDataProvider.UserDefinedGroupings).Return(lst);

			_mocks.ReplayAll();
			var res = _target.CreateGroupPagePerDate(new List<DateOnly> { date }, _groupPageDataProvider, grouping);
			Assert.That(res.GetGroupPageByDate(date), Is.EqualTo(gp));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldFactoryShouldProvideAllTypes()
		{
			var factory = new GroupPageFactory();
			Assert.That(factory.GetContractSchedulesGroupPageCreator(),Is.Not.Null);
			Assert.That(factory.GetContractsGroupPageCreator(),Is.Not.Null);
			Assert.That(factory.GetNotesGroupPageCreator(),Is.Not.Null);
			Assert.That(factory.GetPartTimePercentagesGroupPageCreator(),Is.Not.Null);
			Assert.That(factory.GetPersonsGroupPageCreator(),Is.Not.Null);
			Assert.That(factory.GetRuleSetBagsGroupPageCreator(),Is.Not.Null);
		}
	}
}
