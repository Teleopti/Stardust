using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.WebTest.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.TeamSchedule.ViewModelFactory
{
	[TestFixture]
	public class TeamScheduleViewModelFactoryTest
	{
		[Test]
		public void PermissionForShiftTrade_WhenAgentHasNoPermissionToViewShiftTrade_ShouldBFalse()
		{
			var fakeUserTimeZone = new FakeUserTimeZone();
			var target = new TeamScheduleViewModelFactory(new FakePermissionProvider(),
				new TeamScheduleViewModelMapper(fakeUserTimeZone,
					new CreateHourText(fakeUserTimeZone, new SwedishCulture()), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))),
				new TeamScheduleDomainDataMapper(new FakeSchedulePersonProvider(new IPerson[0]), new FakeScheduleProvider(),
					new TeamScheduleProjectionForMtwForMtwProvider(new ProjectionProvider()), fakeUserTimeZone));

			var result = target.CreateViewModel(DateOnly.Today, new Guid());
			Assert.That(result.ShiftTradePermisssion, Is.True);			
		}

		[Test]
		public void PermissionForShiftTrade_WhenAgentHasPermissionToViewShiftTrade_ShouldBeTrue()
		{
			var fakeUserTimeZone = new FakeUserTimeZone();
			var target = new TeamScheduleViewModelFactory(new FakePermissionProvider(),
				new TeamScheduleViewModelMapper(fakeUserTimeZone,
					new CreateHourText(fakeUserTimeZone, new SwedishCulture()), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))),
				new TeamScheduleDomainDataMapper(new FakeSchedulePersonProvider(new IPerson[0]), new FakeScheduleProvider(),
					new TeamScheduleProjectionForMtwForMtwProvider(new ProjectionProvider()), fakeUserTimeZone));

			var result = target.CreateViewModel(DateOnly.Today, new Guid());
			Assert.That(result.ShiftTradePermisssion, Is.True);
		}

		[Test]
		public void PermissionForShiftTradeBulletinBoard_WhenAgentHasPermissionToViewShiftTradeBulletinBoard_ShouldBeTrue()
		{
			var fakeUserTimeZone = new FakeUserTimeZone();
			var target = new TeamScheduleViewModelFactory(new FakePermissionProvider(),
				new TeamScheduleViewModelMapper(fakeUserTimeZone,
					new CreateHourText(fakeUserTimeZone, new SwedishCulture()), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))),
				new TeamScheduleDomainDataMapper(new FakeSchedulePersonProvider(new IPerson[0]), new FakeScheduleProvider(),
					new TeamScheduleProjectionForMtwForMtwProvider(new ProjectionProvider()), fakeUserTimeZone));

			var result = target.CreateViewModel(DateOnly.Today, new Guid());
			Assert.That(result.ShiftTradeBulletinBoardPermission, Is.True);
		}		
		
		[Test]
		public void PermissionForShiftTradeBulletinBoard_WhenAgentHasPermissionToViewShiftTradeBulletinBoardWithDefaultContructor_ShouldBeTrue()
		{
			var fakeUserTimeZone = new FakeUserTimeZone();
			var target = new TeamScheduleViewModelFactory(new FakePermissionProvider(),
				new TeamScheduleViewModelMapper(fakeUserTimeZone,
					new CreateHourText(fakeUserTimeZone, new SwedishCulture()), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository()))),
				new TeamScheduleDomainDataMapper(new FakeSchedulePersonProvider(new IPerson[0]), new FakeScheduleProvider(),
					new TeamScheduleProjectionForMtwForMtwProvider(new ProjectionProvider()), fakeUserTimeZone));

			var result = target.CreateViewModel();
			Assert.That(result.ShiftTradeBulletinBoardPermission, Is.True);
		}
	}
}
