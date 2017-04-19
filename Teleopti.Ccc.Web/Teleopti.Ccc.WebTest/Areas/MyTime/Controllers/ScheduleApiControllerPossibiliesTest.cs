using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.WebTest.Core.IoC;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[MyTimeWebTest, Toggle(Domain.FeatureFlags.Toggles.MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880)]
	public class ScheduleApiControllerPossibiliesTest
	{
		public ScheduleApiController Target;
		public ICurrentScenario Scenario;
		public ILoggedOnUser User;
		public FakeScheduleDataReadScheduleStorage ScheduleData;
		public MutableNow Now;
		public FakeUserCulture Culture;
		public FakeUserTimeZone TimeZone;
		public FakePersonRequestRepository PersonRequestRepository;

		[Test, SetCulture("sv-SE"), Ignore("not prepare for it yet")]
		public void ShouldReturnPossibiliesForCurrentWeek()
		{
			var result = Target.FetchData(null, StaffingPossiblityType.Absence).Possibilities;
			result.Count(r => r.Date >= Now.LocalDateOnly()).Should().Be.EqualTo(4);
		}
	}
}
