using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.ClipBoard;
using Teleopti.Ccc.TestCommon.FakeData;


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
			_source.PersonAbsenceCollection(true).Should().Not.Be.Empty();

			IScheduleDay result = _target.DoPaste(_source);
			result.PersonAbsenceCollection(true).Should().Be.Empty(); // absence deleted
		}

		[Test]
		public void ShouldDeleteAbsenceEndingEarlierThanScheduleDayStarts()
		{
			_source.CreateAndAddAbsence(new AbsenceLayer(new Absence(), new DateTimePeriod(2015, 08, 26, 08, 2015, 08, 26, 10)));
			_source.PersonAbsenceCollection(true).Should().Not.Be.Empty();

			IScheduleDay result = _target.DoPaste(_source);

			result.PersonAbsenceCollection(true).Should().Be.Empty(); // absence deleted
		}

		[Test]
		public void ShouldPreserveShortAbsence()
		{
			_source.CreateAndAddAbsence(new AbsenceLayer(new Absence(), new DateTimePeriod(2015, 08, 27, 08, 2015, 08, 27, 12)));

			IScheduleDay result = _target.DoPaste(_source);

			var personAbsence = result.PersonAbsenceCollection(true).First();

			personAbsence.Period.StartDateTime.Should().Be.EqualTo(new DateTime(2015, 08, 27, 08, 00, 00));
			personAbsence.Period.EndDateTime.Should().Be.EqualTo(new DateTime(2015, 08, 27, 12, 00, 00));
		}

		[Test]
		public void ShouldCutLongAbsenceAtStartOfScheduleDay()
		{
			_source.CreateAndAddAbsence(new AbsenceLayer(new Absence(), new DateTimePeriod(2015, 08, 26, 10, 2015, 08, 27, 10)));

			IScheduleDay result = _target.DoPaste(_source);

			var personAbsence = result.PersonAbsenceCollection(true).First();

			var scheduleDayLocalStartTime = _source.DateOnlyAsPeriod.Period().StartDateTimeLocal(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
			personAbsence.Period.StartDateTimeLocal(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone).Should().Be.EqualTo(scheduleDayLocalStartTime);
		}

		[Test]
		public void ShouldCutLongAbsenceAtEndOfScheduleDay()
		{
			_source.CreateAndAddAbsence(new AbsenceLayer(new Absence(), new DateTimePeriod(2015, 08, 27, 15, 2015, 08, 29, 10)));

			IScheduleDay result = _target.DoPaste(_source);

			var personAbsence = result.PersonAbsenceCollection(true).First();

			var scheduleDayLocalEndDateTime = _source.DateOnlyAsPeriod.Period().EndDateTimeLocal(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);
			
			personAbsence.Period.EndDateTimeLocal(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone).Should().Be.EqualTo(scheduleDayLocalEndDateTime);
		}

	}
}