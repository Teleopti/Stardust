using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Notification
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
            ScheduleDayReadModel newReadModel = new ScheduleDayReadModel();
            newReadModel.StartDateTime = new DateTime();
            newReadModel.EndDateTime = new DateTime();
            newReadModel.Workday = true;

            ScheduleDayReadModel existingReadModel = new ScheduleDayReadModel();
            existingReadModel.StartDateTime = new DateTime();
            existingReadModel.EndDateTime = new DateTime();
            existingReadModel.Workday = true;

            var message = _scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, existingReadModel,
                                                                              _person.PermissionInformation.Culture(), new DateOnly(2012,08,31));
            Assert.IsNull(message);
        }

        [Test]
        public void ShouldNotReturnNullMessageIfSignifcantChangeExists()
        {
            ScheduleDayReadModel newReadModel = new ScheduleDayReadModel();
            newReadModel.StartDateTime = new DateTime();
            newReadModel.EndDateTime = new DateTime();
            newReadModel.Workday = true;

            ScheduleDayReadModel existingReadModel = new ScheduleDayReadModel();
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
            ScheduleDayReadModel newReadModel = new ScheduleDayReadModel();
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
            ScheduleDayReadModel existingReadModel = new ScheduleDayReadModel();
            existingReadModel.StartDateTime = new DateTime(2012, 01, 01);
            existingReadModel.EndDateTime = new DateTime(2012, 01, 01);
            existingReadModel.Workday = true;

            var message = _scheduleDayReadModelComparer.FindSignificantChanges(null, existingReadModel,
                                                                              _person.PermissionInformation.Culture(),
                                                                              new DateOnly(2012, 01, 01));
            Assert.IsNotNull(message);
        }

        [Test]
        public void ShouldNotDetectFromNullToNoWorkDay()
        {
            ScheduleDayReadModel newReadModel = new ScheduleDayReadModel();
            newReadModel.StartDateTime = new DateTime();
            newReadModel.EndDateTime = new DateTime();
            newReadModel.Workday = false;

            var message = _scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, null,
                                                                              _person.PermissionInformation.Culture(),
                                                                              new DateOnly(DateTime.Now.Date));
            Assert.IsNull(message);
        }

        [Test]
        public void ShouldNotDetectFromNoWorkDayToNull()
        {
            ScheduleDayReadModel existingReadModel = new ScheduleDayReadModel();
            existingReadModel.StartDateTime = new DateTime();
            existingReadModel.EndDateTime = new DateTime();
            existingReadModel.Workday = false;

            var message = _scheduleDayReadModelComparer.FindSignificantChanges(null, existingReadModel,
                                                                              _person.PermissionInformation.Culture(),
                                                                              new DateOnly(DateTime.Now.Date));
            Assert.IsNull(message);
        }

        [Test]
        public void ShouldDetectIfWorkDayIsChangedToNoWorkDay()
        {
            ScheduleDayReadModel newReadModel = new ScheduleDayReadModel();
            newReadModel.StartDateTime = new DateTime();
            newReadModel.EndDateTime = new DateTime();
            newReadModel.Workday = false;

            ScheduleDayReadModel existingReadModel = new ScheduleDayReadModel();
            existingReadModel.StartDateTime = new DateTime();
            existingReadModel.EndDateTime = new DateTime();
            existingReadModel.Workday = true;

            var message = _scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, existingReadModel,
                                                                              _person.PermissionInformation.Culture(),
                                                                              new DateOnly(DateTime.Now.Date));
            Assert.IsNotNull(message);
        }

        [Test]
        public void ShouldDetectIfNoWorkDayIsChangedToWorkDay()
        {
            ScheduleDayReadModel newReadModel = new ScheduleDayReadModel();
            newReadModel.StartDateTime = new DateTime();
            newReadModel.EndDateTime = new DateTime();
            newReadModel.Workday = true;

            ScheduleDayReadModel existingReadModel = new ScheduleDayReadModel();
            existingReadModel.StartDateTime = new DateTime();
            existingReadModel.EndDateTime = new DateTime();
            existingReadModel.Workday = false;

            var message = _scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, existingReadModel,
                                                                              _person.PermissionInformation.Culture(),
                                                                              new DateOnly(DateTime.Now.Date));
            Assert.IsNotNull(message);
        }
    }
}
