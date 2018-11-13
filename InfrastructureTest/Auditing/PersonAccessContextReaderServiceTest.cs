﻿using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Audit;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Auditing
{
	[TestFixture]
	[UnitOfWorkTest]
	[AllTogglesOn]
	public class PersonAccessContextReaderServiceTest //: IIsolateSystem
	{
		public IPersonAccessAuditRepository PersonAccessAuditRepository;
		public IPersonAccessContextReaderService Target;
		public IApplicationRoleRepository ApplicationRoleRepository;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public IPersonRepository PersonRepository;
		public MutableNow Now;


		//public void Isolate(IIsolate isolate)
		//{
		//	isolate.UseTestDouble<FakeUserCulture>().For<IUserCulture>();
		//	isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		//}

		[Test]
		public void ShouldLoadPersonAccessAuditContext()
		{
			var appRole = ApplicationRoleFactory.CreateRole("Superman", "The man");
			ApplicationRoleRepository.Add(appRole);
			dynamic role = new {RoleId = appRole.Id.GetValueOrDefault(), Name = appRole.DescriptionText};
			var person = PersonFactory.CreatePerson();
			PersonRepository.Add(person);
			CurrentUnitOfWork.Current().PersistAll();

			PersonAccessAuditRepository.Add(
				new PersonAccess(person, 
					person, 
					PersonAuditActionType.GrantRole.ToString(), 
					PersonAuditActionResult.Change.ToString(),
					JsonConvert.SerializeObject(role)));
			CurrentUnitOfWork.Current().PersistAll();
			var model = Target.LoadAll().Single();

			model.Data.Should().Be.EqualTo("Role: Superman Action: GrantRole");
		}

		[Test]
		public void ShouldPurgeAccordingToSetting()
		{
			Now.Is(DateTime.UtcNow);
			var person = PersonFactory.CreatePerson();
			PersonRepository.Add(person);
			var appRole = ApplicationRoleFactory.CreateRole("Superman", "The man");
			ApplicationRoleRepository.Add(appRole);
			dynamic role = new { RoleId = appRole.Id.GetValueOrDefault(), Name = appRole.DescriptionText };

			CurrentUnitOfWork.Current().PersistAll();
			var personAccess1 = new PersonAccess(person,
				person,
				PersonAuditActionType.GrantRole.ToString(),
				PersonAuditActionResult.Change.ToString(),
				JsonConvert.SerializeObject(role)){TimeStamp = Now.UtcDateTime()};
			var personAccess2 = new PersonAccess(person,
					person,
					PersonAuditActionType.GrantRole.ToString(),
					PersonAuditActionResult.Change.ToString(),
					JsonConvert.SerializeObject(role))
				{ TimeStamp = Now.UtcDateTime().AddMonths(-4) };
			PersonAccessAuditRepository.Add(personAccess1);
			PersonAccessAuditRepository.Add(personAccess2);

			CurrentUnitOfWork.Current().PersistAll();

			Target.PurgeAudits();
			CurrentUnitOfWork.Current().PersistAll();
			var loadedAudits = Target.LoadAll();
			loadedAudits.Count().Should().Be(1);
			loadedAudits.FirstOrDefault().TimeStamp.Should().Be.EqualTo(personAccess1.TimeStamp);
		}

	}
}