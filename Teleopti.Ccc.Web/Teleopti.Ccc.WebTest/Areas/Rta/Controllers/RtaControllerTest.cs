using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Controllers
{
	public class RtaControllerTest
	{
		[Test]
		public void ShouldCallRtaWebService()
		{
			var externalState = new AjaxUserState
			{
				AuthenticationKey = "!#¤atAbgT%",
				UserCode = "2001",
				StateCode = "OFF",
				IsLoggedOn = true,
				Timestamp = "2014-06-13T10:54:27",
				PlatformTypeId = "00000000-0000-0000-0000-000000000000",
				SourceId = "1",
				IsSnapshot = false
			};
			var rtaWebService = MockRepository.GenerateMock<ITeleoptiRtaService>();
			var target = new ServiceController(rtaWebService);

			target.SaveExternalUserState(externalState);

			rtaWebService.AssertWasCalled(
				x => x.SaveExternalUserState("!#¤atAbgT%",
				"2001",
				"OFF",
				null,
				true,
				0,
				new DateTime(2014, 06, 13, 10, 54, 27),
				Guid.Empty.ToString(),
				"1",
				DateTime.MinValue,
				false));
		}
	}
}
