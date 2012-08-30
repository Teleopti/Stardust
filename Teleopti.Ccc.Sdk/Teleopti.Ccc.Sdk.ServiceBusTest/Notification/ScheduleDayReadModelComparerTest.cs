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
        private MockRepository _mocks;
        private IPerson _person;
        private ScheduleDayReadModelComparer scheduleDayReadModelComparer;

        [SetUp]
        public void Setup()
        {
           
            _person = PersonFactory.CreatePerson("test");
            _person.PermissionInformation.SetCulture(CultureInfo.CurrentCulture);
            scheduleDayReadModelComparer = new ScheduleDayReadModelComparer();
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

            var message = scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, existingReadModel,
                                                                              _person.PermissionInformation.Culture());
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

            var message = scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, existingReadModel,
                                                                              _person.PermissionInformation.Culture());
            Assert.IsNotNull(message);

        }

    }
}
