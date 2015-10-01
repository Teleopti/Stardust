using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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

		//[Test]
		//[ExpectedException(typeof(ArgumentException))]
		//public void SourceShouldBeClone()
		//{
		//	_source.CreateAndAddAbsence(new AbsenceLayer(new Absence(), new DateTimePeriod(2015, 08, 27, 2015, 08, 28)));
		//	var personAbsence = _source.PersistableScheduleDataCollection().First();
		//	personAbsence.SetId(Guid.NewGuid());
		//	IScheduleDay result = _target.DoPaste(_source);
		//}

		[Test]
		public void ShouldDeleteAbsenceStartingNextDay()
		{
			_source.CreateAndAddAbsence(new AbsenceLayer(new Absence(), new DateTimePeriod(2015, 08, 28, 08, 2015, 08, 28, 10)));
			Assert.IsTrue(_source.PersonAbsenceCollection(true).Any());

			IScheduleDay result = _target.DoPaste(_source);
			Assert.IsFalse(result.PersonAbsenceCollection(true).Any());
		}


		[Test]
		public void ShouldCutLongAbsenceAtStartOfDay()
		{
			_source.CreateAndAddAbsence(new AbsenceLayer(new Absence(), new DateTimePeriod(2015, 08, 26, 2015, 08, 28)));

			IScheduleDay result = _target.DoPaste(_source);

			var personAbsence = result.PersonAbsenceCollection(true).First();

			var scheduleDayStartDateTime = _source.DateOnlyAsPeriod.Period().StartDateTime;
			Assert.IsTrue(personAbsence.Period.StartDateTime==scheduleDayStartDateTime);
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
		public void ShouldCutLongAbsenceAtEndOfDay()
		{
			_source.CreateAndAddAbsence(new AbsenceLayer(new Absence(), new DateTimePeriod(2015, 08, 27, 2015, 08, 29)));

			IScheduleDay result = _target.DoPaste(_source);

			var personAbsence = result.PersonAbsenceCollection(true).First();

			var scheduleDayEndDateTime = _source.DateOnlyAsPeriod.Period().EndDateTime;
			Assert.IsTrue(personAbsence.Period.EndDateTime == scheduleDayEndDateTime);
		}

	}
}