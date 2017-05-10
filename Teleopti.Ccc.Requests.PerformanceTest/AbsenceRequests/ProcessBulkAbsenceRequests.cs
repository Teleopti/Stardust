using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Requests.PerformanceTest.AbsenceRequests
{
	[RequestPerformanceTest]
	public class ProcessBulkAbsenceRequests
	{
		private const int statusPending = 0;
		private const int statusDenied = 1;
		private const int statusApproved = 2;
		private const int statusNew = 3;
		private const int statusAutoDenied = 4;
		private const int statusWaitlistedAndDenied = 5;
		private const int statusCancelled = 6;
		private const string tenantName = "Teleopti WFM";

		// BusinesUnit "Telia Sverige"
		private readonly Guid businessUnitId = new Guid("1FA1F97C-EBFF-4379-B5F9-A11C00F0F02B");

		// Workflow controlset "Consumer Support"
		private readonly Guid workflowControlsetId = new Guid("E97BC114-8939-4A70-AE37-A338010FFF19");

		// Absence "Halvdag 16h/år"
		private readonly Guid absenceId = new Guid("5B859CEF-0F35-4BA8-A82E-A14600EEE42E");

		// Budget group "Support BB Lan"
		private readonly Guid budgetGroupId = new Guid("81BAF583-4875-43EC-8E1D-A53A00DF0B3D");

		public IAbsenceRepository AbsenceRepository;
		public AsSystem AsSystem;
		public IDataSourceScope DataSource;
		public IPersonRepository PersonRepository;
		public IPersonRequestRepository PersonRequestRepository;
		public MultiAbsenceRequestsHandler Target;
		public WithUnitOfWork WithUnitOfWork;
		public IWorkflowControlSetRepository WorkflowControlSetRepository;
		public IBudgetGroupRepository BudgetGroupRepository;
		public IBudgetDayRepository BudgetDayRepository;
		public IScenarioRepository ScenarioRepository;
		public IBusinessUnitRepository BusinessUnitRepository;
		public IQueuedAbsenceRequestRepository QueuedAbsenceRequestRepository;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public IAbsenceRequestProcessor AbsenceRequestProcessor;
		public IAbsenceRequestValidatorProvider AbsenceRequestValidatorProvider;

		[SetUp]
		public void Setup()
		{
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
		}

		[Test]
		public void Aa()
		{
			//This is a very ugly way to remove buildSetupStuff from the real tests
		}

		[Test]
		public void ShouldProcessMultipleAbsenceRequests()
		{
			var expectedStatuses = new Dictionary<Guid, int>
			{
				// people from team 'FL_Sup_Sun7_71631' and 'FL_Sup_Sun6_00333'
				//DO NOT CHANGE THE ORDER OF THE GUIDS!
				[new Guid("55E3A133-6305-4C9A-8AEA-A1410113C47D")] = statusApproved,
				[new Guid("87E6CEF0-388A-4365-94FA-A1410113C47D")] = statusAutoDenied,
				[new Guid("391DB822-3936-4C4D-9634-A1410113C47D")] = statusAutoDenied,
				[new Guid("47721DE4-A0CB-45EE-A123-A1410113C47D")] = statusAutoDenied,
				[new Guid("DCF2EA04-3031-4436-A229-A1410113C47D")] = statusAutoDenied,
				[new Guid("4886AEDD-E30F-416C-B5E8-A1410113C47D")] = statusAutoDenied,
				[new Guid("C922055A-B4D0-4C06-B9AD-A1410113C47D")] = statusAutoDenied,
				[new Guid("CADD42C6-5419-48DD-8514-A25B009AD59D")] = statusAutoDenied,
				[new Guid("A35F2179-9A4B-40C9-BF7B-A27400997C79")] = statusAutoDenied,
				[new Guid("42394840-F905-4B22-915F-A332008A5288")] = statusAutoDenied,
				[new Guid("F25262A1-E3C3-4A54-B202-A33200B2E94F")] = statusAutoDenied,
				[new Guid("AE476FA3-7A6C-4948-89C2-A3BF00D0577C")] = statusApproved,
				[new Guid("1EBFEE75-CE35-40FB-975E-A3BF00D0577C")] = statusAutoDenied,
				[new Guid("E4C7A7A7-8D2C-4591-ACA4-A53D00F82C88")] = statusApproved,
				[new Guid("12728761-0ED6-422B-B2B5-A5E001051F1E")] = statusApproved,
				[new Guid("6069902E-5760-4DF4-B733-A5E00105B099")] = statusApproved,
				[new Guid("BF50C741-A780-4930-A64B-A5E00105F325")] = statusApproved
			};

			var personReqs = processMultipleAbsenceRequests(expectedStatuses.Keys, false);

			requestStatusesAssert(personReqs, expectedStatuses);
		}

		[Test]
		public void ShouldProcessMultipleAbsenceRequestsWithIntradayValidator()
		{
			var expectedStatuses = new Dictionary<Guid, int>
			{
				// people from team 'FL_SaveDesk_Far4_00282' and 'OM_D_SME_MBS_45428'
				[new Guid("889F7A33-BE61-4FA7-BFAD-A14100FFA30F")] = statusPending,
				[new Guid("7843C186-3DB9-4D91-9F34-A14100FFA313")] = statusAutoDenied,
				[new Guid("51552353-F770-4340-88FA-A1CE00B56199")] = statusApproved,
				[new Guid("C6232E9E-9234-4983-8055-A1AF0090A8D7")] = statusAutoDenied,
				[new Guid("DD0FE7BF-B56A-4309-BED2-A51A010E01BB")] = statusAutoDenied,
				[new Guid("189295E6-6AFF-41C5-87CF-A49C01026BD0")] = statusPending,
				[new Guid("1CFD93FB-7ABF-4839-BBFC-A4C3009216A2")] = statusAutoDenied,
				[new Guid("C89644EF-371F-45BB-9F01-A47500C13EB4")] = statusPending,
				[new Guid("3606EB98-53E3-4323-BC47-A3A1009DE023")] = statusApproved,
				[new Guid("35E1A58D-97CB-4117-B9C3-A141010AA74C")] = statusPending,
				[new Guid("B94F331B-56C3-4667-85BB-A14100FFA31D")] = statusPending
			};

			var personReqs = processMultipleAbsenceRequests(expectedStatuses.Keys, true, RequestValidatorsFlag.IntradayValidator);

			requestStatusesAssert(personReqs, expectedStatuses);
		}

		[Test]
		public void ShouldDenyBecauseOfPersonAccountIsFull()
		{
			logonSystem();

			var personRequests = new List<IPersonRequest>();
			WithUnitOfWork.Do(() =>
			{
				var absence = AbsenceRepository.Get(absenceId);

				// person  Vinblad, Christian has a person account that on that absence
				var person = PersonRepository.Load(new Guid("6E75AF18-F494-42AE-8272-A141010651CB"));

				var req4 = createAbsenceRequest(person, absence,
					new DateTimePeriod(new DateTime(2016, 4, 4, 8, 0, 0, DateTimeKind.Utc),
						new DateTime(2016, 4, 4, 12, 0, 0, DateTimeKind.Utc)));
				PersonRequestRepository.Add(req4);
				personRequests.Add(req4);

				var req5 = createAbsenceRequest(person, absence,
					new DateTimePeriod(new DateTime(2016, 4, 5, 8, 0, 0, DateTimeKind.Utc),
						new DateTime(2016, 4, 5, 12, 0, 0, DateTimeKind.Utc)));
				PersonRequestRepository.Add(req5);
				personRequests.Add(req5);

				var req6 = createAbsenceRequest(person, absence,
					new DateTimePeriod(new DateTime(2016, 4, 6, 8, 0, 0, DateTimeKind.Utc),
						new DateTime(2016, 4, 6, 12, 0, 0, DateTimeKind.Utc)));
				PersonRequestRepository.Add(req6);
				personRequests.Add(req6);

				handleShouldDeniedRequests(absence, personRequests, new AbsenceRequestNoneValidator(),
					new PersonAccountBalanceValidator(), new GrantAbsenceRequest(), 1, 2);
			});
		}

		[Test]
		public void ShouldDenyBecuaseOfLowAllowanceInBudgetHeadCount()
		{
			logonSystem();

			var personRequests = new List<IPersonRequest>();
			WithUnitOfWork.Do(() =>
			{
				var absence = AbsenceRepository.Get(absenceId);

				var scenario = ScenarioRepository.Load(new Guid("10E3B023-5C3B-4219-AF34-A11C00F0F283"));
				var budgetGroup = BudgetGroupRepository.Get(budgetGroupId);
				var budgetDays = BudgetDayRepository.Find(scenario, budgetGroup,
					new DateOnlyPeriod(new DateOnly(2016, 4, 15), new DateOnly(2016, 4, 15)));
				budgetDays.ForEach(budgetDay =>
				{
					budgetDay.ShrinkedAllowance = 1;
					budgetDay.FullAllowance = 1;
					budgetDay.AbsenceOverride = 1;
				});

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

				handleShouldDeniedRequests(absence, personRequests, new BudgetGroupHeadCountValidator(),
					new AbsenceRequestNoneValidator(), new GrantAbsenceRequest(), 1, 1);
			});
		}

		[Test]
		public void ShouldDenyBecauseOfBudgetIsUsed()
		{
			logonSystem();

			var personRequests = new List<IPersonRequest>();
			WithUnitOfWork.Do(() =>
			{
				var absence = AbsenceRepository.Get(absenceId);

				var bu = BusinessUnitRepository.Load(businessUnitId);
				var scenario = ScenarioRepository.LoadDefaultScenario(bu);
				var bGroup = BudgetGroupRepository.Get(budgetGroupId);
				var bDay = BudgetDayRepository.Find(scenario, bGroup,
					new DateOnlyPeriod(new DateOnly(2016, 4, 11), new DateOnly(2016, 4, 11)));

				bDay.First().ShrinkedAllowance = 1;

				var person = PersonRepository.Load(new Guid("BD2400CC-0FFE-4E30-8D4F-A141010651CB"));
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

				handleShouldDeniedRequests(absence, personRequests, new BudgetGroupAllowanceValidator(),
					new AbsenceRequestNoneValidator(), new GrantAbsenceRequest(), 1, 1, "Otillräcklig bemanning");
			});
		}

		private void requestStatusesAssert(IReadOnlyCollection<IPersonRequest> personRequests,
			IDictionary<Guid, int> expectedStatuses)
		{
			var resultStatuses = new Dictionary<Guid, int>();
			fillResultStatuses(personRequests, resultStatuses);
			foreach (var resultStatuse in resultStatuses)
			{
				Console.WriteLine($"key:{resultStatuse.Key},value:{resultStatuse.Value}");
			}
			CollectionAssert.AreEquivalent(expectedStatuses, resultStatuses);
		}

		private void fillResultStatuses(IReadOnlyCollection<IPersonRequest> personReqs, IDictionary<Guid, int> resultStatuses)
		{
			WithUnitOfWork.Do(() =>
			{
				foreach (var req in personReqs)
				{
					var request = PersonRequestRepository.Get(req.Id.GetValueOrDefault());
					var requestStatus = getRequestStatus(request);
					resultStatuses.Add(request.Person.Id.GetValueOrDefault(), requestStatus);
				}
			});
		}

		private void logonSystem()
		{
			using (DataSource.OnThisThreadUse(tenantName))
			{
				AsSystem.Logon(tenantName, businessUnitId);
			}
		}

		private List<IPersonRequest> processMultipleAbsenceRequests(IEnumerable<Guid> personIds, bool queueRequest,
			RequestValidatorsFlag requestValidatorsFlag = RequestValidatorsFlag.None)
		{
			logonSystem();

			var personReqs = new List<IPersonRequest>();
			var absenceRequestIds = new List<Guid>();

			WithUnitOfWork.Do(() =>
			{
				prepareAbsenceRequests(personIds, personReqs, absenceRequestIds);

				var newMultiAbsenceRequestsCreatedEvent = new NewMultiAbsenceRequestsCreatedEvent
				{
					PersonRequestIds = absenceRequestIds,
					InitiatorId = Guid.Empty,
					LogOnBusinessUnitId = businessUnitId,
					LogOnDatasource = tenantName,
					Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z"),
					Sent = DateTime.UtcNow
				};

				if (queueRequest)
				{
					queueRequests(absenceRequestIds, requestValidatorsFlag);
				}

				Target.Handle(newMultiAbsenceRequestsCreatedEvent);
			});
			return personReqs;
		}

		private void prepareAbsenceRequests(IEnumerable<Guid> personIds, ICollection<IPersonRequest> personReqs,
			ICollection<Guid> absenceRequestIds)
		{
			//Consumer Support
			var wfcs = WorkflowControlSetRepository.Get(workflowControlsetId);
			setAbsenceRequestOpenPeriods(wfcs);

			// load some persons
			var persons = PersonRepository.FindPeople(personIds);

			//load vacation
			var absence = AbsenceRepository.Get(new Guid("3A5F20AE-7C18-4CA5-A02B-A11C00F0F27F"));

			foreach (var person in persons)
			{
				personReqs.Add(createAbsenceRequest(person, absence));
				setAbsenceRequestOpenPeriods(person.WorkflowControlSet);
			}

			CurrentUnitOfWork.Current().PersistAll();

			foreach (var pReq in personReqs)
			{
				PersonRequestRepository.Add(pReq);
				CurrentUnitOfWork.Current().PersistAll();

				absenceRequestIds.Add(pReq.Id.GetValueOrDefault());
			}
		}

		private static void setAbsenceRequestOpenPeriods(IWorkflowControlSet wfcs)
		{
			foreach (var period in wfcs.AbsenceRequestOpenPeriods)
			{
				period.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2016, 3, 1), new DateOnly(2099, 5, 30));
				period.StaffingThresholdValidator = new StaffingThresholdValidator();
				period.AbsenceRequestProcess = new GrantAbsenceRequest();
				var datePeriod = period as AbsenceRequestOpenDatePeriod;
				if (datePeriod != null) datePeriod.Period = period.OpenForRequestsPeriod;
			}
		}

		//Don't remove this, nice to have for manual debugging
		private void queueRequests(IEnumerable<Guid> absenceRequestIds, RequestValidatorsFlag requestValidatorsFlag)
		{
			foreach (var requestId in absenceRequestIds)
			{
				var queuedAbsenceRequest = new QueuedAbsenceRequest
				{
					PersonRequest = requestId,
					Created = new DateTime(2016, 3, 10, 8, 0, 0, DateTimeKind.Utc),
					StartDateTime = new DateTime(2016, 3, 10, 8, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2016, 3, 10, 18, 0, 0, DateTimeKind.Utc),
					MandatoryValidators = requestValidatorsFlag
				};
				QueuedAbsenceRequestRepository.Add(queuedAbsenceRequest);
			}
			CurrentUnitOfWork.Current().PersistAll();
		}

		private static int getRequestStatus(IPersonRequest request)
		{
			var requestStatus = 10;

			if (request.IsApproved)
				requestStatus = statusApproved;
			else if (request.IsPending)
				requestStatus = statusPending;
			else if (request.IsNew)
				requestStatus = statusNew;
			else if (request.IsCancelled)
				requestStatus = statusCancelled;
			else if (request.IsWaitlisted && request.IsDenied)
				requestStatus = statusWaitlistedAndDenied;
			else if (request.IsAutoDenied)
				requestStatus = statusAutoDenied;
			else if (request.IsDenied)
				requestStatus = statusDenied;

			return requestStatus;
		}

		private void checkDeniedAndApprovedRequestCount(IEnumerable<IPersonRequest> personRequests, int expectedDeniedCount,
			int expectedApprovedCount, string expectedDenyReasonContent = null)
		{
			var countApproved = 0;
			var countDenied = 0;
			WithUnitOfWork.Do(() =>
			{
				foreach (var req in personRequests)
				{
					var request = PersonRequestRepository.Get(req.Id.GetValueOrDefault());

					if (request.IsApproved)
					{
						countApproved++;
					}
					else if (request.IsDenied)
					{
						countDenied++;
						if (expectedDenyReasonContent != null)
						{
							Assert.That(request.DenyReason.Contains(expectedDenyReasonContent));
						}
					}
				}
			});

			countDenied.Should().Be.EqualTo(expectedDeniedCount);
			countApproved.Should().Be.EqualTo(expectedApprovedCount);
		}

		private static IPersonRequest createAbsenceRequest(IPerson person, IAbsence absence)
		{
			var startTime = new DateTime(2016, 3, 10, 8, 0, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2016, 3, 10, 18, 0, 0, DateTimeKind.Utc);
			return createAbsenceRequest(person, absence, new DateTimePeriod(startTime, endTime));
		}

		private static IPersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod dateTimePeriod)
		{
			var req = new AbsenceRequest(absence, dateTimePeriod);
			var personReq = new PersonRequest(person) { Request = req };
			personReq.Pending();
			return personReq;
		}

		private void handleShouldDeniedRequests(IAbsence absence, IList<IPersonRequest> personRequests,
			IAbsenceRequestValidator staffingThresholdValidator, IAbsenceRequestValidator personAccountValidator,
			IProcessAbsenceRequest absenceRequestProcess, int expectedDeniedCount, int expectedApprovedCount,
			string expectedDenyReason = null)
		{
			var wfcs = WorkflowControlSetRepository.Get(workflowControlsetId);
			foreach (var period in wfcs.AbsenceRequestOpenPeriods)
			{
				if (!period.Absence.Equals(absence)) continue;

				period.OpenForRequestsPeriod = new DateOnlyPeriod(new DateOnly(2016, 3, 1), new DateOnly(2099, 5, 30));
				period.StaffingThresholdValidator = staffingThresholdValidator;
				period.PersonAccountValidator = personAccountValidator;
				period.AbsenceRequestProcess = absenceRequestProcess;
				var datePeriod = period as AbsenceRequestOpenDatePeriod;
				if (datePeriod != null) datePeriod.Period = period.OpenForRequestsPeriod;
			}

			CurrentUnitOfWork.Current().PersistAll();

			var newMultiAbsenceRequestsCreatedEvent = new NewMultiAbsenceRequestsCreatedEvent
			{
				PersonRequestIds = personRequests.Select(r => r.Id.GetValueOrDefault()).ToList(),
				InitiatorId = Guid.Empty,
				LogOnBusinessUnitId = businessUnitId,
				LogOnDatasource = tenantName,
				Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z"),
				Sent = DateTime.UtcNow
			};

			Target.Handle(newMultiAbsenceRequestsCreatedEvent);

			checkDeniedAndApprovedRequestCount(personRequests, expectedDeniedCount, expectedApprovedCount, expectedDenyReason);
		}
	}
}