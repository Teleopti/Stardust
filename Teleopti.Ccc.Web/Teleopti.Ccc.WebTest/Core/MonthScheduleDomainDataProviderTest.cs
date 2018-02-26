using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core
{
	[TestFixture]
	public class MonthScheduleDomainDataProviderTest
	{
		private FakeScheduleProvider scheduleProvider;
		private IMonthScheduleDomainDataProvider target;
		private ILicenseAvailability licenseAvailability;
		private IPermissionProvider permissionProvider;

		[SetUp]
		public void Setup()
		{
			scheduleProvider = new FakeScheduleProvider();
			permissionProvider = MockRepository.GenerateMock<IPermissionProvider>();
			licenseAvailability = MockRepository.GenerateMock<ILicenseAvailability>();

			target = new MonthScheduleDomainDataProvider(scheduleProvider,
				MockRepository.GenerateMock<ISeatOccupancyProvider>(),
				permissionProvider,
				licenseAvailability);
		}

		[Test]
		public void ShouldMapDate()
		{
			var today = DateOnly.Today;

			var result = target.Get(today, true);

			result.CurrentDate.Should().Be.EqualTo(today);
		}


		[Test]
		[SetCulture("sv-SE")]
		public void ShouldMapDays()
		{
			var date = new DateOnly(2014, 1, 11);
			scheduleProvider.AddScheduleDay(ScheduleDayFactory.Create(new DateOnly(2013, 12, 30)));

			var result = target.Get(date, true);

			result.Days.Count().Should().Be(1);
			result.Days.ElementAt(0).ScheduleDay.DateOnlyAsPeriod.DateOnly.Should().Be(new DateOnly(2013, 12, 30));
		}

		[Test]
		public void ShouldMapAsmPermissionToTrueWhenHavingBothLicenseAndPermission()
		{
			var today = DateOnly.Today;
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)).Return(true);
			licenseAvailability.Stub(x => x.IsLicenseEnabled(DefinedLicenseOptionPaths.TeleoptiCccAgentScheduleMessenger)).Return(true);

			var result = target.Get(today, true);

			result.AsmPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapAsmPermissionToFalseWhenNoAsmLicense()
		{
			var today = DateOnly.Today;
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)).Return(true);
			licenseAvailability.Stub(x => x.IsLicenseEnabled(DefinedLicenseOptionPaths.TeleoptiCccAgentScheduleMessenger)).Return(false);

			var result = target.Get(today, true);

			result.AsmPermission.Should().Be.False();
		}

		[Test]
		public void ShouldMapAsmPermissionToFalseWhenNoAsmPermission()
		{
			var today = DateOnly.Today;
			permissionProvider.Stub(x => x.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger)).Return(false);
			licenseAvailability.Stub(x => x.IsLicenseEnabled(DefinedLicenseOptionPaths.TeleoptiCccAgentScheduleMessenger)).Return(true);

			var result = target.Get(today, true);

			result.AsmPermission.Should().Be.False();
		}
	}
}