using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture]
	public class ShiftTradeRequestPersisterTest
	{
		[Test]
		public void ShouldPersistMappedData()
		{
			var mapper = MockRepository.GenerateMock<IShiftTradeRequestMapper>();
			var autoMapper = MockRepository.GenerateMock<IMappingEngine>();
			var repository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var target = new ShiftTradeRequestPersister(repository, mapper, autoMapper);
			var form = new ShiftTradeRequestForm();
			var shiftTradeRequest = new PersonRequest(new Person());
			var viewModel = new RequestViewModel();

			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);
			autoMapper.Stub(x => x.Map<IPersonRequest, RequestViewModel>(shiftTradeRequest)).Return(viewModel);

			var result = target.Persist(form);

			result.Should().Be.SameInstanceAs(viewModel);
			repository.AssertWasCalled(x => x.Add(shiftTradeRequest));
		}
	}
}