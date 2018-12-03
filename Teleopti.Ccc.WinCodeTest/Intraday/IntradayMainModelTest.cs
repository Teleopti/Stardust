using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;


namespace Teleopti.Ccc.WinCodeTest.Intraday
{
	[TestFixture]
	public class IntradayMainModelTest
	{
		private IntradayMainModel _target;

		[Test]
		public void ShouldReturnPeriodAsDateTimePeriod()
		{
			_target = new IntradayMainModel();
			var dateOnlyPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1));
			_target.Period = dateOnlyPeriod;
			Assert.AreEqual(_target.PeriodAsDateTimePeriod(), dateOnlyPeriod.ToDateTimePeriod(TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone));
		}
	}
}
