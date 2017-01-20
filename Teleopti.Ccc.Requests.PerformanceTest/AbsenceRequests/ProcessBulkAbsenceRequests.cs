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
				[new Guid("55E3A133-6305-4C9A-8AEA-A1410113C47D")] = 2,
				[new Guid("87E6CEF0-388A-4365-94FA-A1410113C47D")] = 4,
				[new Guid("391DB822-3936-4C4D-9634-A1410113C47D")] = 4,
				[new Guid("47721DE4-A0CB-45EE-A123-A1410113C47D")] = 4,
				[new Guid("DCF2EA04-3031-4436-A229-A1410113C47D")] = 4,
				[new Guid("4886AEDD-E30F-416C-B5E8-A1410113C47D")] = 4,
				[new Guid("C922055A-B4D0-4C06-B9AD-A1410113C47D")] = 4,
				[new Guid("CADD42C6-5419-48DD-8514-A25B009AD59D")] = 4,
				[new Guid("A35F2179-9A4B-40C9-BF7B-A27400997C79")] = 4,
				[new Guid("42394840-F905-4B22-915F-A332008A5288")] = 4,
				[new Guid("F25262A1-E3C3-4A54-B202-A33200B2E94F")] = 4,
				[new Guid("AE476FA3-7A6C-4948-89C2-A3BF00D0577C")] = 2,
				[new Guid("1EBFEE75-CE35-40FB-975E-A3BF00D0577C")] = 4,
				[new Guid("E4C7A7A7-8D2C-4591-ACA4-A53D00F82C88")] = 2,
				[new Guid("12728761-0ED6-422B-B2B5-A5E001051F1E")] = 2,
				[new Guid("6069902E-5760-4DF4-B733-A5E00105B099")] = 2,
				[new Guid("BF50C741-A780-4930-A64B-A5E00105F325")] = 2
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
				[new Guid("889F7A33-BE61-4FA7-BFAD-A14100FFA30F")] = 2,
				[new Guid("7843C186-3DB9-4D91-9F34-A14100FFA313")] = 4,
				[new Guid("51552353-F770-4340-88FA-A1CE00B56199")] = 2,
				[new Guid("C6232E9E-9234-4983-8055-A1AF0090A8D7")] = 4,
				[new Guid("DD0FE7BF-B56A-4309-BED2-A51A010E01BB")] = 4,
				[new Guid("189295E6-6AFF-41C5-87CF-A49C01026BD0")] = 2,
				[new Guid("1CFD93FB-7ABF-4839-BBFC-A4C3009216A2")] = 4,
				[new Guid("C89644EF-371F-45BB-9F01-A47500C13EB4")] = 2,
				[new Guid("B21D5EA1-7CD9-4237-B451-A65900EB4857")] = 2,
				[new Guid("3606EB98-53E3-4323-BC47-A3A1009DE023")] = 2,
				[new Guid("35E1A58D-97CB-4117-B9C3-A141010AA74C")] = 2,
				[new Guid("B94F331B-56C3-4667-85BB-A14100FFA31D")] = 2,
				[new Guid("144F887C-5AFF-4CC1-93F4-A65900F1C833")] = 2,
				[new Guid("4AA6D8E5-B679-4F60-98F8-A65900F41B8B")] = 2,
				[new Guid("1B53C821-2837-4893-8ACC-A65900F41C93")] = 2,
				[new Guid("4FD5816A-CB41-418B-837E-A65900F0F1FA")] = 2,
				[new Guid("32E2CCEF-7D00-45DC-8E0B-A65900F2EC3F")] = 2
			};

			var personReqs = processMultipleAbsenceRequests(expectedStatuses.Keys, true, RequestValidatorsFlag.IntradayValidator);

			requestStatusesAssert(personReqs, expectedStatuses);
		}

		private void requestStatusesAssert(List<IPersonRequest> personRequests, Dictionary<Guid, int> expectedStatuses)
		{
			var resultStatuses = new Dictionary<Guid, int>();
			fillResultStatuses(personRequests, resultStatuses);
			foreach (var resultStatuse in resultStatuses)
			{
				Console.WriteLine($"key:{resultStatuse.Key},value:{resultStatuse.Value}");
			}
			CollectionAssert.AreEquivalent(expectedStatuses, resultStatuses);
		}

		private void fillResultStatuses(List<IPersonRequest> personReqs, Dictionary<Guid, int> resultStatuses)
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

		private List<IPersonRequest> processMultipleAbsenceRequests(IEnumerable<Guid> personIds, bool queueRequest, RequestValidatorsFlag requestValidatorsFlag = RequestValidatorsFlag.None)
		{
			using (DataSource.OnThisThreadUse("Teleopti WFM"))
				AsSystem.Logon("Teleopti WFM", new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"));

			var personReqs = new List<IPersonRequest>();
			var absenceRequestIds = new List<Guid>();

			WithUnitOfWork.Do(() =>
			{
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
				CurrentUnitOfWork.Current().PersistAll();

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
					CurrentUnitOfWork.Current().PersistAll();

					absenceRequestIds.Add(pReq.Id.GetValueOrDefault());
				}

				var newMultiAbsenceRequestsCreatedEvent = new NewMultiAbsenceRequestsCreatedEvent()
				{
					PersonRequestIds = absenceRequestIds,
					InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
					LogOnBusinessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"),
					LogOnDatasource = "Teleopti WFM",
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

		//Don't remove this, nice to have for manual debugging
		private void queueRequests(IEnumerable<Guid> absenceRequestIds,
			RequestValidatorsFlag requestValidatorsFlag)
		{
			foreach (var requestId in absenceRequestIds)
			{
				var queuedAbsenceRequest = new QueuedAbsenceRequest()
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
				// person  Vinblad, Christian has a person account that on that absence
				var person = PersonRepository.Load(new Guid("6E75AF18-F494-42AE-8272-A141010651CB"));

				var req4th = createAbsenceRequest(person, absence,
					new DateTimePeriod(new DateTime(2016, 4, 4, 8, 0, 0, DateTimeKind.Utc),
						new DateTime(2016, 4, 4, 12, 0, 0, DateTimeKind.Utc)));
				PersonRequestRepository.Add(req4th);
				personRequests.Add(req4th);
				var req5th = createAbsenceRequest(person, absence,
					new DateTimePeriod(new DateTime(2016, 4, 5, 8, 0, 0, DateTimeKind.Utc),
						new DateTime(2016, 4, 5, 12, 0, 0, DateTimeKind.Utc)));
				PersonRequestRepository.Add(req5th);
				personRequests.Add(req5th);
				var req6th = createAbsenceRequest(person, absence,
					new DateTimePeriod(new DateTime(2016, 4, 6, 8, 0, 0, DateTimeKind.Utc),
						new DateTime(2016, 4, 6, 12, 0, 0, DateTimeKind.Utc)));
				PersonRequestRepository.Add(req6th);
				personRequests.Add(req6th);

				CurrentUnitOfWork.Current().PersistAll();

				var absenceRequestIds = new List<Guid> { req4th.Id.GetValueOrDefault(), req5th.Id.GetValueOrDefault(), req6th.Id.GetValueOrDefault() };

				var newMultiAbsenceRequestsCreatedEvent = new NewMultiAbsenceRequestsCreatedEvent()
				{
					PersonRequestIds = absenceRequestIds,
					InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
					LogOnBusinessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"),
					LogOnDatasource = "Teleopti WFM",
					Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z"),
					Sent = DateTime.UtcNow
				};

				Target.Handle(newMultiAbsenceRequestsCreatedEvent);
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
						cntDenied++;
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

				CurrentUnitOfWork.Current().PersistAll();

				var absenceRequestIds = new List<Guid> { req1.Id.GetValueOrDefault(), req2.Id.GetValueOrDefault() };

				var newMultiAbsenceRequestsCreatedEvent = new NewMultiAbsenceRequestsCreatedEvent()
				{
					PersonRequestIds = absenceRequestIds,
					InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
					LogOnBusinessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"),
					LogOnDatasource = "Teleopti WFM",
					Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z"),
					Sent = DateTime.UtcNow
				};

				Target.Handle(newMultiAbsenceRequestsCreatedEvent);
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

				CurrentUnitOfWork.Current().PersistAll();

				var absenceRequestIds = new List<Guid> { req4Th.Id.GetValueOrDefault(), req4Th2.Id.GetValueOrDefault() };

				var newMultiAbsenceRequestsCreatedEvent = new NewMultiAbsenceRequestsCreatedEvent()
				{
					PersonRequestIds = absenceRequestIds,
					InitiatorId = new Guid("00000000-0000-0000-0000-000000000000"),
					LogOnBusinessUnitId = new Guid("1fa1f97c-ebff-4379-b5f9-a11c00f0f02b"),
					LogOnDatasource = "Teleopti WFM",
					Timestamp = DateTime.Parse("2016-08-08T11:06:00.7366909Z"),
					Sent = DateTime.UtcNow
				};

				Target.Handle(newMultiAbsenceRequestsCreatedEvent);
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
			var req = createAbsenceRequest(person, absence, new DateTimePeriod(new DateTime(2016, 3, 10, 8, 0, 0, DateTimeKind.Utc),
																			   new DateTime(2016, 3, 10, 18, 0, 0, DateTimeKind.Utc)));

			return req;
		}

		private IPersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod dateTimePeriod)
		{
			var req = new AbsenceRequest(absence, dateTimePeriod);
			var personReq = new PersonRequest(person) { Request = req };
			personReq.Pending();
			return personReq;
		}
	}
}