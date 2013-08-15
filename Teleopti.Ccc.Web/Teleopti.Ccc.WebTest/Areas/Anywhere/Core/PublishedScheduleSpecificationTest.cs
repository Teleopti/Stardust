﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Core
{
	[TestFixture]
	public class PublishedScheduleSpecificationTest
	{
		private PublishedScheduleSpecification _target;
		private ISchedulePersonProvider _schedulePersonProvider;
		private Guid _teamId;
		private DateOnly _date;
		private IPersonScheduleDayReadModel _personSchedule;
		private IPerson _person1;
		private IEnumerable<IPerson> _persons;
		private IWorkflowControlSet _workflowControlSet;
		

		private MockRepository _mock; 

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_schedulePersonProvider = _mock.Stub<ISchedulePersonProvider>();
			_teamId = Guid.NewGuid();
			_date = new DateOnly(2013, 02, 01);
			_personSchedule = _mock.Stub<IPersonScheduleDayReadModel>();
			_person1 = _mock.Stub<IPerson>();
			_persons = new List<IPerson>{ _person1 };
			_workflowControlSet = _mock.Stub<IWorkflowControlSet>();
		}

		[Test]
		public void CanInstanciate()
		{
			_target = new PublishedScheduleSpecification(_schedulePersonProvider, _teamId, _date);
			Assert.IsNotNull(_target);
		}

		[Test]
		public void ShouldReturnFalseIfScheduleBelongsToOtherTeam()
		{
			using (_mock.Record())
			{
				_schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(_date, _teamId, null))
					.IgnoreArguments()
			        .Return(_persons);
			}
			using (_mock.Playback())
			{
				_target = new PublishedScheduleSpecification(_schedulePersonProvider, _teamId, _date);

				bool result = _target.IsSatisfiedBy(_personSchedule);
				result.Should().Be.False();
			}
		}

		[Test]
		public void ShouldReturnFalseIfScheduleNotPublished()
		{
			Guid personId = Guid.NewGuid();

			using (_mock.Record())
			{
				_schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(_date, _teamId, null))
					.IgnoreArguments()
					.Return(_persons);
				_person1.Stub(x => x.Id)
				    .Return(personId);
				_personSchedule.PersonId = personId;
				_person1.WorkflowControlSet = _workflowControlSet;
				// set the date before the publish date
				_workflowControlSet.SchedulePublishedToDate = _date.AddDays(-2);
			}
			using (_mock.Playback())
			{
				_target = new PublishedScheduleSpecification(_schedulePersonProvider, _teamId, _date);

				bool result = _target.IsSatisfiedBy(_personSchedule);
				result.Should().Be.False();
			}
		}

		[Test]
		public void ShouldReturnTrueIfSchedulePublished()
		{
			Guid personId = Guid.NewGuid();

			using (_mock.Record())
			{
				_schedulePersonProvider.Stub(x => x.GetPermittedPersonsForTeam(_date, _teamId, null))
					.IgnoreArguments()
					.Return(_persons);
				_person1.Stub(x => x.Id)
					.Return(personId);
				_personSchedule.PersonId = personId;
				_person1.WorkflowControlSet = _workflowControlSet;
				// set the date after the publish date
				_workflowControlSet.SchedulePublishedToDate = _date.AddDays(2);
			}
			using (_mock.Playback())
			{
				_target = new PublishedScheduleSpecification(_schedulePersonProvider, _teamId, _date);

				bool result = _target.IsSatisfiedBy(_personSchedule);
				result.Should().Be.True();
			}
		}
	}
}
