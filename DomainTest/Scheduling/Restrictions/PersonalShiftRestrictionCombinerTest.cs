using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[TestFixture]
	public class PersonalShiftRestrictionCombinerTest
	{
		private PersonalShiftRestrictionCombiner _target;
		private MockRepository _mocks;
		private IScheduleDay _scheduleDay;
		private IEffectiveRestriction _restriction;
	
		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_restriction = _mocks.StrictMock<IEffectiveRestriction>();
			_target = new PersonalShiftRestrictionCombiner(new RestrictionCombiner());
		}

		[Test]
		public void VerifyCreate()
		{
			Assert.IsNotNull(_target);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void VerifyNullScheduleDayParameter()
		{
			_scheduleDay = null;
			_target.Combine(_scheduleDay, _restriction);
		}

		[Test]
		public void VerifyNullRestrictionParameter()
		{
			_restriction = null;
			IEffectiveRestriction result =
				_target.Combine(_scheduleDay, _restriction);
			Assert.IsNull(result);
		}

		[Test]
		public void VerifyEmptyPersonAssignmentCollection()
		{
			_scheduleDay.Stub(x => x.PersonAssignment()).Return(null);
			using (_mocks.Record()) {}
			using (_mocks.Playback())
				{
					IEffectiveRestriction result =
						_target.Combine(_scheduleDay, _restriction);
					Assert.IsNotNull(result);
					Assert.AreSame(_restriction, _restriction);
				}
		}

		[Test]
		public void VerifyAddEffectiveRestriction()
		{
			IPersonAssignment personAssignment = _mocks.StrictMock<IPersonAssignment>();
			IPerson person = PersonFactory.CreatePerson();
			IActivity activity = _mocks.StrictMock<IActivity>();

			_scheduleDay.Stub(x => x.PersonAssignment()).Return(personAssignment);
			_scheduleDay.Stub(x=>x.Person)
				.Return(person);
			personAssignment.Stub(x=>x.PersonalActivities())
				.Return(new []{new PersonalShiftLayer(activity, DateTimeFactory.CreateDateTimePeriodUtc()) });


			using(_mocks.Record())
			{
				Expect.Call(_restriction.Combine(null))
					.IgnoreArguments()
					.Return(_restriction);
			}
			using (_mocks.Playback())
			{
				IEffectiveRestriction result =
					_target.Combine(_scheduleDay, _restriction);
				Assert.IsNotNull(result);
				Assert.AreSame(_restriction, _restriction);
			}
		}
	}
}
