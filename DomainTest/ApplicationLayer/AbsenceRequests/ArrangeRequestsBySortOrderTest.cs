using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AbsenceWaitlisting;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.AbsenceRequests
{
	public class ArrangeRequestsBySortOrderTest
	{
		private ArrangeRequestsByProcessOrder _target;

		[SetUp]
		public void Setup()
		{
			_target = new ArrangeRequestsByProcessOrder();
		}

		[Test]
		public void ShouldReturnSortedListBasedOnFirstCome()
		{
			var personReuqestList = new List<IPersonRequest>();
			var personReq1 = getWaitListedPersonRequest(new DateTime(2016, 03, 01, 10, 10, 0, DateTimeKind.Utc),
				WaitlistProcessOrder.FirstComeFirstServed, new DateOnly(2005, 03, 01));
			var personReq2 = getWaitListedPersonRequest(new DateTime(2016, 03, 01, 10, 0, 0, DateTimeKind.Utc),
				WaitlistProcessOrder.FirstComeFirstServed, new DateOnly(2016, 03, 01));
			var personReq3 = getWaitListedPersonRequest(new DateTime(2016, 03, 01, 11, 0, 0, DateTimeKind.Utc),
				WaitlistProcessOrder.FirstComeFirstServed, new DateOnly(1983, 03, 01));
			var personReq4 = getPersonRequest(new DateTime(2016, 03, 01, 12, 0, 0, DateTimeKind.Utc), new DateOnly(1983, 03, 01));
			personReuqestList.Add(personReq1);
			personReuqestList.Add(personReq2);
			personReuqestList.Add(personReq3);
			personReuqestList.Add(personReq4);

			var sortedList = _target.GetRequestsSortedByDate(personReuqestList).ToList();
			sortedList[0].Should().Be.EqualTo(personReq2);
			sortedList[1].Should().Be.EqualTo(personReq1);
			sortedList[2].Should().Be.EqualTo(personReq3);
			sortedList[3].Should().Be.EqualTo(personReq4);
		}

		[Test]
		public void ShouldIgnoreSeneriotyWhenFetchingFirstComeRequestsTypes()
		{
			var personReuqestList = new List<IPersonRequest>();
			var personReq1 = getWaitListedPersonRequest(new DateTime(2016, 03, 01, 10, 10, 0, DateTimeKind.Utc),
				WaitlistProcessOrder.FirstComeFirstServed, new DateOnly(2005, 03, 01));
			var personReq2 = getWaitListedPersonRequest(new DateTime(2016, 03, 01, 10, 0, 0, DateTimeKind.Utc),
				WaitlistProcessOrder.BySeniority, new DateOnly(2016, 03, 01));
			var personReq3 = getWaitListedPersonRequest(new DateTime(2016, 03, 01, 11, 0, 0, DateTimeKind.Utc),
				WaitlistProcessOrder.FirstComeFirstServed, new DateOnly(1983, 03, 01));
			var personReq4 = getPersonRequest(new DateTime(2016, 03, 01, 12, 0, 0, DateTimeKind.Utc), new DateOnly(1983, 03, 01));
			personReuqestList.Add(personReq1);
			personReuqestList.Add(personReq2);
			personReuqestList.Add(personReq3);
			personReuqestList.Add(personReq4);

			var sortedList = _target.GetRequestsSortedByDate(personReuqestList).ToList();
			sortedList.Count.Should().Be.EqualTo(3);
			sortedList[0].Should().Be.EqualTo(personReq1);
			sortedList[1].Should().Be.EqualTo(personReq3);
			sortedList[2].Should().Be.EqualTo(personReq4);
		}

		[Test]
		public void ShouldReturnSortedListBasedOnSeniority()
		{
			var personReuqestList = new List<IPersonRequest>();
			var personReq1 = getWaitListedPersonRequest(new DateTime(2016, 03, 01, 10, 10, 0, DateTimeKind.Utc),
				WaitlistProcessOrder.BySeniority, new DateOnly(2005, 03, 01));
			var personReq2 = getWaitListedPersonRequest(new DateTime(2016, 03, 01, 10, 0, 0, DateTimeKind.Utc),
				WaitlistProcessOrder.BySeniority, new DateOnly(2016, 03, 01));
			var personReq3 = getWaitListedPersonRequest(new DateTime(2016, 03, 01, 11, 0, 0, DateTimeKind.Utc),
				WaitlistProcessOrder.FirstComeFirstServed, new DateOnly(2000, 03, 01));
			var personReq4 = getPersonRequest(new DateTime(2016, 03, 01, 12, 0, 0, DateTimeKind.Utc), new DateOnly(1983, 03, 01));
			personReuqestList.Add(personReq1);
			personReuqestList.Add(personReq2);
			personReuqestList.Add(personReq3);
			personReuqestList.Add(personReq4);

			var sortedList = _target.GetRequestsSortedBySeniority(personReuqestList).ToList();
			sortedList.Count.Should().Be.EqualTo(2);
			sortedList[0].Should().Be.EqualTo(personReq1);
			sortedList[1].Should().Be.EqualTo(personReq2);
		}

		[Test]
		public void ShouldSortOnCreatedOnIfSeneriotyIsSame()
		{
			var personReuqestList = new List<IPersonRequest>();
			var personReq1 = getWaitListedPersonRequest(new DateTime(2016, 03, 01, 10, 10, 0, DateTimeKind.Utc),WaitlistProcessOrder.BySeniority, new DateOnly(2005, 03, 01));
			var personReq2 = getWaitListedPersonRequest(new DateTime(2016, 03, 01, 10, 0, 0, DateTimeKind.Utc),WaitlistProcessOrder.BySeniority, new DateOnly(2005, 03, 01));
			var personReq4 = getPersonRequest(new DateTime(2016, 03, 01, 12, 0, 0, DateTimeKind.Utc), new DateOnly(1983, 03, 01));
			personReuqestList.Add(personReq1);
			personReuqestList.Add(personReq2);
			personReuqestList.Add(personReq4);

			var sortedList = _target.GetRequestsSortedBySeniority(personReuqestList).ToList();
			sortedList.Count.Should().Be.EqualTo(2);
			sortedList[0].Should().Be.EqualTo(personReq2);
			sortedList[1].Should().Be.EqualTo(personReq1);
		}


		private IPersonRequest getWaitListedPersonRequest(DateTime createdOn, WaitlistProcessOrder processOrder,
			DateOnly startedDate)
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(startedDate);
			var wcs = new WorkflowControlSet()
			{
				AbsenceRequestWaitlistEnabled = true,
				AbsenceRequestWaitlistProcessOrder = processOrder
			};
			wcs.SetId(Guid.NewGuid());
			person.WorkflowControlSet = wcs;
			var period = new DateTimePeriod(new DateTime(2016, 03, 01, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 2, 0, 0, 0, DateTimeKind.Utc));
			var personReq1 = new PersonRequest(person, new AbsenceRequest(new Absence(), period));
			personReq1.SetCreated(createdOn);
			return personReq1;
		}

		private IPersonRequest getPersonRequest(DateTime createdOn, DateOnly startedDate)
		{
			var person = PersonFactory.CreatePersonWithPersonPeriod(startedDate);
			person.WorkflowControlSet = new WorkflowControlSet()
			{
				AbsenceRequestWaitlistEnabled = false
			};
			var period = new DateTimePeriod(new DateTime(2016, 03, 01, 0, 0, 0, DateTimeKind.Utc),
				new DateTime(2016, 03, 2, 0, 0, 0, DateTimeKind.Utc));
			var personReq1 = new PersonRequest(person, new AbsenceRequest(new Absence(), period));
			personReq1.SetCreated(createdOn);
			return personReq1;
		}
	}

	public static class PersonRequestExtensions
	{
		public static void SetCreated(this PersonRequest request, DateTime timestamp)
		{
			var field = typeof(PersonRequest).GetProperty(nameof(request.CreatedOn),
				BindingFlags.Instance | BindingFlags.Public);
			field.SetValue(request, timestamp);
		}
	}
}