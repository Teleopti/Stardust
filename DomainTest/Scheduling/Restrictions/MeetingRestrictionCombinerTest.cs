using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[TestFixture]
	public class MeetingRestrictionCombinerTest
	{
		private MeetingRestrictionCombiner _target;
		private MockRepository _mocks;
		private IScheduleDay _scheduleDay;
		private IEffectiveRestriction _restriction;
	
		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_restriction = _mocks.StrictMock<IEffectiveRestriction>();
			_target = new MeetingRestrictionCombiner(new RestrictionCombiner());
		}

		[Test]
		public void VerifyCreate()
		{
			Assert.IsNotNull(_target);
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
		public void VerifyEmptyPersonMeetingCollection()
		{

			_scheduleDay.Stub(x=>x.PersonMeetingCollection())
				.Return(new IPersonMeeting[0]);

			using(_mocks.Record()) {}
			using (_mocks.Playback())
			{
				IEffectiveRestriction result =
					_target.Combine(_scheduleDay, _restriction);
				Assert.IsNotNull(result);
				Assert.AreSame(_restriction, _restriction);
			}
		}

		[Test]
		public void VerifyNullPersonMeetingCollection()
		{
			_scheduleDay.Stub(x => x.PersonMeetingCollection())
				.Return(null);
			IEffectiveRestriction result =
				_target.Combine(_scheduleDay, _restriction);
			Assert.IsNotNull(result);
			Assert.AreSame(_restriction, _restriction);
		}

		[Test]
		public void VerifyAddEffectiveRestriction()
		{

			IPersonMeeting personMeeting = _mocks.StrictMock<IPersonMeeting>();
			IPerson person = PersonFactory.CreatePerson();


				_scheduleDay.Stub(x=>x.PersonMeetingCollection())
					.Return(new [] {personMeeting});
				_scheduleDay.Stub(x=>x.Person)
					.Return(person);
				personMeeting.Stub(x=>x.Period)
					.Return(new DateTimePeriod());

				using (_mocks.Record())
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
