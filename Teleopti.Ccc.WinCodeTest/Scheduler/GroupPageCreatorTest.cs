﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.WinCode.Scheduling;
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

		[Test]
		public void ShouldReturnMainGrouping()
		{
			var grouping = new GroupPageLight {Key = "Main"};
			var date = new DateOnly(2012, 9, 12);
			var creator = _mocks.DynamicMock<IGroupPageCreator<IBusinessUnit>>();
			Expect.Call(_groupPageFactory.GetPersonsGroupPageCreator()).Return(creator);
			Expect.Call(_groupPageDataProvider.BusinessUnitCollection).Return(new List<IBusinessUnit>());
			Expect.Call(creator.CreateGroupPage(null, null)).IgnoreArguments().Return(new GroupPage("page"));
			_mocks.ReplayAll();
			_target.CreateGroupPagePerDate(new List<DateOnly> {date}, _groupPageDataProvider, grouping);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnContractGrouping()
		{
			var grouping = new GroupPageLight { Key = "Contracts" };
			var date = new DateOnly(2012, 9, 12);
			var creator = _mocks.DynamicMock<IGroupPageCreator<IContract>>();
			Expect.Call(_groupPageFactory.GetContractsGroupPageCreator()).Return(creator);
			Expect.Call(_groupPageDataProvider.ContractCollection).Return(new List<IContract>());
			Expect.Call(creator.CreateGroupPage(null, null)).IgnoreArguments().Return(new GroupPage("page"));
			_mocks.ReplayAll();
			_target.CreateGroupPagePerDate(new List<DateOnly> { date }, _groupPageDataProvider, grouping);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnContractScheduleGrouping()
		{
			var grouping = new GroupPageLight { Key = "ContractSchedule" };
			var date = new DateOnly(2012, 9, 12);
			var creator = _mocks.DynamicMock<IGroupPageCreator<IContractSchedule>>();
			Expect.Call(_groupPageFactory.GetContractSchedulesGroupPageCreator()).Return(creator);
			Expect.Call(_groupPageDataProvider.ContractScheduleCollection).Return(new List<IContractSchedule>());
			Expect.Call(creator.CreateGroupPage(null, null)).IgnoreArguments().Return(new GroupPage("page"));
			_mocks.ReplayAll();
			_target.CreateGroupPagePerDate(new List<DateOnly> { date }, _groupPageDataProvider, grouping);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnPartTimePercentagesGrouping()
		{
			var grouping = new GroupPageLight { Key = "PartTimepercentages" };
			var date = new DateOnly(2012, 9, 12);
			var creator = _mocks.DynamicMock<IGroupPageCreator<IPartTimePercentage>>();
			Expect.Call(_groupPageFactory.GetPartTimePercentagesGroupPageCreator()).Return(creator);
			Expect.Call(_groupPageDataProvider.PartTimePercentageCollection).Return(new List<IPartTimePercentage>());
			Expect.Call(creator.CreateGroupPage(null, null)).IgnoreArguments().Return(new GroupPage("page"));
			_mocks.ReplayAll();
			_target.CreateGroupPagePerDate(new List<DateOnly> { date }, _groupPageDataProvider, grouping);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnRuleSetBagsGrouping()
		{
			var grouping = new GroupPageLight { Key = "RuleSetBag" };
			var date = new DateOnly(2012, 9, 12);
			var creator = _mocks.DynamicMock<IGroupPageCreator<IRuleSetBag>>();
			Expect.Call(_groupPageFactory.GetRuleSetBagsGroupPageCreator()).Return(creator);
			Expect.Call(_groupPageDataProvider.RuleSetBagCollection).Return(new List<IRuleSetBag>());
			Expect.Call(creator.CreateGroupPage(null, null)).IgnoreArguments().Return(new GroupPage("page"));
			_mocks.ReplayAll();
			_target.CreateGroupPagePerDate(new List<DateOnly> { date }, _groupPageDataProvider, grouping);
			_mocks.VerifyAll();
		}


		[Test]
		public void ShouldReturnNotesGrouping()
		{
			var grouping = new GroupPageLight { Key = "Note" };
			var date = new DateOnly(2012, 9, 12);
			var creator = _mocks.DynamicMock<IGroupPageCreator<IPerson>>();
			Expect.Call(_groupPageFactory.GetNotesGroupPageCreator()).Return(creator);
			Expect.Call(creator.CreateGroupPage(null, null)).IgnoreArguments().Return(new GroupPage("page"));
			_mocks.ReplayAll();
			_target.CreateGroupPagePerDate(new List<DateOnly> { date }, _groupPageDataProvider, grouping);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnCustomGrouping()
		{
			var id = Guid.NewGuid();
			var grouping = new GroupPageLight { Key = id.ToString() };
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
		public void ShouldReturnEmptyGrouping()
		{
			var grouping = new GroupPageLight { Key = "And now something completely different" };
			var date = new DateOnly(2012, 9, 12);
			var gp = new GroupPage("custom");
			gp.SetId(Guid.NewGuid());
			var lst = new List<IGroupPage> {gp};
			
			Expect.Call(_groupPageDataProvider.UserDefinedGroupings).Return(lst);
			
			_mocks.ReplayAll();
			var res = _target.CreateGroupPagePerDate(new List<DateOnly> { date }, _groupPageDataProvider, grouping);
			Assert.That(res.GetGroupPageByDate(date),Is.Not.EqualTo(gp));
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnGroupingViaView()
		{
			var grouping = new GroupPageLight { Key = "Main" };
			var date = new DateOnly(2012, 9, 12);
			var creator = _mocks.DynamicMock<IGroupPageCreator<IBusinessUnit>>();
			var view = _mocks.DynamicMock<IScheduleViewBase>();
			Expect.Call(view.AllSelectedDates()).Return(new List<DateOnly> {date});
			Expect.Call(_groupPageFactory.GetPersonsGroupPageCreator()).Return(creator);
			Expect.Call(_groupPageDataProvider.BusinessUnitCollection).Return(new List<IBusinessUnit>());
			Expect.Call(creator.CreateGroupPage(null, null)).IgnoreArguments().Return(new GroupPage("page"));
			_mocks.ReplayAll();
			_target.CreateGroupPagePerDate(view, _groupPageDataProvider, grouping);
			_mocks.VerifyAll();
		}
	}

	
}

