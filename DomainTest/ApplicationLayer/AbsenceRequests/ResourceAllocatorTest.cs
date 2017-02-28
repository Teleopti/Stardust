﻿using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	[DomainTest]
	[TestFixture]
	public class ResourceAllocatorTest : ISetup
	{
		public ResourceAllocator Target;
		public FakeConfigReader ConfigReader;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakeScheduleForecastSkillReadModelRepository ScheduleForecastSkillReadModelRepository;
		public FakeLoggedOnUser LoggedOnUser;
		private ISkill _primarySkill;
		private ISkill _secondarySkill;
		private ISkill _unsortedSkill15;
		private ISkill _unsortedSkill30;

		private const double forecastedPrimary = 2;
		private const double staffingPrimary = 3;
		private const double overstaffedPrimary = staffingPrimary - forecastedPrimary;

		private const double forecastedSecondary = 2; //secondary skill, should not be used
		private const double staffingSecondary = 5;

		private const double forecastedUnsorted15 = 2;
		private const double staffingUnsorted15 = 10;
		private const double overstaffedUnsorted15 = staffingUnsorted15 - forecastedUnsorted15;

		private const double forecastedUnsorted30 = 2;
		private const double staffingUnsorted30 = 10;
		private const double overstaffedUnsorted30 = staffingUnsorted30 - forecastedUnsorted30;

		private const double totaloverstaffed1 = overstaffedPrimary + overstaffedUnsorted15 + overstaffedUnsorted30;
		private const double totaloverstaffed2 = overstaffedPrimary + overstaffedUnsorted15 + overstaffedUnsorted30;
		private const double totaloverstaffed3 = overstaffedUnsorted15;


		private DateTime _now;
		private DateTime _requestStart;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			_now = new DateTime(2016, 03, 14, 12, 00, 00, DateTimeKind.Utc);
			_requestStart = _now.AddHours(2);
		}


		[Test]
		public void ShouldMergeChangesWithReadModelValuesWhenAllocating()
		{
			var requestLength = new TimeSpan(0, 15, 0);
			var request = createNewRequest(requestLength);
			populateReadModel();

			var startDateTime1 = request.Request.Period.StartDateTime;
			var endDateTime1 = startDateTime1.AddMinutes(15);

			var startDateTime2 = endDateTime1;
			var endDateTime2 = startDateTime2.AddMinutes(15);

			ScheduleForecastSkillReadModelRepository.PersistChange(
				new StaffingIntervalChange()
				{
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					StaffingLevel = -1
				});

			var changes = Target.AllocateResource(request, _now);

			var expectedChanges = new List<StaffingIntervalChange>()
			{
			   new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					SkillId = _primarySkill.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedPrimary/(totaloverstaffed1-1)
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StaffingLevel = -(overstaffedUnsorted15 - 1)/(totaloverstaffed1-1)
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime2,
					SkillId = _unsortedSkill30.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedUnsorted30/((totaloverstaffed1-1)*2) 
				}
			};

		  //  var sum = expectedChanges.Sum(x => x.StaffingLevel) + expectedChanges.Last().StaffingLevel; //Should be -1..
			CollectionAssert.AreEquivalent(expectedChanges, changes);
		}


		[Test]
		public void ShouldSplitResourceOverWholeSkillIntervalIfRequestIsNotCoveringWholeIntervalAndRequestIsLargerThanShortestInterval()
		{
			var requestLength = new TimeSpan(0, 20, 0);
			var request = createNewRequest(requestLength);
			populateReadModel();

			var startDateTime1 = request.Request.Period.StartDateTime;
			var endDateTime1 = startDateTime1.AddMinutes(15);

			var startDateTime2 = endDateTime1;
			var endDateTime2 = startDateTime2.AddMinutes(15);

			var changes = Target.AllocateResource(request, _now);

			var expectedChanges = new List<StaffingIntervalChange>()
			{
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					SkillId = _primarySkill.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedPrimary/(totaloverstaffed1)
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime2,
					EndDateTime = endDateTime2,
					SkillId = _primarySkill.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedPrimary/(totaloverstaffed2*3)
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedUnsorted15/(totaloverstaffed1)
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime2,
					EndDateTime = endDateTime2,
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedUnsorted15/(totaloverstaffed2*3)
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime2,
					SkillId = _unsortedSkill30.Id.GetValueOrDefault(),
					StaffingLevel = -(overstaffedUnsorted30/(totaloverstaffed1*2) + overstaffedUnsorted30/(totaloverstaffed2*6))
				},
			};
			//var sum = expectedChanges.Sum(x => x.StaffingLevel) + expectedChanges.Last().StaffingLevel; //Should be -1.333..
			CollectionAssert.AreEquivalent(expectedChanges, changes);
		}



		[Test]
		public void ShouldSplitResourceOverWholeSkillIntervalIfRequestIsNotCoveringWholeInterval()
		{
			var requestLength = new TimeSpan(0, 5, 0);
			var request = createNewRequest(requestLength);
			populateReadModel();

			var startDateTime1 = request.Request.Period.StartDateTime;
			var endDateTime1 = startDateTime1.AddMinutes(15);

			var startDateTime2 = endDateTime1;
			var endDateTime2 = startDateTime2.AddMinutes(15);

			var changes = Target.AllocateResource(request, _now);

			var expectedChanges = new List<StaffingIntervalChange>()
			{
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					SkillId = _primarySkill.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedPrimary/(totaloverstaffed1*3)
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedUnsorted15/(totaloverstaffed1*3)
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime2,
					SkillId = _unsortedSkill30.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedUnsorted30/(totaloverstaffed1*6)
				},
			};
			//var sum = expectedChanges.Sum(x => x.StaffingLevel) + expectedChanges.Last().StaffingLevel; //Should be -0.333..
			CollectionAssert.AreEquivalent(expectedChanges, changes);
		}

		[Test]
		public void ShouldOnlyAddResourceChangeForTargetedIntervals()
		{
			var requestLength = new TimeSpan(0, 15, 0);
			var request = createNewRequest(requestLength);
			populateReadModel();

			var startDateTime1 = request.Request.Period.StartDateTime;
			var endDateTime1 = startDateTime1.AddMinutes(15);

			var startDateTime2 = endDateTime1;
			var endDateTime2 = startDateTime2.AddMinutes(15);

			var changes = Target.AllocateResource(request, _now);

			var expectedChanges = new List<StaffingIntervalChange>()
			{
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					SkillId = _primarySkill.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedPrimary/totaloverstaffed1
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedUnsorted15/totaloverstaffed1
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime2,
					SkillId = _unsortedSkill30.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedUnsorted30/(totaloverstaffed1*2) //resource shared for whole period
				},
			};
			
			//var sum = expectedChanges.Sum(x => x.StaffingLevel) + expectedChanges.Last().StaffingLevel; //Should be -1
			CollectionAssert.AreEquivalent(expectedChanges, changes);
		}

		[Test]
		public void ShouldResourceAllocateOnIntervalsIncludingRequestStart()
		{
			var requestLength = new TimeSpan(0, 15, 0);
			var offset = new TimeSpan(0, 5, 0);
			var request = createNewRequest(requestLength, offset);
			populateReadModel();

			var startDateTime1 = request.Request.Period.StartDateTime.Add(-offset);
			var endDateTime1 = startDateTime1.AddMinutes(15);

			var startDateTime2 = endDateTime1;
			var endDateTime2 = startDateTime2.AddMinutes(15);

			var changes = Target.AllocateResource(request, _now);

			var expectedChanges = new List<StaffingIntervalChange>()
			{
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					SkillId = _primarySkill.Id.GetValueOrDefault(),
					StaffingLevel = -(overstaffedPrimary*2)/(totaloverstaffed1*3)
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime2,
					EndDateTime = endDateTime2,
					SkillId = _primarySkill.Id.GetValueOrDefault(),
					StaffingLevel = -(overstaffedPrimary)/(totaloverstaffed1*3)
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StaffingLevel = -(overstaffedUnsorted15*2)/(totaloverstaffed1*3)
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime2,
					EndDateTime = endDateTime2,
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StaffingLevel = -(overstaffedUnsorted15)/(totaloverstaffed1*3)
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime2,
					SkillId = _unsortedSkill30.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedUnsorted30/(totaloverstaffed1*2) 
				},
			};

			//var sum = expectedChanges.Sum(x => x.StaffingLevel) + expectedChanges.Last().StaffingLevel; //Should be -1
			CollectionAssert.AreEquivalent(expectedChanges, changes);
		}

		[Test]
		public void ShouldUpdateResourcesWhenApproveBySplitResourceDependentOnStaffing()
		{
			var requestLength = new TimeSpan(0, 30, 0);
			var request = createNewRequest(requestLength);
			populateReadModel();

			var startDateTime1 = request.Request.Period.StartDateTime;
			var endDateTime1 = startDateTime1.AddMinutes(15);

			var startDateTime2 = endDateTime1;
			var endDateTime2 = startDateTime2.AddMinutes(15);

			var changes = Target.AllocateResource(request, _now);

			var expectedChanges = new List<StaffingIntervalChange>()
			{
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					SkillId = _primarySkill.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedPrimary/totaloverstaffed1
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime2,
					EndDateTime = endDateTime2,
					SkillId = _primarySkill.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedPrimary/totaloverstaffed2
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedUnsorted15/totaloverstaffed1
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime2,
					EndDateTime = endDateTime2,
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedUnsorted15/totaloverstaffed2
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime2,
					SkillId = _unsortedSkill30.Id.GetValueOrDefault(),
					StaffingLevel = -(overstaffedUnsorted30/totaloverstaffed1 + overstaffedUnsorted30/totaloverstaffed2)/2 
				},
			};
		//	var sum = expectedChanges.Sum(x => x.StaffingLevel) + expectedChanges.Last().StaffingLevel; //Should be -2
			CollectionAssert.AreEquivalent(expectedChanges, changes);
		}
		[Test]
		public void ShouldOnlyUpdateResorucesOnOpenSkills()
		{
			var requestLength = new TimeSpan(0, 45, 0);
			var request = createNewRequest(requestLength);
			populateReadModel();

			var startDateTime1 = request.Request.Period.StartDateTime;
			var endDateTime1 = startDateTime1.AddMinutes(15);

			var startDateTime2 = endDateTime1;
			var endDateTime2 = startDateTime2.AddMinutes(15);

			var startDateTime3 = endDateTime2;
			var endDateTime3 = startDateTime3.AddMinutes(15);

			var changes = Target.AllocateResource(request, _now);

			var expectedChanges = new List<StaffingIntervalChange>()
			{
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					SkillId = _primarySkill.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedPrimary/totaloverstaffed1
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime2,
					EndDateTime = endDateTime2,
					SkillId = _primarySkill.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedPrimary/totaloverstaffed2
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedUnsorted15/totaloverstaffed1
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime2,
					EndDateTime = endDateTime2,
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedUnsorted15/totaloverstaffed2
				},
					new StaffingIntervalChange()
				{
					StartDateTime = startDateTime3,
					EndDateTime = endDateTime3,
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StaffingLevel = -overstaffedUnsorted15/totaloverstaffed3
				},
				new StaffingIntervalChange()
				{
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime2,
					SkillId = _unsortedSkill30.Id.GetValueOrDefault(),
					StaffingLevel = -(overstaffedUnsorted30/totaloverstaffed1 + overstaffedUnsorted30/totaloverstaffed2)/2
				},
			};
			//var sum = expectedChanges.Sum(x => x.StaffingLevel) + expectedChanges.Last().StaffingLevel; //Should be -2
			CollectionAssert.AreEquivalent(expectedChanges, changes);
		}


		private void populateReadModel()
		{
			var startDateTime1 = _requestStart;
			var endDateTime1 = startDateTime1.AddMinutes(15);

			var startDateTime2 = endDateTime1;
			var endDateTime2 = startDateTime2.AddMinutes(15);

			var startDateTime3 = endDateTime2;
			var endDateTime3 = startDateTime3.AddMinutes(15);


			var resourceList = new List<SkillStaffingInterval>
			{
				new SkillStaffingInterval()
				{
					SkillId = _primarySkill.Id.GetValueOrDefault(),
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					Forecast = forecastedPrimary,
					ForecastWithShrinkage = forecastedPrimary,
					StaffingLevel = staffingPrimary
				},
				new SkillStaffingInterval()
				{
					SkillId = _primarySkill.Id.GetValueOrDefault(),
					StartDateTime = startDateTime2,
					EndDateTime = endDateTime2,
					Forecast = forecastedPrimary,
					ForecastWithShrinkage = forecastedPrimary,
					StaffingLevel = staffingPrimary
				},
				new SkillStaffingInterval()
				{
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					Forecast = forecastedUnsorted15,
					ForecastWithShrinkage = forecastedUnsorted15,
					StaffingLevel = staffingUnsorted15
				},
				new SkillStaffingInterval()
				{
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StartDateTime = startDateTime2,
					EndDateTime = endDateTime2,
					Forecast = forecastedUnsorted15,
					ForecastWithShrinkage = forecastedUnsorted15,
					StaffingLevel = staffingUnsorted15
				},
				new SkillStaffingInterval()
				{
					SkillId = _unsortedSkill15.Id.GetValueOrDefault(),
					StartDateTime = startDateTime3,
					EndDateTime = endDateTime3,
					Forecast = forecastedUnsorted15,
					ForecastWithShrinkage = forecastedUnsorted15,
					StaffingLevel = staffingUnsorted15
				},
				new SkillStaffingInterval()
				{
					SkillId = _unsortedSkill30.Id.GetValueOrDefault(),
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime2,
					Forecast = forecastedUnsorted30,
					ForecastWithShrinkage = forecastedUnsorted30,
					StaffingLevel = staffingUnsorted30
				},
				new SkillStaffingInterval()
				{
					SkillId = _secondarySkill.Id.GetValueOrDefault(),
					StartDateTime = startDateTime1,
					EndDateTime = endDateTime1,
					Forecast = forecastedSecondary,
					ForecastWithShrinkage = forecastedSecondary,
					StaffingLevel = staffingSecondary
				},
				new SkillStaffingInterval()
				{
					SkillId = _secondarySkill.Id.GetValueOrDefault(),
					StartDateTime = startDateTime2,
					EndDateTime = endDateTime2,
					Forecast = forecastedSecondary,
					ForecastWithShrinkage = forecastedSecondary,
					StaffingLevel = staffingSecondary
				}
			};

			ScheduleForecastSkillReadModelRepository.Persist(resourceList, _now);
		}

		private IPersonRequest createNewRequest(TimeSpan requestLength, TimeSpan offset)
		{
			var period = new DateTimePeriod(_requestStart.Add(offset), _requestStart.Add(requestLength).Add(offset));

			ConfigReader.FakeSetting("FakeIntradayUtcStartDateTime", "2016-03-14 12:00");

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(absence);
			var person = createAndSetupPerson(workflowControlSet);

			LoggedOnUser.SetFakeLoggedOnUser(person);
			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, person);

			var request = createAbsenceRequest(person, absence, period);

			return request;
		}


		private IPersonRequest createNewRequest(TimeSpan requestLength)
		{
			var period = new DateTimePeriod(_requestStart, _requestStart.Add(requestLength));

			ConfigReader.FakeSetting("FakeIntradayUtcStartDateTime", "2016-03-14 12:00");

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = createWorkFlowControlSet(absence);
			var person = createAndSetupPerson(workflowControlSet);

			LoggedOnUser.SetFakeLoggedOnUser(person);
			var newIdentity = new TeleoptiIdentity("test2", null, null, null, null);
			Thread.CurrentPrincipal = new TeleoptiPrincipal(newIdentity, person);

			var request = createAbsenceRequest(person, absence, period);

			return request;
		}

		private IPerson createAndSetupPerson(IWorkflowControlSet workflowControlSet)
		{
			_primarySkill = SkillFactory.CreateSkillWithId("PrimarySkill1").CascadingIndex(1);
			_secondarySkill = SkillFactory.CreateSkillWithId("SecondarySkill").CascadingIndex(2);
			_unsortedSkill15 = SkillFactory.CreateSkillWithId("NotCascadingSkill15", 15);
			_unsortedSkill30 = SkillFactory.CreateSkillWithId("NotCascadingSkill30", 30);


			var person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(2016, 03, 01), new[] { _primarySkill, _secondarySkill, _unsortedSkill15,_unsortedSkill30 });
			person.WorkflowControlSet = workflowControlSet;

			return person;
		}

		private static WorkflowControlSet createWorkFlowControlSet(IAbsence absence)
		{
			var workflowControlSet = new WorkflowControlSet {AbsenceRequestWaitlistEnabled = false};
			workflowControlSet.SetId(Guid.NewGuid());

			var dateOnlyPeriod = new DateOnlyPeriod(new DateOnly(2016, 01, 01), new DateOnly(2016, 12, 31));

			var absenceRequestOpenPeriod = new AbsenceRequestOpenDatePeriod()
			{
				Absence = absence,
				PersonAccountValidator = new PersonAccountBalanceValidator(),
				Period = dateOnlyPeriod,
				OpenForRequestsPeriod = dateOnlyPeriod
			};

			workflowControlSet.InsertPeriod(absenceRequestOpenPeriod, 0);

			return workflowControlSet;
		}


		private PersonRequest createAbsenceRequest(IPerson person, IAbsence absence, DateTimePeriod requestDateTimePeriod)
		{
			var personRequest = new PersonRequest(person, new AbsenceRequest(absence, requestDateTimePeriod)).WithId();
			
			personRequest.SetCreated(_now);
			PersonRequestRepository.Add(personRequest);

			return personRequest;
		}
	}
}