using System.Collections.Specialized;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Web;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
	public class HttpRequestTrueTest
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

			var target = new HttpRequestTrue(httpContext, businessUnitRepository);
			var businessUnit = target.BusinessUnitForRequest();

			businessUnit.Should().Be.EqualTo(bu);
		}
	}
}