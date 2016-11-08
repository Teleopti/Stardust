using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Settings.ViewModelFactory
{
	[TestFixture]
	public class SettingsPermissionViewModelFactoryTest
	{
		private SettingsPermissionViewModelFactory _target;
		private MockRepository mocks;
		private IPermissionProvider _permissionProvider;
		private ILoggedOnUser _loggedOnUser;


		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			_permissionProvider = mocks.Stub<IPermissionProvider>();
			_loggedOnUser = mocks.Stub<ILoggedOnUser>();
			_target = new SettingsPermissionViewModelFactory(_permissionProvider, _loggedOnUser);

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

		[Test]
		public void ShouldLoadWorkflowControlSet()
		{
			using (mocks.Record())
			{
				var person = PersonFactory.CreatePerson();
				person.WorkflowControlSet = new WorkflowControlSet();
				Expect.On(_loggedOnUser).Call(_loggedOnUser.CurrentUser()).Return(person);
			}
			using (mocks.Playback())
			{
				var result = _target.CreateViewModel();
				Assert.IsTrue(result.HasWorkflowControlSet);
			}
		}
	}
}
