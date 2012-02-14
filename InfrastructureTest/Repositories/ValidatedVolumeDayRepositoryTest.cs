﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Rhino.Mocks;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{   
    /// <summary>
    /// Tests for ValidatedVolumeDayRepository
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class ValidatedVolumeDayRepositoryTest : RepositoryTest<IValidatedVolumeDay>
    {
        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
        }

        /// <summary>
        /// Creates an aggreagte using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override IValidatedVolumeDay CreateAggregateWithCorrectBusinessUnit()
        {
            ISkillType skillType = SkillTypeFactory.CreateSkillType();
            ISkill skill = SkillFactory.CreateSkill("dummy", skillType, 15);
            IActivity activity = ActivityFactory.CreateActivity("dummyActivity");
            IGroupingActivity groupingActivity = GroupingActivityFactory.CreateSimpleGroupingActivity("ga1");
            skill.Activity = activity;

            PersistAndRemoveFromUnitOfWork(skillType);
            PersistAndRemoveFromUnitOfWork(groupingActivity);

            groupingActivity.AddActivity(activity);

            PersistAndRemoveFromUnitOfWork(activity);
            PersistAndRemoveFromUnitOfWork(skill);

            IWorkload workload = WorkloadFactory.CreateWorkload(skill);
            PersistAndRemoveFromUnitOfWork(workload);

            DateOnly dateTime = new DateOnly(2008,7,16);
            ValidatedVolumeDay validatedVolumeDay = new ValidatedVolumeDay(workload, dateTime);

            return validatedVolumeDay;
        }


        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(IValidatedVolumeDay loadedAggregateFromDatabase)
        {
            IValidatedVolumeDay validatedVolumeDay = CreateAggregateWithCorrectBusinessUnit();
            Assert.AreEqual(validatedVolumeDay.VolumeDayDate, loadedAggregateFromDatabase.VolumeDayDate);
            Assert.AreEqual(validatedVolumeDay.Workload.Name, loadedAggregateFromDatabase.Workload.Name);
        }

        /// <summary>
        /// Tests the repository.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-28
        /// </remarks>
        protected override Repository<IValidatedVolumeDay> TestRepository(IUnitOfWork unitOfWork)
        {
            return new ValidatedVolumeDayRepository(unitOfWork);
        }

        /// <summary>
        /// Determines whether this instance [can find validated volume days].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-28
        /// </remarks>
        [Test]
        public void CanFindValidatedVolumeDays()
        {
            IValidatedVolumeDay validatedVolumeDay = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(validatedVolumeDay);

            IValidatedVolumeDayRepository validatedVolumeDayRepository = new ValidatedVolumeDayRepository(UnitOfWork);
            ICollection<IValidatedVolumeDay> validatedVolumeDays = validatedVolumeDayRepository.FindRange(new DateOnlyPeriod(validatedVolumeDay.VolumeDayDate, validatedVolumeDay.VolumeDayDate.AddDays(1)),
                validatedVolumeDay.Workload);

            Assert.AreEqual(1, validatedVolumeDays.Count);
        }

        /// <summary>
        /// Determines whether this instance [can match task owner days with validated volume days].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-28
        /// </remarks>
        [Test]
        public void CanMatchTaskOwnerDaysWithValidatedVolumeDays()
        {
            IValidatedVolumeDay validatedVolumeDay1 = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(validatedVolumeDay1);
            IValidatedVolumeDay validatedVolumeDay2 = new ValidatedVolumeDay(
                validatedVolumeDay1.Workload,
                validatedVolumeDay1.VolumeDayDate.AddDays(1));
            PersistAndRemoveFromUnitOfWork(validatedVolumeDay2);

            DateOnlyPeriod period = new DateOnlyPeriod(validatedVolumeDay1.VolumeDayDate, validatedVolumeDay1.VolumeDayDate.AddDays(3));

            ITaskOwner taskOwnerDay1 = Mocks.StrictMock<ITaskOwner>();
            ITaskOwner taskOwnerDay2 = Mocks.StrictMock<ITaskOwner>();
            ITaskOwner taskOwnerDay3 = Mocks.StrictMock<ITaskOwner>();

            Expect.Call(taskOwnerDay1.CurrentDate).Return(validatedVolumeDay1.VolumeDayDate).Repeat.AtLeastOnce();
            Expect.Call(taskOwnerDay2.CurrentDate).Return(validatedVolumeDay2.VolumeDayDate).Repeat.AtLeastOnce();
            Expect.Call(taskOwnerDay3.CurrentDate).Return(validatedVolumeDay2.VolumeDayDate.AddDays(1)).Repeat.AtLeastOnce();
            IList<ITaskOwner> taskOwnerList = new List<ITaskOwner> { taskOwnerDay1, taskOwnerDay2, taskOwnerDay3 };

            Mocks.ReplayAll();

            IValidatedVolumeDayRepository repository = new ValidatedVolumeDayRepository(UnitOfWork);
            ICollection<IValidatedVolumeDay> existingValidatedVolumeDays = repository.FindRange(period, validatedVolumeDay1.Workload);
            Assert.AreEqual(2, existingValidatedVolumeDays.Count);
            ICollection<ITaskOwner> validatedVolumeDays = repository.MatchDays(validatedVolumeDay1.Workload, taskOwnerList, existingValidatedVolumeDays);
            Assert.AreEqual(3, validatedVolumeDays.Count);
            Assert.AreEqual(taskOwnerDay1, ((ValidatedVolumeDay)validatedVolumeDays.ElementAt(0)).TaskOwner);
            Assert.AreEqual(taskOwnerDay2, ((ValidatedVolumeDay)validatedVolumeDays.ElementAt(1)).TaskOwner);
            Assert.AreEqual(taskOwnerDay3, ((ValidatedVolumeDay)validatedVolumeDays.ElementAt(2)).TaskOwner);
            Assert.IsNotNull(((ValidatedVolumeDay)validatedVolumeDays.ElementAt(0)).Id);
            Assert.IsNotNull(((ValidatedVolumeDay)validatedVolumeDays.ElementAt(1)).Id);
            Assert.IsNotNull(((ValidatedVolumeDay)validatedVolumeDays.ElementAt(2)).Id);

            Mocks.VerifyAll();
        }

        /// <summary>
        /// Determines whether this instance [can match task owner days with validated volume days without adding to repository].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-21
        /// </remarks>
        [Test]
        public void CanMatchTaskOwnerDaysWithValidatedVolumeDaysWithoutAddingToRepository()
        {
            IValidatedVolumeDay validatedVolumeDay1 = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(validatedVolumeDay1);
            IValidatedVolumeDay validatedVolumeDay2 = new ValidatedVolumeDay(
                validatedVolumeDay1.Workload,
                validatedVolumeDay1.VolumeDayDate.AddDays(1));
            PersistAndRemoveFromUnitOfWork(validatedVolumeDay2);

            DateOnlyPeriod period = new DateOnlyPeriod(validatedVolumeDay1.VolumeDayDate, validatedVolumeDay1.VolumeDayDate.AddDays(3));

            ITaskOwner taskOwnerDay1 = Mocks.StrictMock<ITaskOwner>();
            ITaskOwner taskOwnerDay2 = Mocks.StrictMock<ITaskOwner>();
            ITaskOwner taskOwnerDay3 = Mocks.StrictMock<ITaskOwner>();

            Expect.Call(taskOwnerDay1.CurrentDate).Return(validatedVolumeDay1.VolumeDayDate).Repeat.AtLeastOnce();
            Expect.Call(taskOwnerDay2.CurrentDate).Return(validatedVolumeDay2.VolumeDayDate).Repeat.AtLeastOnce();
            Expect.Call(taskOwnerDay3.CurrentDate).Return(validatedVolumeDay2.VolumeDayDate.AddDays(1)).Repeat.AtLeastOnce();
            IList<ITaskOwner> taskOwnerList = new List<ITaskOwner> { taskOwnerDay1, taskOwnerDay2, taskOwnerDay3 };

            Mocks.ReplayAll();

            IValidatedVolumeDayRepository repository = new ValidatedVolumeDayRepository(UnitOfWork);
            ICollection<IValidatedVolumeDay> existingValidatedVolumeDays = repository.FindRange(period, validatedVolumeDay1.Workload);
            Assert.AreEqual(2, existingValidatedVolumeDays.Count);
            ICollection<ITaskOwner> validatedVolumeDays = repository.MatchDays(validatedVolumeDay1.Workload, taskOwnerList, existingValidatedVolumeDays,false);
            Assert.AreEqual(3, validatedVolumeDays.Count);
            Assert.AreEqual(taskOwnerDay1, ((ValidatedVolumeDay)validatedVolumeDays.ElementAt(0)).TaskOwner);
            Assert.AreEqual(taskOwnerDay2, ((ValidatedVolumeDay)validatedVolumeDays.ElementAt(1)).TaskOwner);
            Assert.AreEqual(taskOwnerDay3, ((ValidatedVolumeDay)validatedVolumeDays.ElementAt(2)).TaskOwner);
            Assert.IsNotNull(((ValidatedVolumeDay)validatedVolumeDays.ElementAt(0)).Id);
            Assert.IsNotNull(((ValidatedVolumeDay)validatedVolumeDays.ElementAt(1)).Id);
            Assert.IsNull(((ValidatedVolumeDay)validatedVolumeDays.ElementAt(2)).Id);

            Mocks.VerifyAll();
        }

        /// <summary>
        /// Verifies the get latest validated date works.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-28
        /// </remarks>
        [Test]
        public void VerifyGetLatestValidatedDateWorks()
        {
            IValidatedVolumeDay validatedVolumeDay1 = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(validatedVolumeDay1);
            IValidatedVolumeDay validatedVolumeDay2 = new ValidatedVolumeDay(
                validatedVolumeDay1.Workload,
                validatedVolumeDay1.VolumeDayDate.AddDays(-1));
            PersistAndRemoveFromUnitOfWork(validatedVolumeDay2);

            IValidatedVolumeDayRepository validatedVolumeDayRepository = new ValidatedVolumeDayRepository(UnitOfWork);
            DateOnly latestValidated = validatedVolumeDayRepository.FindLastValidatedDay(validatedVolumeDay1.Workload);

            var timeZone = validatedVolumeDay1.Workload.Skill.TimeZone;
            Assert.AreEqual(new DateOnly(timeZone.ConvertTimeFromUtc(validatedVolumeDay1.VolumeDayDate,timeZone)), latestValidated);
        }

        /// <summary>
        /// Verifies the get latest validated date works without persistent data.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-28
        /// </remarks>
        [Test]
        public void VerifyGetLatestValidatedDateWorksWithoutPersistentData()
        {
            IValidatedVolumeDay validatedVolumeDay = CreateAggregateWithCorrectBusinessUnit();
            //No persisting here! Just get the day to create skill/workload!

            IValidatedVolumeDayRepository validatedVolumeDayRepository = new ValidatedVolumeDayRepository(UnitOfWork);
            DateOnly latestValidated = validatedVolumeDayRepository.FindLastValidatedDay(validatedVolumeDay.Workload);

            Assert.AreEqual(latestValidated, new DateOnly(DateTime.Today.AddMonths(-1)));
        }

        [Test]
        public void CanCancelMatchDaysOperation()
        {
            IValidatedVolumeDay validatedVolumeDay1 = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(validatedVolumeDay1);
            IValidatedVolumeDay validatedVolumeDay2 = new ValidatedVolumeDay(
                validatedVolumeDay1.Workload,
                validatedVolumeDay1.VolumeDayDate.AddDays(1));
            PersistAndRemoveFromUnitOfWork(validatedVolumeDay2);

            DateOnlyPeriod period = new DateOnlyPeriod(validatedVolumeDay1.VolumeDayDate, validatedVolumeDay1.VolumeDayDate.AddDays(3));

            ITaskOwner taskOwnerDay1 = Mocks.StrictMock<ITaskOwner>();
            ITaskOwner taskOwnerDay2 = Mocks.StrictMock<ITaskOwner>();
            ITaskOwner taskOwnerDay3 = Mocks.StrictMock<ITaskOwner>();

            Expect.Call(taskOwnerDay1.CurrentDate).Return(validatedVolumeDay1.VolumeDayDate).Repeat.AtLeastOnce();
            Expect.Call(taskOwnerDay2.CurrentDate).Return(validatedVolumeDay2.VolumeDayDate).Repeat.AtLeastOnce();
            Expect.Call(taskOwnerDay3.CurrentDate).Return(validatedVolumeDay2.VolumeDayDate.AddDays(1)).Repeat.AtLeastOnce();
            IList<ITaskOwner> taskOwnerList = new List<ITaskOwner> { taskOwnerDay1, taskOwnerDay2, taskOwnerDay3 };

            Mocks.ReplayAll();

            IValidatedVolumeDayRepository repository = new ValidatedVolumeDayRepository(UnitOfWork);
            ICollection<IValidatedVolumeDay> existingValidatedVolumeDays = repository.FindRange(period, validatedVolumeDay1.Workload);
            Assert.AreEqual(2, existingValidatedVolumeDays.Count);
            
            //Cancel the matchdays operation (before, but anyway)
            repository.CancelMatchDays();
            ICollection<ITaskOwner> validatedVolumeDays = repository.MatchDays(validatedVolumeDay1.Workload, taskOwnerList, existingValidatedVolumeDays, false);

            Assert.AreEqual(null, validatedVolumeDays);
        }

        [Test]
        public void VerifyGetLastModifiedValidatedDay()
        {
            IValidatedVolumeDay validatedVolumeDay1 = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(validatedVolumeDay1);
            IValidatedVolumeDay validatedVolumeDay2 = new ValidatedVolumeDay(
                validatedVolumeDay1.Workload,
                validatedVolumeDay1.VolumeDayDate.AddDays(-1));
            Thread.Sleep(1000);
            PersistAndRemoveFromUnitOfWork(validatedVolumeDay2);

            IValidatedVolumeDayRepository validatedVolumeDayRepository = new ValidatedVolumeDayRepository(UnitOfWork);
            IValidatedVolumeDay result = validatedVolumeDayRepository.FindLatestUpdated(validatedVolumeDay1.Workload.Skill);

            Assert.AreEqual(validatedVolumeDay2, result);

            validatedVolumeDay1.AddAbsoluteValueToTasks(10);
            Thread.Sleep(1000);
            PersistAndRemoveFromUnitOfWork(validatedVolumeDay1);

            result = validatedVolumeDayRepository.FindLatestUpdated(validatedVolumeDay1.Workload.Skill);

            Assert.AreEqual(validatedVolumeDay1, result);
        }
    }
}
