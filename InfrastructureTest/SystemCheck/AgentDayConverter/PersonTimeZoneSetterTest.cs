using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.SystemCheck.AgentDayConverter;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.SystemCheck.AgentDayConverter
{
	[TestFixture]
	public class PersonTimeZoneSetterTest : DatabaseTestWithoutTransaction
	{
		private IPerson snubbe;

		protected override void SetupForRepositoryTestWithoutTransaction()
		{
			snubbe = new Person();
			snubbe.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			PersistAndRemoveFromUnitOfWork(snubbe);
			UnitOfWork.PersistAll();
		}

		[Test]
		public void ShouldPersistTimeZone()
		{
			var newTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
			new PersonTimeZoneSetter().ExecuteConverterAndWrapInTransaction(snubbe.Id.Value, newTimeZone);

			new PersonRepository(UnitOfWork).Get(snubbe.Id.Value).PermissionInformation.DefaultTimeZone().Id
																.Should().Be.EqualTo(newTimeZone.Id);
		}

		[Test]
		public void ShouldNotPersistTimeZoneForWrongPerson()
		{
			var oldTimeZone = snubbe.PermissionInformation.DefaultTimeZone();

			new PersonTimeZoneSetter().ExecuteConverterAndWrapInTransaction(Guid.NewGuid(), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

			new PersonRepository(UnitOfWork).Get(snubbe.Id.Value).PermissionInformation.DefaultTimeZone()
													.Should().Be.EqualTo(oldTimeZone);
		}

		protected override void TeardownForRepositoryTest()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				new PersonRepository(uow).Remove(snubbe);
				uow.PersistAll();
			}
		}
	}
}