using System;
using NUnit.Framework;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Interfaces.Domain;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.ViewModelFactory
{
	[TestFixture, IoCTest]
	public class StaffingPossibilityViewModelFactoryTest : ISetup
	{
		public IStaffingPossibilityViewModelFactory StaffingPossibilityViewModelFactory;
		public FakeLoggedOnUser LoggedOnUser;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			system.UseTestDouble<StaffingPossibilityViewModelFactory>().For<IStaffingPossibilityViewModelFactory>();
		}
	}
}
