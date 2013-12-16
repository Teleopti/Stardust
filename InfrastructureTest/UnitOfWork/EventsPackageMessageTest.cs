using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class EventsPackageMessageTest
	{
		[Test]
		public void ShouldSerializeEventMessages()
		{
			var message = new EventsPackageMessage{Events = new List<Event>{new Event()}};
			string xml = SerializationHelper.SerializeAsXml(message);
			Assert.IsNotEmpty(xml);
		}
	}
}