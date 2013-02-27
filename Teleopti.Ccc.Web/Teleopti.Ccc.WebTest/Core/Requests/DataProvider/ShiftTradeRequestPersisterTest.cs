using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture]
	public class ShiftTradeRequestPersisterTest
	{
		[Test]
		public void ShouldPersistMappedData()
		{
			var mapper = MockRepository.GenerateMock<IShiftTradeRequestMapper>();
			var repository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var target = new ShiftTradeRequestPersister(repository, mapper);
			var form = new ShiftTradeRequestForm();
			var shiftTradeRequest = new PersonRequest(new Person());
			mapper.Expect(x => x.Map(form)).Return(shiftTradeRequest);

			target.Persist(form);

			repository.AssertWasCalled(x => x.Add(shiftTradeRequest));
		}
	}
}