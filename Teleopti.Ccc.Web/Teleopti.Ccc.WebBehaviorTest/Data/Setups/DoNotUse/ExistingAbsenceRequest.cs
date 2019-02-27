using System;
using System.Drawing;
using System.Globalization;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.TestCommon.TestData.Core;


namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class ExistingAbsenceRequest : IUserDataSetup
	{
		public PersonRequest PersonRequest;
		public AbsenceRequest AbsenceRequest;

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			
			var today = DateTime.UtcNow.Date;

			var absenceRepository = AbsenceRepository.DONT_USE_CTOR(unitOfWork);
			var absence = AbsenceFactory.CreateAbsence(RandomName.Make(), RandomName.Make(), Color.FromArgb(210, 150, 150));
			absenceRepository.Add(absence);

			AbsenceRequest = new AbsenceRequest(absence, new DateTimePeriod(today, today.AddHours(5)));
			PersonRequest = new PersonRequest(person, AbsenceRequest) {Subject = "I need some vacation"};
			PersonRequest.TrySetMessage("This is just a short text that doesn't say anything, except explaining that it doesn't say anything");

			var requestRepository = new PersonRequestRepository(unitOfWork);

			requestRepository.Add(PersonRequest);
		}
	}

	public class ExistingApprovedAbsenceRequest : IUserDataSetup
	{
		public PersonRequest PersonRequest;
		public AbsenceRequest AbsenceRequest;

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var today = DateTime.UtcNow.Date;

			var absenceRepository = AbsenceRepository.DONT_USE_CTOR(unitOfWork);
			var absence = AbsenceFactory.CreateAbsence(RandomName.Make(), RandomName.Make(), Color.FromArgb(210, 150, 150));
			absenceRepository.Add(absence);
			
			AbsenceRequest = new AbsenceRequest(absence, new DateTimePeriod(today, today.AddHours(5)));
			PersonRequest = new PersonRequest(person, AbsenceRequest) { Subject = "I need some vacation" };
			PersonRequest.TrySetMessage("This is just a short text that doesn't say anything, except explaining that it doesn't say anything");
			PersonRequest.Pending();
			PersonRequest.Approve(new ApprovalServiceForTest(), new PersonRequestAuthorizationCheckerForTest());

			var requestRepository = new PersonRequestRepository(unitOfWork);

			requestRepository.Add(PersonRequest);
		}
	}

	public class ExistingDeniedAbsenceRequest : IUserDataSetup
	{
		private readonly IAbsence _absence;
		private readonly bool _isAutoDenied;
		private readonly string _denyReason;
		public PersonRequest PersonRequest;
		public AbsenceRequest AbsenceRequest;

		public ExistingDeniedAbsenceRequest(IAbsence absence, bool isAutoDenied)
		{
			_absence = absence;
			_isAutoDenied = isAutoDenied;
		}

		public ExistingDeniedAbsenceRequest(string denyReason)
		{
			_denyReason = denyReason;
		}

		public void Apply(ICurrentUnitOfWork unitOfWork, IPerson person, CultureInfo cultureInfo)
		{
			var today = DateTime.UtcNow.Date;
			IAbsence absence;
			if (_absence != null)
			{
				absence = _absence;
			}
			else
			{
				var absenceRepository = AbsenceRepository.DONT_USE_CTOR(unitOfWork);
				absence = AbsenceFactory.CreateAbsence(RandomName.Make(), RandomName.Make(), Color.FromArgb(210, 150, 150));
				absenceRepository.Add(absence);
			}

			AbsenceRequest = new AbsenceRequest(absence, new DateTimePeriod(today, today.AddHours(5)));
			PersonRequest = new PersonRequest(person, AbsenceRequest) { Subject = "I need some vacation" };
			PersonRequest.TrySetMessage("This is just a short text that doesn't say anything, except explaining that it doesn't say anything");
			PersonRequestDenyOption denyOption = PersonRequestDenyOption.AutoDeny;
			if (!_isAutoDenied)
			{
				PersonRequest.Pending();
				denyOption = PersonRequestDenyOption.None;
			}
			PersonRequest.Deny(_denyReason, new PersonRequestAuthorizationCheckerForTest(), null, denyOption);

			var requestRepository = new PersonRequestRepository(unitOfWork);

			requestRepository.Add(PersonRequest);
		}
	}
}