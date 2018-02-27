using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core
{
	[FullPermissions]
	[DomainTest]
	[TestFixture]
	public class MonthScheduleDomainDataProviderTest : ISetup
	{
		public FakeScheduleProvider ScheduleProvider;
		public IMonthScheduleDomainDataProvider Target;
		public ICurrentDataSource CurrentDataSource;
		public FullPermission Authorization;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<MonthScheduleDomainDataProvider>().For<IMonthScheduleDomainDataProvider>();
			system.UseTestDouble<FakeScheduleProvider>().For<IScheduleProvider>();
			system.UseTestDouble<SeatOccupancyProvider>().For<ISeatOccupancyProvider>();
		}

		[Test]
		public void ShouldMapDate()
		{
			var today = DateOnly.Today;

			var result = Target.Get(today, true);

			result.CurrentDate.Should().Be.EqualTo(today);
		}


		[Test]
		[SetCulture("sv-SE")]
		public void ShouldMapDays()
		{
			var date = new DateOnly(2014, 1, 11);
			ScheduleProvider.AddScheduleDay(ScheduleDayFactory.Create(new DateOnly(2013, 12, 30)));

			var result = Target.Get(date, true);

			result.Days.Count().Should().Be(1);
			result.Days.ElementAt(0).ScheduleDay.DateOnlyAsPeriod.DateOnly.Should().Be(new DateOnly(2013, 12, 30));
		}

		[Test]
		public void ShouldMapAsmPermissionToTrueWhenHavingBothLicenseAndPermission()
		{
			var today = DateOnly.Today;
			var licenseActivator = LicenseProvider.GetLicenseActivator(new AsmFakeLicenseService(true));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var result = Target.Get(today, true);

			result.AsmPermission.Should().Be.True();
		}

		[Test]
		public void ShouldMapAsmPermissionToFalseWhenNoAsmLicense()
		{
			var today = DateOnly.Today;
			var licenseActivator = LicenseProvider.GetLicenseActivator(new AsmFakeLicenseService(false));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var result = Target.Get(today, true);

			result.AsmPermission.Should().Be.False();
		}

		[Test]
		public void ShouldMapAsmPermissionToFalseWhenNoAsmPermission()
		{
			Authorization.AddToBlackList(DefinedRaptorApplicationFunctionPaths.AgentScheduleMessenger);
			var today = DateOnly.Today;
			var licenseActivator = LicenseProvider.GetLicenseActivator(new AsmFakeLicenseService(true));
			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var result = Target.Get(today, true);

			result.AsmPermission.Should().Be.False();
		}
	}
}