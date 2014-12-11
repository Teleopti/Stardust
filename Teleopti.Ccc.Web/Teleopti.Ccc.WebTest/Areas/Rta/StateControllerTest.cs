using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class StateControllerTest
	{
		[Test]
		public void ShouldSendMessageIfBatchIdIsNull()
		{
			var personId = Guid.NewGuid();
			var fakeRtaDatabase = new FakeRtaDatabase()
				.WithUser("usercode", personId)
				.Make();
			var fakeMessageSender = new FakeMessageSender();
			var target =
				new StateController(new RtaForTest(fakeRtaDatabase, new ThisIsNow("2014-11-14 10:00"),
					fakeMessageSender));

			target.Change(new ExternalUserStateWebModelForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			fakeMessageSender.NotificationOfType<IActualAgentState>()
				.DeseralizeActualAgentState()
				.PersonId.Should().Be(personId);
		}
	}
}