using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class PersonAssignmentByDateSorterTest
	{
		private PersonAssignmentByDateSorter _target;
		private IPersonAssignment _ass1;
		private IPersonAssignment _ass2;

		[SetUp]
		public void Setup()
		{
			_target = new PersonAssignmentByDateSorter();
			IPerson person = PersonFactory.CreatePerson();
			var dtp = new DateTimePeriod(2011, 4, 1, 2011, 4, 2);
			_ass1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, dtp);
			
			_ass2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, dtp);
			
		}

		[Test]
		public void SortingShouldBeConsistentWhenExactlyTheSameDateTimeAndCreatedOnIsSet()
		{
			ReflectionHelper.SetCreatedOn(_ass1, new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc));
			ReflectionHelper.SetCreatedOn(_ass2, new DateTime(2011, 1, 1, 8, 0, 1, DateTimeKind.Utc));
			var list1 = new List<IPersonAssignment> { _ass1, _ass2 };
			list1.Sort(_target);
			var list2 = new List<IPersonAssignment> { _ass2, _ass1 };
			list2.Sort(_target);

			Assert.AreSame(list1[0], list2[0]);
		}

		[Test]
		public void ShouldHandleSortingWhenCreatedOnIsNotSet()
		{
			var list1 = new List<IPersonAssignment> { _ass1, _ass2 };
			list1.Sort(_target);
			var list2 = new List<IPersonAssignment> { _ass2, _ass1 };
			list2.Sort(_target);

			Assert.AreNotSame(list1[0], list2[0]);
		}

		[Test]
		public void ShouldHandleSortingWhenCreatedOnIsNotSetOnOne()
		{
			ReflectionHelper.SetCreatedOn(_ass1, new DateTime(2011, 1, 1, 8, 0, 0, DateTimeKind.Utc));
			var list1 = new List<IPersonAssignment> { _ass1, _ass2 };
			list1.Sort(_target);
			var list2 = new List<IPersonAssignment> { _ass2, _ass1 };
			list2.Sort(_target);

			Assert.AreNotSame(list1[0], list2[0]);
		}

	}
}