using System.Collections.Specialized;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Web;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
	public class BusinessUnitForRequestTest
	{
		[Test]
		public void GetBusinessUnitForRequest()
		{
			var bu = BusinessUnitFactory.CreateWithId("testBu1");
			var httpContext = new MutableFakeCurrentHttpContext();
			var headers = new NameValueCollection { { "X-Business-Unit-Filter", bu.Id.Value.ToString() } };
			httpContext.SetContext(new FakeHttpContext(null, null, null, null, null, null, null, headers));

			var businessUnitRepository = MockRepository.GenerateMock<IBusinessUnitRepository>();
			businessUnitRepository.Stub(x => x.Load(bu.Id.Value)).Return(bu);
			var uowFactory = new FakeUnitOfWorkFactory();
			uowFactory.SetHasCurrentUnitOfWork();
			var currentUowFactory = MockRepository.GenerateStub<ICurrentUnitOfWorkFactory>();
			currentUowFactory.Expect(x => x.Current()).Return(uowFactory);
			var target = new BusinessUnitForRequest(httpContext, businessUnitRepository, currentUowFactory);
			var businessUnit = target.TryGetBusinessUnit();

			businessUnit.Should().Be.EqualTo(bu);
		}

		[Test]
		public void ShouldNotThrowIfHttpContextDoesntExistButReturnNullInstead()
		{
			//sort of wrong - but to keep old behavior after HttpRequestFalse no longer used in web
			var bu = BusinessUnitFactory.CreateWithId("testBu1");
			var httpContext = new MutableFakeCurrentHttpContext();
			var headers = new NameValueCollection { { "X-Business-Unit-Filter", bu.Id.Value.ToString() } };
			httpContext.SetContext(new FakeHttpContext(null, null, null, null, null, null, null, headers));
			var uowFactory = new FakeUnitOfWorkFactory();
			var currentUowFactory = MockRepository.GenerateStub<ICurrentUnitOfWorkFactory>();
			currentUowFactory.Expect(x => x.Current()).Return(uowFactory);

			var target = new BusinessUnitForRequest(httpContext, null, currentUowFactory);
			var businessUnit = target.TryGetBusinessUnit();

			businessUnit.Should().Be.Null();
		}
	}
}