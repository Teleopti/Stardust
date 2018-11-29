using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting.Angel
{
	public class DayIndexVolumesTest
	{
		[Test]
		public void ShouldCreate()
		{
			var target = new IndexVolumesShortTerm();
			var result = target.Create(new TaskOwnerPeriod(new DateOnly(2015, 1, 1), new ITaskOwner[] { }, TaskOwnerPeriodType.Other));
			var volumeYears = result as IVolumeYear[] ?? result.ToArray();
			volumeYears[0].GetType().Should().Be.EqualTo(typeof(DayOfWeeks));
			
		}
	}
}