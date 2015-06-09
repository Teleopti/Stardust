using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class SessionSpecificDataTest
	{
		private SessionSpecificData target;
		private Guid buId;
		private Guid personId;
		private string dataSource;
		private string tenantPassword;

		[SetUp]
		public void Setup()
		{
			buId = Guid.NewGuid();
			personId = Guid.NewGuid();
			dataSource = Guid.NewGuid().ToString();
			tenantPassword = RandomName.Make();
			target = new SessionSpecificData(buId, dataSource, personId, tenantPassword);
		}

		[Test]
		public void DataCanBeRead()
		{
			target.BusinessUnitId.Should().Be.EqualTo(buId);
			target.PersonId.Should().Be.EqualTo(personId);
			target.DataSourceName.Should().Be.EqualTo(dataSource);
			target.TenantPassword.Should().Be.EqualTo(tenantPassword);
		}
	}
}