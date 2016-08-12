using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.Requests.PerformanceTest
{
	[System.ComponentModel.Category("ProcessBulkAbsenceRequest")]
	[DomainTest]
	public class ProcessBulkAbsenceRequestTest : ISetup
	{
		public IProcessMultipleAbsenceRequest Target;
		public WithUnitOfWork WithUnitOfWork;
		public IDataSourceScope DataSource;
		public IPersonRepository PersonRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public IAbsenceRepository AbsenceRepository;
		public IWorkflowControlSetRepository WorkflowControlSetRepository;
		public AsSystem AsSystem;
		public MutableNow Now;

		[Test]
		public void ShouldProcessMultipleAbsenceRequests()
		{
			IEnumerable<Guid> personIds = new List<Guid>
			{ // people from team 'FL_Sup_Sun7_71631' and 'FL_Sup_Sun6_00333'
				//DO NOT CHANGE THE ORDER OF THE GUIDS!
				new Guid("55E3A133-6305-4C9A-8AEA-A1410113C47D"),
				new Guid("87E6CEF0-388A-4365-94FA-A1410113C47D"),
				new Guid("391DB822-3936-4C4D-9634-A1410113C47D"),
				new Guid("47721DE4-A0CB-45EE-A123-A1410113C47D"),
				new Guid("DCF2EA04-3031-4436-A229-A1410113C47D"),
				new Guid("4886AEDD-E30F-416C-B5E8-A1410113C47D"),
				new Guid("C922055A-B4D0-4C06-B9AD-A1410113C47D"),
				new Guid("CADD42C6-5419-48DD-8514-A25B009AD59D"),
				new Guid("A35F2179-9A4B-40C9-BF7B-A27400997C79"),
				new Guid("42394840-F905-4B22-915F-A332008A5288"),
				new Guid("F25262A1-E3C3-4A54-B202-A33200B2E94F"),
				new Guid("AE476FA3-7A6C-4948-89C2-A3BF00D0577C"),
				new Guid("1EBFEE75-CE35-40FB-975E-A3BF00D0577C"),
				new Guid("E4C7A7A7-8D2C-4591-ACA4-A53D00F82C88"),
				new Guid("12728761-0ED6-422B-B2B5-A5E001051F1E"),
				new Guid("6069902E-5760-4DF4-B733-A5E00105B099"),
				new Guid("BF50C741-A780-4930-A64B-A5E00105F325")
			};
			
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
			AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			

			var personReqs = new List<IPersonRequest>();

			var absenceRequests = new List<NewAbsenceRequestCreatedEvent>();
			WithUnitOfWork.Do(() =>
			{
				var wfcs = WorkflowControlSetRepository.Get(new Guid("E97BC114-8939-4A70-AE37-A338010FFF19"));
				foreach (var period in wfcs.AbsenceRequestOpenPeriods)
				{
					period.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2016, 3, 1), new DateOnly(2099, 5, 30));
					period.StaffingThresholdValidator = new StaffingThresholdValidator();
					period.AbsenceRequestProcess = new GrantAbsenceRequest();
					var datePeriod = period as AbsenceRequestOpenDatePeriod;
					if(datePeriod != null)
						datePeriod.Period = period.OpenForRequestsPeriod;
				}
				WorkflowControlSetRepository.UnitOfWork.PersistAll();

				// load some persons
				var persons = PersonRepository.FindPeople(personIds);

				//load vacation
				IAbsence absence = AbsenceRepository.Get(new Guid("3A5F20AE-7C18-4CA5-A02B-A11C00F0F27F"));

			
				foreach (var person in persons)
				{
					personReqs.Add(createAbsenceRequest(person, absence));
				}



				foreach (var pReq in personReqs)
				{
					PersonRequestRepository.Add(pReq);
					PersonRequestRepository.UnitOfWork.PersistAll();

					absenceRequests.Add(
						new NewAbsenceRequestCreatedEvent()
						{
							PersonRequestId = pReq.Id.Value,
							InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
							LogOnBusinessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"),
							LogOnDatasource = "Teleopti WFM",
							Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z")
						}
						);
				}

				Target.Process(absenceRequests);

			});

			var expectedStatuses = new Dictionary<Guid, int>();
			var resultStatuses = new Dictionary<Guid, int>();

			expectedStatuses.Add(new Guid("BF50C741-A780-4930-A64B-A5E00105F325"), 2);
			expectedStatuses.Add(new Guid("6069902E-5760-4DF4-B733-A5E00105B099"), 2);
			expectedStatuses.Add(new Guid("12728761-0ED6-422B-B2B5-A5E001051F1E"), 2);
			expectedStatuses.Add(new Guid("E4C7A7A7-8D2C-4591-ACA4-A53D00F82C88"), 2);
			expectedStatuses.Add(new Guid("1EBFEE75-CE35-40FB-975E-A3BF00D0577C"), 4);
			expectedStatuses.Add(new Guid("AE476FA3-7A6C-4948-89C2-A3BF00D0577C"), 2);
			expectedStatuses.Add(new Guid("F25262A1-E3C3-4A54-B202-A33200B2E94F"), 4);
			expectedStatuses.Add(new Guid("42394840-F905-4B22-915F-A332008A5288"), 4);
			expectedStatuses.Add(new Guid("A35F2179-9A4B-40C9-BF7B-A27400997C79"), 4);
			expectedStatuses.Add(new Guid("CADD42C6-5419-48DD-8514-A25B009AD59D"), 4);
			expectedStatuses.Add(new Guid("C922055A-B4D0-4C06-B9AD-A1410113C47D"), 4);
			expectedStatuses.Add(new Guid("4886AEDD-E30F-416C-B5E8-A1410113C47D"), 4);
			expectedStatuses.Add(new Guid("DCF2EA04-3031-4436-A229-A1410113C47D"), 4);
			expectedStatuses.Add(new Guid("47721DE4-A0CB-45EE-A123-A1410113C47D"), 4);
			expectedStatuses.Add(new Guid("391DB822-3936-4C4D-9634-A1410113C47D"), 4);
			expectedStatuses.Add(new Guid("87E6CEF0-388A-4365-94FA-A1410113C47D"), 4);
			expectedStatuses.Add(new Guid("55E3A133-6305-4C9A-8AEA-A1410113C47D"), 2);
			

			WithUnitOfWork.Do(() =>
			{
				foreach (var req in personReqs)
				{
					var request = PersonRequestRepository.Get(req.Id.Value);

					int requestStatus = 0;

					if (request.IsApproved)
						requestStatus = 2;

					else if (request.IsDenied)
						requestStatus = 4;

					resultStatuses.Add(request.Person.Id.Value, requestStatus);
				}
			});

			CollectionAssert.AreEquivalent(expectedStatuses,resultStatuses);
		}


		private IPersonRequest createAbsenceRequest(IPerson person, IAbsence absence)
		{
			var req = new AbsenceRequest(absence,
				new DateTimePeriod(new DateTime(2016, 3, 10, 8, 0, 0, DateTimeKind.Utc),
					new DateTime(2016, 3, 10, 18, 0, 0, DateTimeKind.Utc)));
			return new PersonRequest(person) {Request = req};
		}


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new CommonModule(configuration));
			system.UseTestDouble<NewAbsenceRequestHandler>().For<NewAbsenceRequestHandler>();
			system.UseTestDouble<ProcessMultipleAbsenceRequest>().For<IProcessMultipleAbsenceRequest>();
			system.UseTestDouble<NoMessageSender>().For<IMessageSender>();
			system.AddService<Database>();
			system.AddModule(new TenantServerModule(configuration));

		}
	}


	public class ProcessMultipleAbsenceRequest : IProcessMultipleAbsenceRequest
	{
		private readonly NewAbsenceRequestHandler _newAbsenceRequestHandler;

		public ProcessMultipleAbsenceRequest(NewAbsenceRequestHandler newAbsenceRequestHandler)
		{
			_newAbsenceRequestHandler = newAbsenceRequestHandler;
		}

		public void Process(List<NewAbsenceRequestCreatedEvent> absenceRequests)
		{

			foreach (var req in absenceRequests)
			{
				_newAbsenceRequestHandler.Handle(req);
			}


		}
	}

	public interface IProcessMultipleAbsenceRequest
	{
		void Process(List<NewAbsenceRequestCreatedEvent> absenceRequests);
	}
}
