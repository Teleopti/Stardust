using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
	[TestFixture]
	public class EffectiveRestrictionForPersonalShiftTest
	{
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void GetEffectiveRestrictionForPersonalShiftShouldThrowIfScheduleDayIsNull()
		{
			var effective = MockRepository.GenerateMock<IEffectiveRestriction>();
			new EffectiveRestrictionForPersonalShift().AddEffectiveRestriction(null, effective);
		}

		//[Test]
		//public void CanGetEffectiveRestrictionForPersonalShift()
		//{
		//    var dateTime = new DateTime(2010, 1, 1, 10, 0, 0, DateTimeKind.Utc);
		//    var period = new DateTimePeriod(dateTime, dateTime.AddHours(1));
		//    IPersonAssignment assignment = PersonAssignmentFactory.CreateAssignmentWithPersonalShift(_person, period);
		//    var assignments = new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { assignment });
		//    IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
		//                                                                          new EndTimeLimitation(),
		//                                                                          new WorkTimeLimitation(), null, null, null,
		//                                                                          new List<IActivityRestriction>());
		//    Expect.Call(_scheduleDay.PersonAssignmentCollection()).Return(assignments).Repeat.Twice();
		//    Expect.Call(_scheduleDay.Person).Return(_person);
		//    _mocks.ReplayAll();
		//    effectiveRestriction = MinMaxWorkTimeChecker.GetEffectiveRestrictionForPersonalShift(_scheduleDay, effectiveRestriction);
		//    Assert.AreEqual(TimeZoneHelper.ConvertFromUtc(dateTime, _permissionInformation.DefaultTimeZone()).TimeOfDay, effectiveRestriction.StartTimeLimitation.EndTime);
		//    Assert.AreEqual(TimeZoneHelper.ConvertFromUtc(dateTime.AddHours(1), _permissionInformation.DefaultTimeZone()).TimeOfDay, effectiveRestriction.EndTimeLimitation.StartTime);
		//    Assert.AreEqual(period.ElapsedTime(), effectiveRestriction.WorkTimeLimitation.StartTime);
		//    _mocks.VerifyAll();
		//}
	}
}
