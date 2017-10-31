﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetGroupsForGroupPageAtDateQueryHandlerTest
	{
		[Test]
		public void ShouldGetCustomGroups()
		{
			var groupDetail = new ReadOnlyGroupDetail {GroupId = Guid.NewGuid(), GroupName = "My Group"};
			var groupingReadOnlyRepository = new FakeGroupingReadOnlyRepository(groupDetail);
			var currentUnitOfWorkFactory = new FakeCurrentUnitOfWorkFactory(new FakeStorage());
			var target = new GetGroupsForGroupPageAtDateQueryHandler(groupingReadOnlyRepository, currentUnitOfWorkFactory);

			var readOnlyGroupPage = new ReadOnlyGroupPage {PageId = Guid.NewGuid(), PageName = "Test"};
			var dateOnly = new DateOnly(2012, 4, 30);

			var result =
				target.Handle(new GetGroupsForGroupPageAtDateQueryDto
				{
					PageId = readOnlyGroupPage.PageId,
					QueryDate = new DateOnlyDto {DateTime = dateOnly.Date}
				});
			result.Count.Should().Be.EqualTo(1);
		}
	}
}