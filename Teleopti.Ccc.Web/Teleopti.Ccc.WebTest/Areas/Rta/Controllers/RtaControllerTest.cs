using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Controllers
{
	public class RtaControllerTest
	{
		[Test]
		public void ShouldCallRtaWebService()
		{
			const string externalState =
				@"{
'authenticationKey':'!#¤atAbgT%',
'userCode':2001,
'stateCode':'OFF',
'isLoggedOn':'true',
'timestamp':'2014-06-13T10:54:27',
'platformTypeId':'00000000-0000-0000-0000-000000000000',
'sourceId':1,
'isSnapshot':'false'}";
			var rtaWebService = MockRepository.GenerateMock<ITeleoptiRtaService>();
			var target = new RtaServiceController(rtaWebService);

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
