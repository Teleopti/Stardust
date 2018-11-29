using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class ValidationAlertsModelTest
	{
		[Test]
		public void ShouldReturnUniqueList()
		{
			var result = new HashSet<ValidationAlertsModel.ValidationAlert>();
			var alert = new ValidationAlertsModel.ValidationAlert(new DateOnly(2016,10,10), 
						"olle", "hej");
			result.Add(alert);

			alert = new ValidationAlertsModel.ValidationAlert(new DateOnly(2016, 10, 10),
						"olle", "hej");
			result.Add(alert);

			alert = new ValidationAlertsModel.ValidationAlert(new DateOnly(2016, 10, 11),
						"olle", "hej");
			result.Add(alert);

			result.Count.Should().Be.EqualTo(2);
		}
	}
}