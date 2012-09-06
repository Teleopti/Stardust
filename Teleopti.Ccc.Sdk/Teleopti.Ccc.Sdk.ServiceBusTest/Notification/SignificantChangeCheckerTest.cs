﻿using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Notification
{
	[TestFixture]
	public class SignificantChangeCheckerTest
	{
		private MockRepository _mocks;
		private SignificantChangeChecker _target;
		private IPerson _person;
		private IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private IScheduleDayReadModelComparer _scheduleDayReadModelComparer;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleDayReadModelRepository = _mocks.StrictMock<IScheduleDayReadModelRepository>();
			_scheduleDayReadModelComparer = _mocks.StrictMock<IScheduleDayReadModelComparer>();
			_target = new SignificantChangeChecker(_scheduleDayReadModelRepository, _scheduleDayReadModelComparer);
			_person = PersonFactory.CreatePerson("test");
            _person.SetId(Guid.NewGuid());
		}

		[Test]
		public void ShouldReturnFalseIfPeriodAfterWithinFourteenDays()
		{
			var date = DateTime.Now.Date;
			var period = new DateOnlyPeriod(new DateOnly(date.AddDays(20)), new DateOnly(date.AddDays(30)) );

			Assert.That(_target.SignificantChangeNotificationMessage(period, _person, new List<ScheduleDayReadModel>()).Subject, Is.EqualTo(""));
		}

		[Test]
		public void ShouldReturnFalseIfPeriodBeforeWithinFourteenDays()
		{
			var date = DateTime.Now.Date;
			var period = new DateOnlyPeriod(new DateOnly(date.AddDays(-20)), new DateOnly(date.AddDays(-1)));

			Assert.That(_target.SignificantChangeNotificationMessage(period, _person, new List<ScheduleDayReadModel>()).Subject, Is.Empty);
		}

        [Test]
        public void ShouldReturnMessageIfChangeIsWithinFourteenDays()
        {
            var date = DateTime.Now.Date;
            var period = new DateOnlyPeriod(new DateOnly(date), new DateOnly(date.AddDays(2)));
            
            IList<ScheduleDayReadModel> newReadModelList = new List<ScheduleDayReadModel>();
            var newReadModel = new ScheduleDayReadModel();
            newReadModel.StartDateTime = date.AddDays(1).AddHours(2);
            newReadModel.EndDateTime = date.AddDays(1).AddHours(4);
            newReadModel.PersonId = _person.Id.GetValueOrDefault();
            newReadModel.Workday = true;
            newReadModel.Date = date.AddDays(1).Date;
            newReadModelList.Add(newReadModel);
            
            IList<ScheduleDayReadModel> oldReadModelList = new List<ScheduleDayReadModel>();
            var oldReadModel = new ScheduleDayReadModel();
            oldReadModel.StartDateTime = date.AddDays(1).AddHours(1);
            oldReadModel.EndDateTime = date.AddDays(1).AddHours(3);
            oldReadModel.PersonId = _person.Id.GetValueOrDefault();
            oldReadModel.Workday = true;
            oldReadModel.Date = date.AddDays(1).Date;
            oldReadModelList.Add(oldReadModel);

            const string message = "test message ";

            Expect.Call(_scheduleDayReadModelRepository.ReadModelsOnPerson(new DateOnly(date), new DateOnly(date.AddDays(2)),
                                                                           _person.Id.GetValueOrDefault())).Return
                                                                           (oldReadModelList);

            Expect.Call(_scheduleDayReadModelComparer.FindSignificantChanges(null, null,
                                                                             _person.PermissionInformation.UICulture(),
                                                                             new DateOnly(date))).Return(message);

            Expect.Call(_scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, oldReadModel,
                                                                             _person.PermissionInformation.UICulture(),
                                                                             new DateOnly(date.AddDays(1)))).Return(message);

            Expect.Call(_scheduleDayReadModelComparer.FindSignificantChanges(null, null,
                                                                             _person.PermissionInformation.UICulture(),
                                                                             new DateOnly(date.AddDays(2)))).Return(message);
            _mocks.ReplayAll();

            Assert.That(_target.SignificantChangeNotificationMessage(period, _person, newReadModelList).Subject, Is.Not.Empty);
            _mocks.VerifyAll();
        }

        [Test]
		public void ShouldReturnNullIfNoOverlappingPeriod()
		{
            var date = DateTime.Now.Date;
            var period = new DateOnlyPeriod(new DateOnly(date.AddDays(15)), new DateOnly(date.AddDays(20)));
            
            Assert.That(_target.SignificantChangeNotificationMessage(period, _person, null).Subject, Is.Empty);
       }

        [Test]
        public void ShouldReturnMessageIfDayNotExistInOverlappingPeriod()
        {
            var date = DateTime.Now.Date;
            var period = new DateOnlyPeriod(new DateOnly(date.AddDays(1)), new DateOnly(date.AddDays(1)));

            IList<ScheduleDayReadModel> newReadModelList = new List<ScheduleDayReadModel>();
            var newReadModel = new ScheduleDayReadModel();
            newReadModel.StartDateTime = date.AddDays(-1).AddHours(2);
            newReadModel.EndDateTime = date.AddDays(-1).AddHours(4);
            newReadModel.PersonId = _person.Id.GetValueOrDefault();
            newReadModel.Workday = true;
            newReadModel.Date = date.AddDays(-1).Date;
            newReadModelList.Add(newReadModel);

            IList<ScheduleDayReadModel> oldReadModelList = new List<ScheduleDayReadModel>();
            var oldReadModel = new ScheduleDayReadModel();
            oldReadModel.StartDateTime = date.AddDays(1).AddHours(1);
            oldReadModel.EndDateTime = date.AddDays(1).AddHours(3);
            oldReadModel.PersonId = _person.Id.GetValueOrDefault();
            oldReadModel.Workday = true;
            oldReadModel.Date = date.AddDays(1).Date;
            oldReadModelList.Add(oldReadModel);

            const string message = "test message ";

            Expect.Call(_scheduleDayReadModelRepository.ReadModelsOnPerson(new DateOnly(date.AddDays(1)), new DateOnly(date.AddDays(1)),
                                                                           _person.Id.GetValueOrDefault())).Return
                                                                           (oldReadModelList);

            Expect.Call(_scheduleDayReadModelComparer.FindSignificantChanges(null, oldReadModel,
                                                                             _person.PermissionInformation.UICulture(),
                                                                             new DateOnly(date.AddDays(1)))).Return(message);
            _mocks.ReplayAll();

            Assert.That(_target.SignificantChangeNotificationMessage(period, _person, newReadModelList).Subject, Is.Not.Empty);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnNotificationIfDayNotExistIncurrentReadModel()
        {
            var date = DateTime.Now.Date;
            var period = new DateOnlyPeriod(new DateOnly(date.AddDays(1)), new DateOnly(date.AddDays(1)));

            IList<ScheduleDayReadModel> newReadModelList = new List<ScheduleDayReadModel>();
            var newReadModel = new ScheduleDayReadModel();
            newReadModel.StartDateTime = date.AddDays(1).AddHours(2);
            newReadModel.EndDateTime = date.AddDays(1).AddHours(4);
            newReadModel.PersonId = _person.Id.GetValueOrDefault();
            newReadModel.Workday = true;
            newReadModel.Date = date.AddDays(1).Date;
            newReadModelList.Add(newReadModel);

            IList<ScheduleDayReadModel> oldReadModelList = new List<ScheduleDayReadModel>();
            var oldReadModel = new ScheduleDayReadModel();
            oldReadModel.StartDateTime = date.AddDays(-1).AddHours(1);
            oldReadModel.EndDateTime = date.AddDays(-1).AddHours(3);
            oldReadModel.PersonId = _person.Id.GetValueOrDefault();
            oldReadModel.Workday = true;
            oldReadModel.Date = date.AddDays(-1).Date;
            oldReadModelList.Add(oldReadModel);

            const string message = "test message ";

            Expect.Call(_scheduleDayReadModelRepository.ReadModelsOnPerson(new DateOnly(date.AddDays(1)), new DateOnly(date.AddDays(1)),
                                                                           _person.Id.GetValueOrDefault())).Return
                                                                           (oldReadModelList);

            Expect.Call(_scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, null,
                                                                             _person.PermissionInformation.UICulture(),
                                                                             new DateOnly(date.AddDays(1)))).Return(message);
            _mocks.ReplayAll();

            Assert.That(_target.SignificantChangeNotificationMessage(period, _person, newReadModelList).Subject, Is.Not.Empty);
            _mocks.VerifyAll();

        }


	}

}