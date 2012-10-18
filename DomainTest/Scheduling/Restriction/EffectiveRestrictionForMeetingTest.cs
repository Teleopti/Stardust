using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restriction
{
	[TestFixture]
	public class EffectiveRestrictionForMeetingTest
	{
		private EffectiveRestrictionForMeeting _target;
		private MockRepository _mocks;
		private IScheduleDay _scheduleDay;
		private IEffectiveRestriction _restriction;
	
		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_restriction = _mocks.StrictMock<IEffectiveRestriction>();
			_target = new EffectiveRestrictionForMeeting();
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
			_target.AddEffectiveRestriction(_scheduleDay, _restriction);
		}

		[Test]
		public void VerifyNullRestrictionParameter()
		{
			_restriction = null;
			IEffectiveRestriction result = 
				_target.AddEffectiveRestriction(_scheduleDay, _restriction);
			Assert.IsNull(result);
		}

		[Test]
		public void VerifyEmptyPersonMeetingCollection()
		{
			using(_mocks.Record())
			{
				Expect.Call(_scheduleDay.PersonMeetingCollection())
					.Return(new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>()));
			}
			using (_mocks.Playback())
			{
				IEffectiveRestriction result =
					_target.AddEffectiveRestriction(_scheduleDay, _restriction);
				Assert.IsNotNull(result);
				Assert.AreSame(_restriction, _restriction);
			}
		}

		[Test]
		public void VerifyAddEffectiveRestriction()
		{

			IPersonMeeting personMeeting = _mocks.StrictMock<IPersonMeeting>();
			IPerson person = PersonFactory.CreatePerson();

			using(_mocks.Record())
			{
				Expect.Call(_scheduleDay.PersonMeetingCollection())
					.Return(new ReadOnlyCollection<IPersonMeeting>(new List<IPersonMeeting>{personMeeting}));
				Expect.Call(_scheduleDay.Person)
					.Return(person);
				Expect.Call(personMeeting.Period)
					.Return(new DateTimePeriod());

				Expect.Call(_restriction.Combine(null))
					.IgnoreArguments()
					.Return(_restriction);

			}
			using (_mocks.Playback())
			{
				IEffectiveRestriction result =
					_target.AddEffectiveRestriction(_scheduleDay, _restriction);
				Assert.IsNotNull(result);
				Assert.AreSame(_restriction, _restriction);
			}
		}
	}
}
