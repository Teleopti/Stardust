using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;


namespace Teleopti.Ccc.WebTest.Core.WeekSchedule.DataProvider
{
	[TestFixture]
	[DomainTest]
	public class AbsenceTypesProviderTest : IIsolateSystem
	{
		public AbsenceTypesProvider Target;
		public FakeToggleManager ToggleManager;
		public FakeAbsenceRepository AbsenceRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakePersonRepository PersonRepository;
		public FakeBusinessUnitRepository BusinessUnitRepository;
		public FakeThreadPrincipalContext ThreadPrincipalContext;
		public FakeDatabase Database;
		public MutableNow _now;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<AbsenceTypesProvider>().For<IAbsenceTypesProvider>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}

		[Test]
		public void ShouldReturnAbsenceTypes()
		{
			ToggleManager.Disable(Toggles.MyTimeWeb_AbsenceRequest_LimitAbsenceTypes_77446);
			var ret = Target.GetRequestableAbsences();
			ret.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldReturnReportableAbsenceTypes()
		{
			var ret = Target.GetReportableAbsences();
			ret.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldGetAbsencesByWFCSet()
		{
			var requestDate = DateOnly.Today;
			_now.Is(requestDate.Date);
			var expactedAbsenceName = "requestableAbsence";
			var openPeriod = new DateOnlyPeriod(requestDate.AddDays(-2), requestDate.AddDays(2));
			setupData(expactedAbsenceName, openPeriod);
			ToggleManager.Enable(Toggles.MyTimeWeb_AbsenceRequest_LimitAbsenceTypes_77446);
			var ret = Target.GetRequestableAbsences();
			ret.ToList().First().Name.Should().Be.EqualTo(expactedAbsenceName);
		}

		[Test]
		public void ShouldNotGetAbsenceIfItIsNotRequestable()
		{
			var requestDate = DateOnly.Today;
			_now.Is(requestDate.Date);
			var expactedAbsenceName = "requestableAbsence";
			var openPeriod = new DateOnlyPeriod(requestDate.AddDays(-2), requestDate.AddDays(2));
			setupData(expactedAbsenceName, openPeriod, false);
			ToggleManager.Enable(Toggles.MyTimeWeb_AbsenceRequest_LimitAbsenceTypes_77446);
			var ret = Target.GetRequestableAbsences();
			ret.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotGetAbsenceWhenRequestDateOutOfAbsenceOpenPeriod()
		{
			var requestDate = DateOnly.Today;
			_now.Is(requestDate.Date);
			var expactedAbsenceName = "requestableAbsence";
			var openPeriod = new DateOnlyPeriod(requestDate.AddDays(2), requestDate.AddDays(4));
			setupData(expactedAbsenceName, openPeriod);
			ToggleManager.Enable(Toggles.MyTimeWeb_AbsenceRequest_LimitAbsenceTypes_77446);
			var ret = Target.GetRequestableAbsences();
			ret.Should().Be.Empty();
		}

		private void setupData(string absenceName, DateOnlyPeriod openPeriod, bool requestable = true)
		{
			var personId = Guid.NewGuid();
			var wfcId = Guid.NewGuid();
			var absenceOpenRequest = new AbsenceRequestOpenDatePeriod
			{
				Absence = new Absence{Description = new Description(absenceName) , Requestable = requestable },
				OpenForRequestsPeriod = openPeriod
			}; 

			Database.WithOpenAbsenceRequestWorkflowConrolSet(wfcId, absenceOpenRequest)
				.WithPerson(personId);

			setPrincipal(personId);
		}

		private void setPrincipal(Guid personId)
		{
			var currentUser = PersonRepository.Get(personId);
			LoggedOnUser.SetFakeLoggedOnUser(currentUser);
			var principal = new TeleoptiPrincipalForLegacy(
				new TeleoptiIdentity(
					"Fake Login",
					null,
					BusinessUnitRepository.LoadAll().FirstOrDefault(),
					null,
					null
				),
				currentUser);
			ThreadPrincipalContext.SetCurrentPrincipal(principal);
		}
	}
}
