using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Requests;
using Teleopti.Ccc.Web.Areas.Requests.Controller;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModel;
using Teleopti.Ccc.Web.Areas.Requests.Core.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Requests.Controller
{
	[TestFixture]
	public class RequestsControllerTest
	{
		[Test]
		public void ShouldGetAllRequests()
		{
			var dateOnlyPeriod = new DateOnlyPeriod(2015, 11, 1, 2015, 11, 2);
			
			var requestsViewModelFactory = MockRepository.GenerateMock<IRequestsViewModelFactory>();
			var requestViewModels = new[] { new RequestViewModel()};
			requestsViewModelFactory.Stub(x => x.Create(dateOnlyPeriod)).Return(requestViewModels);
			var target = new RequestsController(requestsViewModelFactory);
			var requests = target.All(dateOnlyPeriod);

			requests.Should().Be.SameInstanceAs(requestViewModels);
		}
	}
}