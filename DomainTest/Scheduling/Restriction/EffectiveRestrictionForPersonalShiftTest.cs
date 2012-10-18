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

namespace Teleopti.Ccc.DomainTest.Scheduling.Restriction
{
	[TestFixture]
	public class EffectiveRestrictionForPersonaShiftTest
	{
		private EffectiveRestrictionForPersonalShift _target;
		private MockRepository _mocks;
		private IScheduleDay _scheduleDay;
		private IEffectiveRestriction _restriction;
	
		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_restriction = _mocks.StrictMock<IEffectiveRestriction>();
			_target = new EffectiveRestrictionForPersonalShift();
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
		public void VerifyEmptyPersonAssignmentCollection()
		{
			using(_mocks.Record())
			{
				Expect.Call(_scheduleDay.PersonAssignmentCollection())
					.Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>()));
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

			IPersonAssignment personAssignment = _mocks.StrictMock<IPersonAssignment>();
			IPerson person = PersonFactory.CreatePerson();
			IPersonalShift personalShift = _mocks.StrictMock<IPersonalShift>();
			IActivity activity = _mocks.StrictMock<IActivity>();
			ILayerCollection<IActivity> layerCollection = new LayerCollection<IActivity>();
			layerCollection.Add(new ActivityLayer(activity, DateTimeFactory.CreateDateTimePeriodUtc()));

			using(_mocks.Record())
			{
				Expect.Call(_scheduleDay.PersonAssignmentCollection())
					.Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { personAssignment }));
				Expect.Call(_scheduleDay.Person)
					.Return(person);
				Expect.Call(personAssignment.PersonalShiftCollection)
					.Return(new ReadOnlyCollection<IPersonalShift>(new List<IPersonalShift> { personalShift }));
				Expect.Call(personalShift.LayerCollection)
					.Return(layerCollection);

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
