using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.DomainTest.Scheduling.Tracking
{
	[DomainTest]
	[LoggedOff]
	[DefaultData]
	[RealPermissions]
	public class PersonAccountProjectionServicePermissionTest
	{
		public FakeDatabase Database;
		public FakePersonRepository PersonRepository;
		public FakePersonAbsenceRepository PersonAbsenceRepository;
		public IScheduleStorage ScheduleStorage;
		public ILogOnOff LogOnOff;

		[TestCase(true, ExpectedResult = true)]
		[TestCase(false, ExpectedResult = true)]
		public bool ShouldNotConsiderPermissionOnUnpublishedSchedule(bool havePermissionToViewUnpublishedSchedule)
		{
			var userId = Guid.NewGuid();
			Database.WithTenant("_").WithPerson(userId, "_");
			var applicationFunctions = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions;
			foreach (var applicationFunction in applicationFunctions)
			{
				if (applicationFunction.FunctionPath.Equals(DefinedRaptorApplicationFunctionPaths.All))
					continue;

				if (!havePermissionToViewUnpublishedSchedule && applicationFunction.FunctionPath.Equals(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
					continue;

				Database.WithRole(AvailableDataRangeOption.Everyone, applicationFunction.FunctionPath);
			}

			var user = PersonRepository.Load(userId);
			LogOnOff.LogOn("_", user, Database.CurrentBusinessUnitId());
			var date = new DateOnly(2001, 1, 1);
			var absence = new Absence();
			var scenario = new Scenario();
			var personAbsenceAccount = new PersonAbsenceAccount(user, absence);
			var account = new AccountDay(date);
			personAbsenceAccount.Add(account);
			PersonAbsenceRepository.Add(new PersonAbsence(user, scenario, new AbsenceLayer(absence, date.ToDateTimePeriod(user.PermissionInformation.DefaultTimeZone()))));
			var target = new PersonAccountProjectionService(account);
			var days = target.CreateProjection(ScheduleStorage, scenario);

			return days.First().PersonAbsenceCollection().Length.Equals(1);
		}
	}
}
