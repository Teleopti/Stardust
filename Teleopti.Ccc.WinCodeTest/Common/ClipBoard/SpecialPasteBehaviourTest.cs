using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common.Clipboard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Common.Clipboard
{
	[TestFixture]
	public class SpecialPasteBehaviourTest
	{
		private SpecialPasteBehaviour _target;
		private Person _person;
		private DateOnly _dateOnly;
		private IScheduleDay _source;


		[SetUp]
		public void Setup()
		{
			_person = new Person();
			_dateOnly = new DateOnly(2015, 08, 27);
			_source = ScheduleDayFactory.Create(_dateOnly, _person);
			_target = new SpecialPasteBehaviour();
		}

		[Test]
		public void ShouldDeleteAbsenceStartingLaterThanScheduleDayEnds()
		{
			_source.CreateAndAddAbsence(new AbsenceLayer(new Absence(), new DateTimePeriod(2015, 08, 28, 08, 2015, 08, 28, 10)));
			Assert.IsTrue(_source.PersonAbsenceCollection(true).Any());

			IScheduleDay result = _target.DoPaste(_source);
			Assert.IsFalse(result.PersonAbsenceCollection(true).Any()); // absence deleted
		}

		[Test]
		public void ShouldDeleteAbsenceEndingEarlierThanScheduleDayStarts()
		{
			_source.CreateAndAddAbsence(new AbsenceLayer(new Absence(), new DateTimePeriod(2015, 08, 26, 08, 2015, 08, 26, 10)));
			Assert.IsTrue(_source.PersonAbsenceCollection(true).Any());

			IScheduleDay result = _target.DoPaste(_source);
			Assert.IsFalse(result.PersonAbsenceCollection(true).Any()); // absence deleted
		}

		[Test]
		public void ShouldPreserveShortAbsence()
		{
			_source.CreateAndAddAbsence(new AbsenceLayer(new Absence(), new DateTimePeriod(2015, 08, 27, 08, 2015, 08, 27, 12)));

			IScheduleDay result = _target.DoPaste(_source);

			var personAbsence = result.PersonAbsenceCollection(true).First();

			Assert.IsTrue(personAbsence.Period.StartDateTime == new DateTime(2015, 08, 27, 08, 00, 00));
			Assert.IsTrue(personAbsence.Period.EndDateTime == new DateTime(2015, 08, 27, 12, 00, 00));
		}

		[Test]
		public void ShouldCutLongAbsenceAtStartOfScheduleDay()
		{
			_source.CreateAndAddAbsence(new AbsenceLayer(new Absence(), new DateTimePeriod(2015, 08, 26, 10, 2015, 08, 27, 10)));

			IScheduleDay result = _target.DoPaste(_source);

			var personAbsence = result.PersonAbsenceCollection(true).First();

			var scheduleDayLocalStartTime = _source.DateOnlyAsPeriod.Period().LocalStartDateTime;
			Assert.IsTrue(personAbsence.Period.LocalStartDateTime == scheduleDayLocalStartTime);
		}

		[Test]
		public void ShouldCutLongAbsenceAtEndOfScheduleDay()
		{
			_source.CreateAndAddAbsence(new AbsenceLayer(new Absence(), new DateTimePeriod(2015, 08, 27, 15, 2015, 08, 29, 10)));

			IScheduleDay result = _target.DoPaste(_source);

			var personAbsence = result.PersonAbsenceCollection(true).First();

			var scheduleDayLocalEndDateTime = _source.DateOnlyAsPeriod.Period().LocalEndDateTime;
			Assert.IsTrue(personAbsence.Period.LocalEndDateTime == scheduleDayLocalEndDateTime);
		}

	}
}