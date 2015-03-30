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
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetGroupsForGroupPageAtDateQueryHandlerTest
	{
		private IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private GetGroupsForGroupPageAtDateQueryHandler target;
		private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IUnitOfWork _unitOfWork;

		[SetUp]
		public void Setup()
		{
			_groupingReadOnlyRepository = MockRepository.GenerateMock<IGroupingReadOnlyRepository>();
			_currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			_unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			target = new GetGroupsForGroupPageAtDateQueryHandler(_groupingReadOnlyRepository, _currentUnitOfWorkFactory);
		}

		[Test]
		public void ShouldGetCustomGroups()
		{
			var readOnlyGroupPage = new ReadOnlyGroupPage{PageId = Guid.NewGuid(),PageName = "Test"};
			var groupDetail = new ReadOnlyGroupDetail{GroupId = Guid.NewGuid(),GroupName = "My Group"};
			var groupDetailList = new List<ReadOnlyGroupDetail> { groupDetail };
			var dateOnly = new DateOnly(2012, 4, 30);

			_currentUnitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_groupingReadOnlyRepository.Stub(x => x.AvailableGroups(readOnlyGroupPage, dateOnly)).Constraints(
					new Rhino.Mocks.Constraints.PredicateConstraint<ReadOnlyGroupPage>(p => p.PageId == readOnlyGroupPage.PageId),
					Rhino.Mocks.Constraints.Is.Equal(dateOnly)).Return(groupDetailList);
			
			var result = target.Handle(new GetGroupsForGroupPageAtDateQueryDto{PageId = readOnlyGroupPage.PageId,QueryDate = new DateOnlyDto{DateTime = dateOnly.Date}});
			result.Count.Should().Be.EqualTo(1);
		}
	}
}