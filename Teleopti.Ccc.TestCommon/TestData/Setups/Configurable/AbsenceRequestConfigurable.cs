using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class AbsenceRequestConfigurable : IUserDataSetup
	{
		public string Absence { get; set; }

		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public string Status { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{

			IAbsence absence;
			if (Absence != null)
			{
				absence = new AbsenceRepository(currentUnitOfWork).LoadAll().Single(a => a.Name == Absence);
			}
			else
			{
				absence  = AbsenceFactory.CreateAbsence("Legacy common absence", "LCA", Color.FromArgb(210, 150, 150));
				var absenceRepository = new AbsenceRepository(currentUnitOfWork);
				absenceRepository.Add(absence);
			}

			var absenceRequest = new AbsenceRequest(absence, new DateTimePeriod(DateTime.SpecifyKind(StartTime, DateTimeKind.Utc), DateTime.SpecifyKind(EndTime, DateTimeKind.Utc)));
			var personRequest = new PersonRequest(user, absenceRequest) { Subject = "I need some vacation" };
			personRequest.TrySetMessage("This is some text that is just here to fill a space and demonstrate how this will behave when we have lots and lots of character is a long long text that doesnt really mean anything at all.");

			var requestRepository = new PersonRequestRepository(currentUnitOfWork);
			var personAbsenceRepository = new PersonAbsenceRepository(currentUnitOfWork);
			IPersonAbsence personAbsence = null;

			if (Status == "AutoDenied")
			{
				personRequest.Deny (null, null, new PersonRequestAuthorizationCheckerForTest(), PersonRequestDenyOption.AutoDeny);
			}

			if (Status == "Pending")
			{
				personRequest.ForcePending();
			}

			if (Status == "Approved")
			{
				personRequest.ForcePending();
				personRequest.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest(), true);
				var scenarioRepository = new ScenarioRepository(currentUnitOfWork);
				var scenario = scenarioRepository.LoadDefaultScenario();
				personAbsence = createPersonAbsence(absence, personRequest, scenario);
			}

			requestRepository.Add(personRequest);
			if (personAbsence != null)
			{
				personAbsenceRepository.Add(personAbsence);
			}
		}

		private PersonAbsence createPersonAbsence(IAbsence absence, IPersonRequest personRequest, IScenario scenario)
		{
			var absenceLayer = new AbsenceLayer(absence, personRequest.Request.Period);
			var personAbsence = new PersonAbsence(personRequest.Person, scenario, absenceLayer, personRequest).WithId();
			return personAbsence;
		}
	}
}