using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetGroupsForGroupPageAtDateQueryHandlerTest
	{
		private MockRepository mocks;
		private IGroupingReadOnlyRepository groupingReadOnlyRepository;
		private GetGroupsForGroupPageAtDateQueryHandler target;
		
		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			groupingReadOnlyRepository = mocks.DynamicMock<IGroupingReadOnlyRepository>();
			target = new GetGroupsForGroupPageAtDateQueryHandler(groupingReadOnlyRepository);
		}

		[Test]
		public void ShouldGetCustomGroups()
		{
			var readOnlyGroupPage = new ReadOnlyGroupPage{PageId = Guid.NewGuid(),PageName = "Test"};
			var groupDetail = new ReadOnlyGroupDetail{GroupId = Guid.NewGuid(),GroupName = "My Group"};
			var groupDetailList = new List<ReadOnlyGroupDetail> { groupDetail };
			var dateOnly = new DateOnly(2012, 4, 30);
			using (mocks.Record())
			{
				Expect.Call(groupingReadOnlyRepository.AvailableGroups(readOnlyGroupPage, dateOnly)).Constraints(
					new Rhino.Mocks.Constraints.PredicateConstraint<ReadOnlyGroupPage>(p => p.PageId == readOnlyGroupPage.PageId),
					Rhino.Mocks.Constraints.Is.Equal(dateOnly)).Return(groupDetailList);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetGroupsForGroupPageAtDateQueryDto{PageId = readOnlyGroupPage.PageId,QueryDate = new DateOnlyDto{DateTime = dateOnly}});
				result.Count.Should().Be.EqualTo(1);
			}
		}
	}
}