using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Notification
{
	[TestFixture, DomainTest]
	public class UserDeviceServiceTest : ISetup
	{
		public UserDeviceService Target;
		public FakeUserDeviceRepository UserDeviceRepository;
		public FakeLoggedOnUser LogonUser;
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}

		[Test]
		public void ShouldStoreUserToken()
		{
			Target.StoreUserDevice("asdfasdf");

			var result = UserDeviceRepository.Find(LogonUser.CurrentUser());
			result.Single().Token.Should().Be.EqualTo("asdfasdf");
		}
		
		[Test]
		public void ShouldNotStoreDuplicateUserToken()
		{
			Target.StoreUserDevice("asdfasdf");
			Target.StoreUserDevice("asdfasdf");
			var result = UserDeviceRepository.Find(LogonUser.CurrentUser());
			result.Single().Token.Should().Be.EqualTo("asdfasdf");
		}
		
		[Test]
		public void ShouldNotStoreEmptyUserToken()
		{
			Target.StoreUserDevice("");
			var result = UserDeviceRepository.Find(LogonUser.CurrentUser());
			result.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetUserTokens()
		{
			var token = "asdfkjlkjasdf-asdfkjlkjasdf-asdfkjlkjasdf";
			UserDeviceRepository.Add(new UserDevice
			{
				Owner = LogonUser.CurrentUser(),
				Token = token
			});
			
				var result = Target.GetUserTokens();
				result.Single().Should().Be.EqualTo(token);
		}

		[Test]
		public void ShouldSaveTokenForLastUser()
		{
			Target.StoreUserDevice("asdfasdf");
			var person = PersonFactory.CreatePerson().WithId();
			LogonUser.SetFakeLoggedOnUser(person);
			Target.StoreUserDevice("asdfasdf");
			var result = UserDeviceRepository.FindByToken("asdfasdf");
			result.Owner.Id.Should().Be.EqualTo(person.Id);


		}
	}
}
