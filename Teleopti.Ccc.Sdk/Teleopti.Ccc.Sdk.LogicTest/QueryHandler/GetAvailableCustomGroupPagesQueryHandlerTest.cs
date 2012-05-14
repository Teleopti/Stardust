using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetAvailableCustomGroupPagesQueryHandlerTest
	{
		private MockRepository mocks;
		private IGroupingReadOnlyRepository groupingReadOnlyRepository;
		private GetAvailableCustomGroupPagesQueryHandler target;
		
		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			groupingReadOnlyRepository = mocks.DynamicMock<IGroupingReadOnlyRepository>();
			target = new GetAvailableCustomGroupPagesQueryHandler(groupingReadOnlyRepository);
		}

		[Test]
		public void ShouldGetAvailableCustomGroupPages()
		{
			var readOnlyGroupPage = new ReadOnlyGroupPage{PageId = Guid.NewGuid(),PageName = "Test"};
			var readOnlyGroupPageList = new List<ReadOnlyGroupPage> {readOnlyGroupPage};
			using (mocks.Record())
			{
				Expect.Call(groupingReadOnlyRepository.AvailableGroupPages()).Return(readOnlyGroupPageList);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetAvailableCustomGroupPagesQueryDto());
				result.First().Id.Should().Be.EqualTo(readOnlyGroupPage.PageId);
			}
		}
	}
}