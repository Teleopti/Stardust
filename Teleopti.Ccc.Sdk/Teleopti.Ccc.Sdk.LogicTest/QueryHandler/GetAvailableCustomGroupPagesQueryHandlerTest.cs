using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetAvailableCustomGroupPagesQueryHandlerTest
	{
		private IGroupingReadOnlyRepository _groupingReadOnlyRepository;
		private GetAvailableCustomGroupPagesQueryHandler target;
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
			target = new GetAvailableCustomGroupPagesQueryHandler(_groupingReadOnlyRepository, _currentUnitOfWorkFactory);
		}

		[Test]
		public void ShouldGetAvailableCustomGroupPages()
		{
			var readOnlyGroupPage = new ReadOnlyGroupPage{PageId = Guid.NewGuid(),PageName = "Test"};
			var readOnlyGroupPageList = new List<ReadOnlyGroupPage> {readOnlyGroupPage};
			
			_currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			_groupingReadOnlyRepository.Stub(x => x.AvailableGroupPages()).Return(readOnlyGroupPageList);
			var result = target.Handle(new GetAvailableCustomGroupPagesQueryDto());
			result.First().Id.Should().Be.EqualTo(readOnlyGroupPage.PageId);
			
		}
	}
}