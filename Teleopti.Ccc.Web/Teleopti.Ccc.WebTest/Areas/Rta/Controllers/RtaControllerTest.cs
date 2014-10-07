using System;
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Rta.WebService;
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

		[Test]
		public void ShouldNotThrowOnResultZero_WhichIThinkMeansItWentOkButStateWasNotWritten()
		{
			var rtaWebService = MockRepository.GenerateMock<ITeleoptiRtaService>();
			rtaWebService.Stub(
				x => x.SaveExternalUserState(null, null, null, null, true, 0, DateTime.Now, null, null, DateTime.Now, false))
				.IgnoreArguments()
				.Return(0);
			var target = new ServiceController(rtaWebService);

			Assert.DoesNotThrow(() => target.SaveExternalUserState(new AjaxUserState()));
		}

		[Test]
		public void ShouldNotThrowOnResultOne_WhichIThinkMeansStateWasWritten()
		{
			var rtaWebService = MockRepository.GenerateMock<ITeleoptiRtaService>();
			rtaWebService.Stub(
				x => x.SaveExternalUserState(null, null, null, null, true, 0, DateTime.Now, null, null, DateTime.Now, false))
				.IgnoreArguments()
				.Return(1);
			var target = new ServiceController(rtaWebService);

			Assert.DoesNotThrow(() => target.SaveExternalUserState(new AjaxUserState()));

		}

		[Test]
		public void ShouldThrowOnErrorResult()
		{
			var rtaWebService = MockRepository.GenerateMock<ITeleoptiRtaService>();
			rtaWebService.Stub(
				x => x.SaveExternalUserState(null, null, null, null, true, 0, DateTime.Now, null, null, DateTime.Now, false))
				.IgnoreArguments()
				.Return(-300);
			var target = new ServiceController(rtaWebService);

			Assert.Throws<HttpException>(() => target.SaveExternalUserState(new AjaxUserState()));
		}
		
	}
}
