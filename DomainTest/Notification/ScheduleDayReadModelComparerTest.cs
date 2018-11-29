using System;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Notification
{
    [TestFixture]
    public class ScheduleDayReadModelComparerTest
    {
        private IPerson _person;
        private ScheduleDayReadModelComparer _scheduleDayReadModelComparer;

        [SetUp]
        public void Setup()
        {
           
            _person = PersonFactory.CreatePerson("test");
            _person.PermissionInformation.SetCulture(CultureInfo.CurrentCulture);
            _person.PermissionInformation.SetUICulture(CultureInfo.CurrentUICulture);
            _scheduleDayReadModelComparer = new ScheduleDayReadModelComparer();
        }

        [Test]
        public void ShouldGetNullMessageIfNoSignificantChange()
        {
            var newReadModel = new ScheduleDayReadModel();
            newReadModel.StartDateTime = new DateTime();
            newReadModel.EndDateTime = new DateTime();
            newReadModel.Workday = true;

            var existingReadModel = new ScheduleDayReadModel();
            existingReadModel.StartDateTime = new DateTime();
            existingReadModel.EndDateTime = new DateTime();
            existingReadModel.Workday = true;

            var message = _scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, existingReadModel,
                                                                              _person.PermissionInformation.Culture(), new DateOnly(2012,08,31));
            Assert.IsNull(message);
        }

        [Test]
        public void ShouldReturnNotificationIfSignificantChangeExists()
        {
            var newReadModel = new ScheduleDayReadModel();
            newReadModel.StartDateTime = new DateTime();
            newReadModel.EndDateTime = new DateTime();
            newReadModel.Workday = true;

            var existingReadModel = new ScheduleDayReadModel();
            existingReadModel.StartDateTime = new DateTime(2012,01,01);
            existingReadModel.EndDateTime = new DateTime(2012,01,01);
            existingReadModel.Workday = true;

            var message = _scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, existingReadModel,
                                                                              _person.PermissionInformation.Culture(), new DateOnly(2012,08,31));
            Assert.IsNotNull(message);
        }

        [Test]
        public void ShouldReturnNullIfBothAreOffDays()
        {
            var message = _scheduleDayReadModelComparer.FindSignificantChanges(null, null,
                                                                              _person.PermissionInformation.Culture(),
                                                                              new DateOnly(2012, 08, 31));
            Assert.IsNull(message);
        }

        [Test]
        public void ShouldReturnMessageIfChangeFromOffDayToWorkingDay()
        {
            var newReadModel = new ScheduleDayReadModel();
            newReadModel.StartDateTime = new DateTime();
            newReadModel.EndDateTime = new DateTime();
            newReadModel.Workday = true;

            var message = _scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, null,
                                                                              _person.PermissionInformation.Culture(),
                                                                              new DateOnly(2012, 08, 31));
            Assert.IsNotNull(message);
        }

        [Test]
        public void ShouldReturnMessageIfChangeFromWorkingDayToOffDay()
        {
            var existingReadModel = new ScheduleDayReadModel();
            existingReadModel.StartDateTime = new DateTime(2012, 01, 01);
            existingReadModel.EndDateTime = new DateTime(2012, 01, 01);
            existingReadModel.Workday = true;

            var message = _scheduleDayReadModelComparer.FindSignificantChanges(null, existingReadModel,
                                                                              _person.PermissionInformation.Culture(),
                                                                              new DateOnly(2012, 01, 01));
            Assert.IsNotNull(message);
        }

        [Test]
        public void ShouldNotDetectFromNullToNoWorkday()
        {
            var newReadModel = new ScheduleDayReadModel();
            newReadModel.StartDateTime = new DateTime();
            newReadModel.EndDateTime = new DateTime();
            newReadModel.Workday = false;

            var message = _scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, null,
                                                                              _person.PermissionInformation.Culture(),
                                                                              DateOnly.Today);
            Assert.IsNull(message);
        }

        [Test]
        public void ShouldNotDetectFromNoWorkdayToNull()
        {
            var existingReadModel = new ScheduleDayReadModel();
            existingReadModel.StartDateTime = new DateTime();
            existingReadModel.EndDateTime = new DateTime();
            existingReadModel.Workday = false;

            var message = _scheduleDayReadModelComparer.FindSignificantChanges(null, existingReadModel,
                                                                              _person.PermissionInformation.Culture(),
                                                                              DateOnly.Today);
            Assert.IsNull(message);
        }

        [Test]
        public void ShouldDetectIfWorkdayIsChangedToNoWorkday()
        {
            var newReadModel = new ScheduleDayReadModel();
            newReadModel.StartDateTime = new DateTime();
            newReadModel.EndDateTime = new DateTime();
            newReadModel.Workday = false;

            var existingReadModel = new ScheduleDayReadModel();
            existingReadModel.StartDateTime = new DateTime();
            existingReadModel.EndDateTime = new DateTime();
            existingReadModel.Workday = true;

            var message = _scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, existingReadModel,
                                                                              _person.PermissionInformation.Culture(),
                                                                              DateOnly.Today);
            Assert.IsNotNull(message);
        }

        [Test]
        public void ShouldDetectIfNoWorkdayIsChangedToWorkday()
        {
            var newReadModel = new ScheduleDayReadModel();
            newReadModel.StartDateTime = new DateTime();
            newReadModel.EndDateTime = new DateTime();
            newReadModel.Workday = true;

            var existingReadModel = new ScheduleDayReadModel();
            existingReadModel.StartDateTime = new DateTime();
            existingReadModel.EndDateTime = new DateTime();
            existingReadModel.Workday = false;

            var message = _scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, existingReadModel,
                                                                              _person.PermissionInformation.Culture(),
                                                                              DateOnly.Today);
            Assert.IsNotNull(message);
        }

        [Test]
        public void ShouldNotBotherAboutSecondsOrMilliseconds()
        {
            var newReadModel = new ScheduleDayReadModel
            {
                StartDateTime = new DateTime(2016, 4, 29, 8, 10, 25, 25),
                EndDateTime = new DateTime(2016, 4, 29, 16, 20, 25, 25),
                Workday = true
            };

            var existingReadModel = new ScheduleDayReadModel
            {
                StartDateTime = new DateTime(2016, 4, 29, 8, 10, 26, 111),
                EndDateTime = new DateTime(2016, 4, 29, 16, 20, 26, 111),
                Workday = true
            };

            var message = _scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, existingReadModel,
                                                                              _person.PermissionInformation.Culture(),
                                                                              DateOnly.Today);
            Assert.IsNull(message);
        }

        [Test]
        public void ShouldBeNoChangeIfBothAreNotWorkdays()
        {
            var newReadModel = new ScheduleDayReadModel
            {
                StartDateTime = new DateTime(2016, 4, 29, 8, 10,0),
                EndDateTime = new DateTime(2016, 4, 29, 16, 20, 0),
                Workday = false
            };

            var existingReadModel = new ScheduleDayReadModel
            {
                StartDateTime = new DateTime(2016, 4, 29, 9, 10, 0),
                EndDateTime = new DateTime(2016, 4, 29, 17, 20, 0),
                Workday = false
            };

            var message = _scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, existingReadModel,
                                                                              _person.PermissionInformation.Culture(),
                                                                              DateOnly.Today);
            Assert.IsNull(message);
        }

		[Test]
		public void ShouldReturnNotificationWhenOvertimeAddedOnDayOff()
		{
			var newReadModel = new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2012, 08, 31, 08, 00, 00),
				EndDateTime = new DateTime(2012, 08, 31, 09, 00, 00),
				Date = new DateTime(2012, 08, 31),
				Workday = false,
				WorkTimeTicks = TimeSpan.FromHours(1).Ticks,
				ContractTimeTicks = 0,
				Label = "FP",
				ColorCode = 0,
				NotScheduled = false,
				Version = 2
			};

			var existingReadModel = new ScheduleDayReadModel
			{
				Date = new DateTime(2012, 08, 31),
				Workday = false,
				WorkTimeTicks = 0,
				ContractTimeTicks = 0,
				Label = "FP",
				ColorCode = 0,
				NotScheduled = false,
				Version = 1
			};

			var message = _scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, existingReadModel,
				_person.PermissionInformation.Culture(), new DateOnly(2012, 08, 31));
			Assert.IsNotNull(message);
		}

		[Test]
		public void ShouldReturnNotificationWhenOvertimeAddedOnNonScheduledDay()
		{
			var newReadModel = new ScheduleDayReadModel
			{
				StartDateTime = new DateTime(2012, 08, 31, 08, 00, 00),
				EndDateTime = new DateTime(2012, 08, 31, 09, 00, 00),
				Date = new DateTime(2012, 08, 31),
				Workday = true,
				WorkTimeTicks = 0,
				ContractTimeTicks = 0,
				Label = "FP",
				ColorCode = 0,
				NotScheduled = false,
				Version = 2
			};

			var message = _scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, null,
				_person.PermissionInformation.Culture(), new DateOnly(2012, 08, 31));
			Assert.IsNotNull(message);
		}
	}
}
