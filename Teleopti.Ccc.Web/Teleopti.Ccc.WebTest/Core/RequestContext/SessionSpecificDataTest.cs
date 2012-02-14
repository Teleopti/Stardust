using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Core.RequestContext
{
	[TestFixture]
	public class SessionSpecificDataTest
	{
		private SessionSpecificData target;
		private Guid buId;
		private Guid personId;
		private string dataSource;

		[SetUp]
		public void Setup()
		{
			buId = Guid.NewGuid();
			personId = Guid.NewGuid();
			dataSource = Guid.NewGuid().ToString();
			target = new SessionSpecificData(buId, dataSource, personId);			
		}

		[Test]
		public void DataCanBeRead()
		{
			target.BusinessUnitId.Should().Be.EqualTo(buId);
			target.PersonId.Should().Be.EqualTo(personId);
			target.DataSourceName.Should().Be.EqualTo(dataSource);
		}
	}
}