using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Intraday;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Requests.PerformanceTest.AbsenceRequests
{
	[IntradayRequestPerformanceTest]
	public class IntradayAbsenceRequestTest
	{
		public IAbsenceRepository AbsenceRepository;
		public IPersonRepository PersonRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public ISkillRepository SkillRepository;
		//public INewAbsenceRequestHandler Target;
		public QueuedAbsenceRequestFastIntradayHandler Target;
		public WithUnitOfWork WithUnitOfWork;
		public IWorkflowControlSetRepository WorkflowControlSetRepository;
		public FakeConfigReader ConfigReader;
		public ResourceCalculateReadModelUpdater ResourceCalculateReadModelUpdater;
		public IDataSourceScope DataSource;
		public AsSystem AsSystem;
		public ICurrentUnitOfWork CurrentUnitOfWork;
	    public IMutateNow Now;

		[Test]
		public void ShouldApproveAbsenceRequest()
		{
			ConfigReader.FakeSetting("FakeIntradayUtcStartDateTime", "2016-03-15 05:00");

			var requestPeriod = new DateTimePeriod(new DateTime(2016, 3, 15, 07, 0, 0, DateTimeKind.Utc),
													   new DateTime(2016, 3, 15, 07, 15, 0, DateTimeKind.Utc));

			var request = prepareAndCreateRequest(requestPeriod);
			var newRequestEvent = new NewAbsenceRequestCreatedEvent() { PersonRequestId = request.Id.GetValueOrDefault() };


			WithUnitOfWork.Do(() => Target.Handle(newRequestEvent));


			WithUnitOfWork.Do(() => request = PersonRequestRepository.Get(request.Id.GetValueOrDefault()));
			Assert.AreEqual(2, getRequestStatus(request));
		}

		[Test]
		public void ShouldDenyAbsenceRequest()
		{
			ConfigReader.FakeSetting("FakeIntradayUtcStartDateTime", "2016-03-14 05:00");

			var requestPeriod = new DateTimePeriod(new DateTime(2016, 3, 14, 8, 15, 0, DateTimeKind.Utc),
													   new DateTime(2016, 3, 14, 10, 0, 0, DateTimeKind.Utc));

			var request = prepareAndCreateRequest(requestPeriod);
			var newRequestEvent = new NewAbsenceRequestCreatedEvent() { PersonRequestId = request.Id.GetValueOrDefault() };


			WithUnitOfWork.Do(() => Target.Handle(newRequestEvent));


			WithUnitOfWork.Do(() => request = PersonRequestRepository.Get(request.Id.GetValueOrDefault()));
			Assert.AreEqual(4, getRequestStatus(request));
		}


		private int getRequestStatus(IPersonRequest request)
		{
			var requestStatus = 10;

			if (request.IsApproved)
				requestStatus = 2;
			else if (request.IsPending)
				requestStatus = 0;
			else if (request.IsNew)
				requestStatus = 3;
			else if (request.IsCancelled)
				requestStatus = 6;
			else if (request.IsWaitlisted && request.IsDenied)
				requestStatus = 5;
			else if (request.IsAutoDenied)
				requestStatus = 4;
			else if (request.IsDenied)
				requestStatus = 1;


			return requestStatus;
		}

		private IPersonRequest prepareAndCreateRequest(DateTimePeriod requestPeriod)
		{
			IPersonRequest request = null;
			WithUnitOfWork.Do(() =>
			{
				var absence = AbsenceRepository.Get(new Guid("3A5F20AE-7C18-4CA5-A02B-A11C00F0F27F"));
				var person = PersonRepository.Get(new Guid("FBA68B4E-AFB2-4502-880E-A39D008D7F86")); //Viktor Hansson, NAY209, FL_Sup_Far1_00065
				var personPeriod = (PersonPeriod)person.PersonPeriodCollection.First();

				foreach (var skill in person.Period(new DateOnly(2016,03,15)).PersonSkillCollection.Where(y => !(y.Skill.CascadingIndex > 0)))
				{
					personPeriod.DeletePersonSkill(skill);
				}

				var wfcs = WorkflowControlSetRepository.Get(new Guid("E97BC114-8939-4A70-AE37-A338010FFF19")); //Consumer Support
				foreach (var period in wfcs.AbsenceRequestOpenPeriods)
				{
					period.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2016, 3, 1), new DateOnly(2099, 5, 30));
					period.StaffingThresholdValidator = new StaffingThresholdValidator();
					period.AbsenceRequestProcess = new GrantAbsenceRequest();
					var datePeriod = period as AbsenceRequestOpenDatePeriod;
					if (datePeriod != null)
						datePeriod.Period = period.OpenForRequestsPeriod;
				}

				var supportBb = SkillRepository.Get(new Guid("0165E0EA-210A-4393-B25A-A15000925656"));
				supportBb.SetCascadingIndex(1);

				request = createAbsenceRequest(person, absence, requestPeriod);

				PersonRequestRepository.Add(request);
			});
			return request;
		}

		private IPersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod dateTimePeriod)
		{
			var req = new AbsenceRequest(absence, dateTimePeriod);
			return new PersonRequest(person) {Request = req};
		}


	
	}




}