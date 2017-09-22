using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Settings.ViewModelFactory
{
	[TestFixture]
	public class SettingsPermissionViewModelFactoryTest
	{
		private SettingsPermissionViewModelFactory _target;
		private MockRepository mocks;
		private IPermissionProvider _permissionProvider;
		private ILoggedOnUser _loggedOnUser;
		private IPerson _person;


		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			_permissionProvider = mocks.Stub<IPermissionProvider>();
			_loggedOnUser = mocks.Stub<ILoggedOnUser>();
			_person = PersonFactory.CreatePerson();
			_person.WorkflowControlSet = new WorkflowControlSet();
			_target = new SettingsPermissionViewModelFactory(_permissionProvider, _loggedOnUser);

		}

		[Test]
		public void ShouldLoadPermission()
		{
			using (mocks.Record())
			{
				Expect.On(_loggedOnUser).Call(_loggedOnUser.CurrentUser()).Return(_person);
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
				Expect.On(_loggedOnUser).Call(_loggedOnUser.CurrentUser()).Return(_person);
			}
			using (mocks.Playback())
			{
				var result = _target.CreateViewModel();
				Assert.IsTrue(result.HasWorkflowControlSet);
			}
		}
	}
}
