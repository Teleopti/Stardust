using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	[DatabaseTest]
	public class UserDevicesRepositoryTest
	{
		public ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;
		public IPersonRepository PersonRepository;
		public IUserDeviceRepository Target;
		public WithUnitOfWork WithUnitOfWork;

		private IPerson person;
		private IPerson person2;

		[Test]
		public void ShouldAddDevice()
		{
			setUpPerson();
			WithUnitOfWork.Do(() => Target.Add(new UserDevice
			{
				Owner = person,
				Token = "token1"
			}));

			WithUnitOfWork.Do(() =>
			{
				var device = Target.Find(person).Single();
				device.Token.Should().Be.EqualTo("token1");
				device.Owner.Id.Should().Be.EqualTo(person.Id);
			});
		}


		[Test]
		public void ShouldFindUserDeviceByToken()
		{
			setUpPerson();

			WithUnitOfWork.Do(() => Target.Add(new UserDevice
			{
				Owner = person,
				Token = "token1"
			}));

			WithUnitOfWork.Do(() =>
			{
				var device = Target.FindByToken("token1");
				device.Owner.Id.Should().Be.EqualTo(person.Id);
				device.Token.Should().Be.EqualTo("token1");
			});
		}

		[Test]
		public void ShouldFindUserDeviceByPerson()
		{
			setUpPerson();

			WithUnitOfWork.Do(() =>
			{
				Target.Add(new UserDevice
				{
					Owner = person,
					Token = "token1"
				});

				Target.Add(new UserDevice
				{
					Owner = person,
					Token = "token2"
				});
			});

			WithUnitOfWork.Do(() =>
			{
				var devices = Target.Find(person).OrderBy(d => d.Token).ToArray();
				devices.Count().Should().Be.EqualTo(2);

				devices[0].Token.Should().Be.EqualTo("token1");
				devices[0].Owner.Id.Should().Be.EqualTo(person.Id);
				devices[1].Token.Should().Be.EqualTo("token2");
				devices[1].Owner.Id.Should().Be.EqualTo(person.Id);
			});
		}

		[Test]
		public void ShouldThrowAExceptionWhenAddDuplicateUserDeviceToken()
		{
			setUpPerson(true);

			WithUnitOfWork.Do(() => Target.Add(new UserDevice
			{
				Owner = person,
				Token = "token1"
			}));

			Assert.Throws<Infrastructure.Foundation.ConstraintViolationException>(() =>
			{
				WithUnitOfWork.Do(() => Target.Add(new UserDevice
				{
					Owner = person2,
					Token = "token1"
				}));
			});
		}

		[Test]
		public void ShouldRemoveUserDeviceByTokens()
		{
			setUpPerson();

			WithUnitOfWork.Do(() =>
			{
				Target.Add(new UserDevice
				{
					Owner = person,
					Token = "newToken"
				});
				Target.Add(new UserDevice
				{
					Owner = person,
					Token = "anotherToken"
				});
			});

			WithUnitOfWork.Do(() => Target.Remove("newToken", "anotherToken"));

			WithUnitOfWork.Do(() => Target.Find(person).Should().Be.Empty());
		}

		private void setUpPerson(bool addAnother = false)
		{
			WithUnitOfWork.Do(() =>
			{
				person = PersonFactory.CreatePerson("Person1");
				PersonRepository.Add(person);
				if (addAnother)
				{
					person2 = PersonFactory.CreatePerson("Person2");
					PersonRepository.Add(person2);
				}
			});

		}
	}


}