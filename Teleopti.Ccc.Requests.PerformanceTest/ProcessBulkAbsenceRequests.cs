using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Absence;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Hangfire;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Messaging.Client;

namespace Teleopti.Ccc.Requests.PerformanceTest
{
	[RequestPerformanceTest]
	public class ProcessBulkAbsenceRequests 
	{
		public IAbsenceRepository AbsenceRepository;
		public AsSystem AsSystem;
		public IDataSourceScope DataSource;
		public IPersonRepository PersonRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public IProcessMultipleAbsenceRequest Target;
		public WithUnitOfWork WithUnitOfWork;
		public IWorkflowControlSetRepository WorkflowControlSetRepository;
		public IBudgetGroupRepository BudgetGroupRepository;
		public IBudgetDayRepository BudgetDayRepository;
		public IScenarioRepository ScenarioRepository;
		public IBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldProcessMultipleAbsenceRequests()
		{
			IEnumerable<Guid> personIds = new List<Guid>
			{
				// people from team 'FL_Sup_Sun7_71631' and 'FL_Sup_Sun6_00333'
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
			var absenceRequestIds = new List<Guid>();

			WithUnitOfWork.Do(() =>
			{
				var wfcs = WorkflowControlSetRepository.Get(new Guid("E97BC114-8939-4A70-AE37-A338010FFF19"));
				foreach (var period in wfcs.AbsenceRequestOpenPeriods)
				{
					period.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2016, 3, 1), new DateOnly(2099, 5, 30));
					period.StaffingThresholdValidator = new StaffingThresholdValidator();
					period.AbsenceRequestProcess = new GrantAbsenceRequest();
					var datePeriod = period as AbsenceRequestOpenDatePeriod;
					if (datePeriod != null)
						datePeriod.Period = period.OpenForRequestsPeriod;
				}
#pragma warning disable 618
				WorkflowControlSetRepository.UnitOfWork.PersistAll();
#pragma warning restore 618

				// load some persons
				var persons = PersonRepository.FindPeople(personIds);

				//load vacation
				var absence = AbsenceRepository.Get(new Guid("3A5F20AE-7C18-4CA5-A02B-A11C00F0F27F"));


				foreach (var person in persons)
				{
					personReqs.Add(createAbsenceRequest(person, absence));
				}


				foreach (var pReq in personReqs)
				{
					PersonRequestRepository.Add(pReq);
#pragma warning disable 618
					PersonRequestRepository.UnitOfWork.PersistAll();
#pragma warning restore 618

					absenceRequestIds.Add(pReq.Id.Value);
				}

				var newMultiAbsenceRequestsCreatedEvent = new NewMultiAbsenceRequestsCreatedEvent()
				{
					PersonRequestIds = absenceRequestIds,
					InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
					LogOnBusinessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"),
					LogOnDatasource = "Teleopti WFM",
					Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z")
				};

				Target.Process(newMultiAbsenceRequestsCreatedEvent);
			});

			var expectedStatuses = new Dictionary<Guid, int>();
			var resultStatuses = new Dictionary<Guid, int>();

			expectedStatuses.Add(new Guid("BF50C741-A780-4930-A64B-A5E00105F325"), 2);
			expectedStatuses.Add(new Guid("6069902E-5760-4DF4-B733-A5E00105B099"), 2);
			expectedStatuses.Add(new Guid("12728761-0ED6-422B-B2B5-A5E001051F1E"), 2);
			expectedStatuses.Add(new Guid("E4C7A7A7-8D2C-4591-ACA4-A53D00F82C88"), 2);
			expectedStatuses.Add(new Guid("1EBFEE75-CE35-40FB-975E-A3BF00D0577C"), 4);
			expectedStatuses.Add(new Guid("AE476FA3-7A6C-4948-89C2-A3BF00D0577C"), 2);
			expectedStatuses.Add(new Guid("F25262A1-E3C3-4A54-B202-A33200B2E94F"), 4); //
			expectedStatuses.Add(new Guid("42394840-F905-4B22-915F-A332008A5288"), 4); //
			expectedStatuses.Add(new Guid("A35F2179-9A4B-40C9-BF7B-A27400997C79"), 4); //
			expectedStatuses.Add(new Guid("CADD42C6-5419-48DD-8514-A25B009AD59D"), 4);
			expectedStatuses.Add(new Guid("C922055A-B4D0-4C06-B9AD-A1410113C47D"), 4); //
			expectedStatuses.Add(new Guid("4886AEDD-E30F-416C-B5E8-A1410113C47D"), 4);
			expectedStatuses.Add(new Guid("DCF2EA04-3031-4436-A229-A1410113C47D"), 4);
			expectedStatuses.Add(new Guid("47721DE4-A0CB-45EE-A123-A1410113C47D"), 4); //
			expectedStatuses.Add(new Guid("391DB822-3936-4C4D-9634-A1410113C47D"), 4); //
			expectedStatuses.Add(new Guid("87E6CEF0-388A-4365-94FA-A1410113C47D"), 4);
			expectedStatuses.Add(new Guid("55E3A133-6305-4C9A-8AEA-A1410113C47D"), 2);


			WithUnitOfWork.Do(() =>
			{
				foreach (var req in personReqs)
				{
					var request = PersonRequestRepository.Get(req.Id.Value);

					var requestStatus = 0;

					if (request.IsApproved)
						requestStatus = 2;

					else if (request.IsDenied)
						requestStatus = 4;

					resultStatuses.Add(request.Person.Id.Value, requestStatus);
				}
			});

			CollectionAssert.AreEquivalent(expectedStatuses, resultStatuses);
		}

		[Test]
		public void ShouldDenyBecauseOfPersonAccountIsFull()
		{
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			//Consumer Online
			var personRequests = new List<IPersonRequest>();
			WithUnitOfWork.Do(() =>
			{
				//load  Halvdag 16h/år
				var absence = AbsenceRepository.Get(new Guid("5B859CEF-0F35-4BA8-A82E-A14600EEE42E"));

				var wfcs = WorkflowControlSetRepository.Get(new Guid("E97BC114-8939-4A70-AE37-A338010FFF19"));
				foreach (var period in wfcs.AbsenceRequestOpenPeriods)
				{
					if (period.Absence.Equals(absence))
					{
						period.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2016, 3, 1), new DateOnly(2099, 5, 30));
						period.StaffingThresholdValidator = new AbsenceRequestNoneValidator();
						period.PersonAccountValidator = new PersonAccountBalanceValidator();
						period.AbsenceRequestProcess = new GrantAbsenceRequest();
						var datePeriod = period as AbsenceRequestOpenDatePeriod;
						if (datePeriod != null)
							datePeriod.Period = period.OpenForRequestsPeriod;
					}

				}
				// person  Didrik Wikberg, Lul14
				var person = PersonRepository.Load(new Guid("8080B4A4-785D-44FD-B7F9-A141010651CB"));

				var req4th = createAbsenceRequest(person, absence,
					new DateTimePeriod(new DateTime(2016, 4, 4, 8, 0, 0, DateTimeKind.Utc),
						new DateTime(2016, 4, 4, 12, 0, 0, DateTimeKind.Utc)));
				PersonRequestRepository.Add(req4th);
				personRequests.Add(req4th);
				var req5th = createAbsenceRequest(person, absence,
					new DateTimePeriod(new DateTime(2016, 4, 5, 11, 0, 0, DateTimeKind.Utc),
						new DateTime(2016, 4, 5, 13, 0, 0, DateTimeKind.Utc)));
				PersonRequestRepository.Add(req5th);
				personRequests.Add(req5th);
				var req6th = createAbsenceRequest(person, absence,
					new DateTimePeriod(new DateTime(2016, 4, 6, 13, 0, 0, DateTimeKind.Utc),
						new DateTime(2016, 4, 6, 17, 0, 0, DateTimeKind.Utc)));
				PersonRequestRepository.Add(req6th);
				personRequests.Add(req6th);

#pragma warning disable 618
				PersonRequestRepository.UnitOfWork.PersistAll();
#pragma warning restore 618
				var absenceRequestIds = new List<Guid> {req4th.Id.Value, req5th.Id.Value, req6th.Id.Value};

				var newMultiAbsenceRequestsCreatedEvent = new NewMultiAbsenceRequestsCreatedEvent()
				{
					PersonRequestIds = absenceRequestIds,
					InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
					LogOnBusinessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"),
					LogOnDatasource = "Teleopti WFM",
					Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z")
				};

				Target.Process(newMultiAbsenceRequestsCreatedEvent);
			});

			var cntApproved = 0;
			var cntDenied = 0;
			WithUnitOfWork.Do(() =>
			{
				foreach (var req in personRequests)
				{
					var request = PersonRequestRepository.Get(req.Id.Value);

					if (request.IsApproved)
						cntApproved ++;

					else if (request.IsDenied)
						cntDenied ++;
					
				}
			});

			cntDenied.Should().Be.EqualTo(1);
			cntApproved.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldDenyBecuaseOfLowAllowanceInBudgetHeadCount()
		{
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			//Consumer Online
			var personRequests = new List<IPersonRequest>();
			WithUnitOfWork.Do(() =>
			{
				//load  Halvdag 16h/år
				var absence = AbsenceRepository.Get(new Guid("5B859CEF-0F35-4BA8-A82E-A14600EEE42E"));

				var wfcs = WorkflowControlSetRepository.Get(new Guid("E97BC114-8939-4A70-AE37-A338010FFF19"));
				foreach (var period in wfcs.AbsenceRequestOpenPeriods)
				{
					if (period.Absence.Equals(absence))
					{
						period.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2016, 3, 1), new DateOnly(2099, 5, 30));
						period.StaffingThresholdValidator = new BudgetGroupHeadCountValidator();
						period.PersonAccountValidator = new AbsenceRequestNoneValidator();
						period.AbsenceRequestProcess = new GrantAbsenceRequest();
						var datePeriod = period as AbsenceRequestOpenDatePeriod;
						if (datePeriod != null)
							datePeriod.Period = period.OpenForRequestsPeriod;
					}
				}

				var scenario = ScenarioRepository.Load(new Guid("10E3B023-5C3B-4219-AF34-A11C00F0F283"));
				var budgetGroup = BudgetGroupRepository.Get(new Guid("81BAF583-4875-43EC-8E1D-A53A00DF0B3D"));
				var budgetDays = BudgetDayRepository.Find(scenario, budgetGroup,
					new DateOnlyPeriod(new DateOnly(2016, 4, 15), new DateOnly(2016, 4, 15)));
				budgetDays.ForEach(budgetDay =>
				{
					budgetDay.Allowance = 36;
					budgetDay.TotalAllowance = 36;
					budgetDay.AbsenceOverride = 36;
				});
#pragma warning disable 618
				BudgetDayRepository.UnitOfWork.PersistAll();
#pragma warning restore 618
				// person Englund, Rasmus 
				var person1 = PersonRepository.Load(new Guid("90FA4DCD-65CA-4599-B1EB-A276008EC775"));
				//Persson, Josefin
				var person2 = PersonRepository.Load(new Guid("F2588126-373C-47CC-BD78-A3E000AACF79"));

				var req1 = createAbsenceRequest(person1, absence,
					new DateTimePeriod(new DateTime(2016, 4, 15, 11, 0, 0, DateTimeKind.Utc),
						new DateTime(2016, 4, 15, 13, 0, 0, DateTimeKind.Utc)));
				PersonRequestRepository.Add(req1);
				personRequests.Add(req1);
				var req2 = createAbsenceRequest(person2, absence,
					new DateTimePeriod(new DateTime(2016, 4, 15, 14, 0, 0, DateTimeKind.Utc),
						new DateTime(2016, 4, 15, 16, 0, 0, DateTimeKind.Utc)));
				PersonRequestRepository.Add(req2);
				personRequests.Add(req2);

#pragma warning disable 618
				PersonRequestRepository.UnitOfWork.PersistAll();
#pragma warning restore 618
				var absenceRequestIds = new List<Guid> { req1.Id.Value, req2.Id.Value};

				var newMultiAbsenceRequestsCreatedEvent = new NewMultiAbsenceRequestsCreatedEvent()
				{
					PersonRequestIds = absenceRequestIds,
					InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
					LogOnBusinessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"),
					LogOnDatasource = "Teleopti WFM",
					Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z")
				};

				Target.Process(newMultiAbsenceRequestsCreatedEvent);
			});

			var cntApproved = 0;
			var cntDenied = 0;
			WithUnitOfWork.Do(() =>
			{
				foreach (var req in personRequests)
				{
					var request = PersonRequestRepository.Get(req.Id.Value);

					if (request.IsApproved)
						cntApproved++;

					else if (request.IsDenied)
						cntDenied++;

				}
			});

			cntDenied.Should().Be.EqualTo(1);
			cntApproved.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldDenyBecauseOfBudgetIsUsed()
		{
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
			//Consumer Online
			var personRequests = new List<IPersonRequest>();
			WithUnitOfWork.Do(() =>
			{
				//load  Halvdag 16h/år
				var absence = AbsenceRepository.Get(new Guid("5B859CEF-0F35-4BA8-A82E-A14600EEE42E"));

				var wfcs = WorkflowControlSetRepository.Get(new Guid("E97BC114-8939-4A70-AE37-A338010FFF19"));
				foreach (var period in wfcs.AbsenceRequestOpenPeriods)
				{
					if (period.Absence.Equals(absence))
					{
						period.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2016, 3, 1), new DateOnly(2099, 5, 30));
						period.StaffingThresholdValidator = new BudgetGroupAllowanceValidator();
						period.PersonAccountValidator = new AbsenceRequestNoneValidator();
						period.AbsenceRequestProcess = new GrantAbsenceRequest();
						var datePeriod = period as AbsenceRequestOpenDatePeriod;
						if (datePeriod != null)
							datePeriod.Period = period.OpenForRequestsPeriod;
					}

				}

				var bu = BusinessUnitRepository.Load(new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));
				var scenario = ScenarioRepository.LoadDefaultScenario(bu);
				var bGroup = BudgetGroupRepository.Get(new Guid("81BAF583-4875-43EC-8E1D-A53A00DF0B3D"));
				var bDay = BudgetDayRepository.Find(scenario, bGroup,
					new DateOnlyPeriod(new DateOnly(2016, 4, 11), new DateOnly(2016, 4, 11)));

				bDay.First().Allowance = 1;

				var person = PersonRepository.Load(new Guid("6E75AF18-F494-42AE-8272-A141010651CB"));
				var person2 = PersonRepository.Load(new Guid("8080B4A4-785D-44FD-B7F9-A141010651CB"));

				var req4Th = createAbsenceRequest(person, absence,
					new DateTimePeriod(new DateTime(2016, 4, 11, 6, 0, 0, DateTimeKind.Utc),
						new DateTime(2016, 4, 11, 18, 0, 0, DateTimeKind.Utc)));
				PersonRequestRepository.Add(req4Th);
				personRequests.Add(req4Th);
				var req4Th2 = createAbsenceRequest(person2, absence,
					new DateTimePeriod(new DateTime(2016, 4, 11, 6, 0, 0, DateTimeKind.Utc),
						new DateTime(2016, 4, 11, 18, 0, 0, DateTimeKind.Utc)));
				PersonRequestRepository.Add(req4Th2);
				personRequests.Add(req4Th2);

#pragma warning disable 618
				PersonRequestRepository.UnitOfWork.PersistAll();
#pragma warning restore 618
				var absenceRequestIds = new List<Guid> { req4Th.Id.GetValueOrDefault(), req4Th2.Id.GetValueOrDefault()};

				var newMultiAbsenceRequestsCreatedEvent = new NewMultiAbsenceRequestsCreatedEvent()
				{
					PersonRequestIds = absenceRequestIds,
					InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
					LogOnBusinessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"),
					LogOnDatasource = "Teleopti WFM",
					Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z")
				};

				Target.Process(newMultiAbsenceRequestsCreatedEvent);
			});

			var cntApproved = 0;
			var cntDenied = 0;
			WithUnitOfWork.Do(() =>
			{
				foreach (var req in personRequests)
				{
					var request = PersonRequestRepository.Get(req.Id.GetValueOrDefault());

					if (request.IsApproved)
						cntApproved++;

					else if (request.IsDenied)
					{
						cntDenied++;
						Assert.That(request.DenyReason.Contains("Otillräcklig bemanning"));
					}

				}
			});

			cntDenied.Should().Be.EqualTo(1);
			cntApproved.Should().Be.EqualTo(1);
		}
		private IPersonRequest createAbsenceRequest(IPerson person, IAbsence absence)
		{
			var req = new AbsenceRequest(absence,
										 new DateTimePeriod(new DateTime(2016, 3, 10, 8, 0, 0, DateTimeKind.Utc),
															new DateTime(2016, 3, 10, 18, 0, 0, DateTimeKind.Utc)));
			return new PersonRequest(person) { Request = req };
		}

		private IPersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod dateTimePeriod)
		{
			var req = new AbsenceRequest(absence,dateTimePeriod);
			return new PersonRequest(person) { Request = req };
		}
	}

	public class RequestPerformanceTestAttribute : IoCTestAttribute
	{
		//protected override FakeConfigReader Config()
		//{
		//	var config = base.Config();
		//	config.FakeConnectionString("Tenancy", InfraTestConfigReader.ConnectionString);
		//	config.FakeConnectionString("Hangfire", InfraTestConfigReader.AnalyticsConnectionString);
		//	return config;
		//}

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);
			system.AddModule(new CommonModule(configuration));
			system.UseTestDouble<MultiAbsenceRequestsUpdater>().For<MultiAbsenceRequestsUpdater>();
			system.UseTestDouble<MultiAbsenceRequestProcessor>().For<IMultiAbsenceRequestProcessor>();
			system.UseTestDouble<MultiAbsenceRequestsHandler>().For<MultiAbsenceRequestsHandler>();
			system.UseTestDouble<ProcessMultipleAbsenceRequests>().For<IProcessMultipleAbsenceRequest>();
			system.UseTestDouble<ApproveRequestCommandHandler>().For<IHandleCommand<ApproveRequestCommand>>();
			system.UseTestDouble<DenyRequestCommandHandler>().For<IHandleCommand<DenyRequestCommand>>();
			system.UseTestDouble<RequestApprovalServiceFactory>().For<IRequestApprovalServiceFactory>();
			system.UseTestDouble<NoMessageSender>().For<IMessageSender>();
			system.UseTestDouble<StardustJobFeedback>().For<IStardustJobFeedback>();
			system.AddService<Database>();
			system.AddModule(new TenantServerModule(configuration));

		}

		protected override void Startup(IComponentContext container)
		{
			base.Startup(container);
			container.Resolve<HangfireClientStarter>().Start();
		}
	}
}