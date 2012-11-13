﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class NHibernateUnitOfWorkCommitCallbackTest
	{
		[Test]
		public void ShouldCallTxCallbackWhenSuccessfulCommit()
		{
			var isCalled = false;
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.AfterSuccessfulTx(dummy => isCalled = true);
				uow.PersistAll();
				isCalled.Should().Be.True();				
			}
		}

		[Test]
		public void ShouldNotCallTxCallbackWhenRollbacked()
		{
			var isCalled = false;
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.AfterSuccessfulTx(dummy => isCalled = true);
			}
			isCalled.Should().Be.False();
		}

		[Test]
		public void ShouldNotCallTxCallbackIfException()
		{
			var isCalled = false;
			var person = new Person();
			var correctEx = false;
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				person.PermissionInformation.SetDefaultTimeZone(new CccTimeZoneInfo(TimeZoneInfo.Local));
				new PersonRepository(uow).Add(person);
				uow.PersistAll();
			}
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(person);
				uow.AfterSuccessfulTx(dummy => isCalled = true);
				//too long email
				var email = new string('c', 300);
				person.Email = email;
				try
				{
					uow.PersistAll();
				}
				catch (DataSourceException)
				{
					correctEx = true;
				}
			}

			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				person.Email = "ok";
				new PersonRepository(uow).Remove(person);
				uow.PersistAll();
			}
			correctEx.Should().Be.True();
			isCalled.Should().Be.False();
		}

		[Test]
		public void ShouldWorkWithCurrentUnitOfWork()
		{
			var isCalled = false;
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				UnitOfWorkFactory.Current.CurrentUnitOfWork().AfterSuccessfulTx(dummy => isCalled = true);
				UnitOfWorkFactory.Current.CurrentUnitOfWork().PersistAll();
				isCalled.Should().Be.True();
			}
		}
	}
}