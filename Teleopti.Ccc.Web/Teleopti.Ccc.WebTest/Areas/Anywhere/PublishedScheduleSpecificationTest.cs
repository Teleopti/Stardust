using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere
{
	[TestFixture]
	public class PublishedScheduleSpecificationTest
	{
		private PublishedScheduleSpecification _target;
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
			_date = new DateOnly(2013, 02, 01);
			_personSchedule = _mock.Stub<IPersonScheduleDayReadModel>();
			_person1 = _mock.Stub<IPerson>();
			_persons = new List<IPerson>{ _person1 };
			_workflowControlSet = _mock.Stub<IWorkflowControlSet>();
		}

		[Test]
		public void CanInstanciate()
		{
			_target = new PublishedScheduleSpecification(_persons, _date);
			Assert.IsNotNull(_target);
		}

		[Test]
		public void ShouldReturnFalseIfScheduleBelongsToOtherTeam()
		{
			_target = new PublishedScheduleSpecification(_persons, _date);

			bool result = _target.IsSatisfiedBy(_personSchedule);
			result.Should().Be.False();
		}

		[Test]
		public void ShouldReturnFalseIfPersonHasNoWorkflowControlSet()
		{
			Guid personId = Guid.NewGuid();

			_person1.Stub(x => x.Id)
			        .Return(personId);
			_personSchedule.PersonId = personId;
			_person1.WorkflowControlSet = null;

			_target = new PublishedScheduleSpecification(_persons, _date);

			bool result = _target.IsSatisfiedBy(_personSchedule);
			result.Should().Be.False();
		}

		[Test]
		public void ShouldReturnFalseIfScheduleNotPublished()
		{
			Guid personId = Guid.NewGuid();

			_person1.Stub(x => x.Id)
			        .Return(personId);
			_personSchedule.PersonId = personId;
			_person1.WorkflowControlSet = _workflowControlSet;
			// set the date before the publish date
			_workflowControlSet.SchedulePublishedToDate = _date.AddDays(-2).Date;

			_target = new PublishedScheduleSpecification(_persons, _date);

			bool result = _target.IsSatisfiedBy(_personSchedule);
			result.Should().Be.False();
		}

		[Test]
		public void ShouldReturnTrueIfSchedulePublished()
		{
			Guid personId = Guid.NewGuid();

			using (_mock.Record())
			{
				_person1.Stub(x => x.Id)
					.Return(personId);
				_personSchedule.PersonId = personId;
				_person1.WorkflowControlSet = _workflowControlSet;
				// set the date after the publish date
				_workflowControlSet.SchedulePublishedToDate = _date.AddDays(2).Date;
			}
			using (_mock.Playback())
			{
				_target = new PublishedScheduleSpecification(_persons, _date);

				bool result = _target.IsSatisfiedBy(_personSchedule);
				result.Should().Be.True();
			}
		}

		[Test]
		public void ShouldReturnFalseIfScheduleIsNotPublished()
		{
			var personId = Guid.NewGuid();

			using (_mock.Record())
			{
				_person1.Stub(x => x.Id)
					.Return(personId);
				_personSchedule.PersonId = personId;
				_person1.WorkflowControlSet = _workflowControlSet;
				_workflowControlSet.SchedulePublishedToDate = _date.Date;
			}
			using (_mock.Playback())
			{
				_target = new PublishedScheduleSpecification(_persons, _date.AddDays(1));

				bool result = _target.IsSatisfiedBy(_personSchedule);
				result.Should().Be.False();
			}
		}
	}
}
