using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Settings.Mapping
{
	[TestFixture]
	public class SettingsPermissionViewModelFactoryTest
	{
		private SettingsPermissionViewModelFactory _target;
		private MockRepository mocks;
		private IPermissionProvider _permissionProvider;


		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			_permissionProvider = mocks.Stub<IPermissionProvider>();
			_target = new SettingsPermissionViewModelFactory(_permissionProvider);

		}

		[Test]
		public void ShouldLoadPermission()
		{
			using (mocks.Record())
			{
				Expect.On(_permissionProvider).Call(_permissionProvider.HasApplicationFunctionPermission(
					DefinedRaptorApplicationFunctionPaths.ShareCalendar)).Return(true);
			}
			using (mocks.Playback())
			{
				var result = _target.CreateViewModel();
				Assert.IsTrue(result.ShareCalendarPermission);
			}
		}
	}
}
