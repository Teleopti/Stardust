﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class PersistConflictTest
    {
        private IPersistConflictView view;
        private MockRepository mocks;
        private PersistConflictPresenter target;
        private PersistConflictModel model;
        private IList<IPersistConflict> conflicts;
        private IScheduleDictionary schedDic;
        private IList<IEventMessage> eventMessages;
        private IEventMessage eventMessage;
        private IList<IPersistableScheduleData> modifiedData;

        [SetUp]
        public void Setup()
        {
            modifiedData = new List<IPersistableScheduleData>();
            eventMessages = new List<IEventMessage>();
            mocks=new MockRepository();
            eventMessage = mocks.StrictMock<IEventMessage>();
            schedDic = mocks.DynamicMock<IScheduleDictionary>();
            view = mocks.StrictMock<IPersistConflictView>();
            conflicts = new List<IPersistConflict>();
            model = new PersistConflictModel(schedDic, conflicts, modifiedData);
            target = new PersistConflictPresenter(view, model);
            eventMessages.Add(eventMessage);
        }

        [Test]
        public void VerifyInitializeWhenModifiedOnClient()
        {
            var assPer = new Person {Name = new Name("roger", "moore")};
            var changedByPer = new Person {Name = new Name("hubba", "bubba")};

            var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Scenario("sdf"), assPer, new DateTimePeriod(2000,1,1,2000,1,2));
            ReflectionHelper.SetUpdatedBy(ass, changedByPer);
            conflicts.Add(new PersistConflictMessageState(new DifferenceCollectionItem<IPersistableScheduleData>(ass, ass.EntityClone()), ass.EntityClone(), eventMessage, removeMessage));

            using(mocks.Record())
            {
                view.SetupGridControl(model.Data);
            }
            using(mocks.Playback())
            {
                target.Initialize();            
            }
            Assert.AreEqual(1, model.Data.Count());
            var oneModelRow = model.Data.First();
            Assert.AreEqual("roger moore", oneModelRow.Name);
            Assert.AreEqual(new DateOnly(2000, 1, 1), oneModelRow.Date);
            Assert.AreEqual(UserTexts.Resources.Shift, oneModelRow.ConflictType);
            Assert.AreEqual("hubba bubba", oneModelRow.LastModifiedName);
        }

        [Test]
        public void VerifyInitializeWhenDeletedOnClient()
        {
            var assPer = new Person { Name = new Name("roger", "moore") };
            var changedByPer = new Person { Name = new Name("hubba", "bubba") };

						var ass = PersonAssignmentFactory.CreateAssignmentWithDayOff(new Scenario("ffsdf"), assPer, new DateOnly(2000, 1, 2), new DayOffTemplate(new Description()));
            ReflectionHelper.SetUpdatedBy(ass, changedByPer);
            conflicts.Add(new PersistConflictMessageState(new DifferenceCollectionItem<IPersistableScheduleData>(ass, null), ass.EntityClone(), eventMessage, removeMessage));

            using (mocks.Record())
            {
                view.SetupGridControl(model.Data);
            }
            using (mocks.Playback())
            {
                target.Initialize();
            }

            Assert.AreEqual(1, model.Data.Count());
            var oneModelRow = model.Data.First();
            Assert.AreEqual("roger moore", oneModelRow.Name);
            Assert.AreEqual(new DateOnly(2000, 1, 1), oneModelRow.Date);
            Assert.AreEqual(UserTexts.Resources.Shift, oneModelRow.ConflictType);
            Assert.AreEqual("hubba bubba", oneModelRow.LastModifiedName);
        }

        [Test]
        public void VerifyInitializeWhenDeletedInDatabase()
        {
            var assPer = new Person { Name = new Name("roger", "moore") };
            var changedLocallyBy = new Person { Name = new Name("hubba", "bubba") };

            var ass = PersonAbsenceFactory.CreatePersonAbsence(assPer, new Scenario("sdf"), new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
            ReflectionHelper.SetUpdatedBy(ass, changedLocallyBy);
            conflicts.Add(new PersistConflictMessageState(new DifferenceCollectionItem<IPersistableScheduleData>(ass, ass.EntityClone()), null, eventMessage, removeMessage)); ;

            using (mocks.Record())
            {
                view.SetupGridControl(model.Data);
            }
            using (mocks.Playback())
            {
                target.Initialize();
            }

            Assert.AreEqual(1, model.Data.Count());
            var oneModelRow = model.Data.First();
            Assert.AreEqual("roger moore", oneModelRow.Name);
            Assert.AreEqual(new DateOnly(2000, 1, 1), oneModelRow.Date);
            Assert.AreEqual(UserTexts.Resources.Absence, oneModelRow.ConflictType);
            Assert.AreEqual(UserTexts.Resources.Deleted, oneModelRow.LastModifiedName);
        }

        [Test]
        public void VerifyInitializeWhenStrangeType()
        {
            conflicts.Add(new PersistConflictMessageState(new DifferenceCollectionItem<IPersistableScheduleData>(new dummyScheduleData(), null), null, eventMessage, removeMessage)); ;
            using (mocks.Record())
            {
                view.SetupGridControl(model.Data);
            }
            using (mocks.Playback())
            {
                target.Initialize();
            }
            Assert.AreEqual(1, model.Data.Count());
            var oneModelRow = model.Data.First();
            Assert.AreEqual("Arne weise", oneModelRow.Name);
            Assert.AreEqual(new DateOnly(2000, 1, 1), oneModelRow.Date);
			Assert.AreEqual(UserTexts.Resources.Unknown + "(" + typeof(dummyScheduleData) + ")", oneModelRow.ConflictType);
            Assert.AreEqual(UserTexts.Resources.Deleted, oneModelRow.LastModifiedName);
        }

        [Test]
        public void VerifyInitializeWhenNoteType()
        {
            var per = new Person { Name = new Name("roger", "moore") };
            var changedLocallyBy = new Person { Name = new Name("hubba", "bubba") };
            var note = new Note(per, new DateOnly(2010, 4, 27), new Scenario("scenario"), "note");

            ReflectionHelper.SetUpdatedBy(note, changedLocallyBy);

            conflicts.Add(new PersistConflictMessageState(new DifferenceCollectionItem<IPersistableScheduleData>(note, null), null, eventMessage, removeMessage));
              
            using (mocks.Record())
            {
                view.SetupGridControl(model.Data);
            }
            using (mocks.Playback())
            {
                target.Initialize();
            }
            Assert.AreEqual(1, model.Data.Count());
            var oneModelRow = model.Data.First();
            Assert.AreEqual("roger moore", oneModelRow.Name);
            Assert.AreEqual(UserTexts.Resources.Note, oneModelRow.ConflictType);
            Assert.AreEqual(UserTexts.Resources.Deleted, oneModelRow.LastModifiedName);
        }

        [Test]
        public void ShouldInitializeWithStudentAvailabilityType()
        {
            var per = new Person { Name = new Name("roger", "moore") };
            var changedLocallyBy = new Person { Name = new Name("hubba", "bubba") };
            var note = new StudentAvailabilityDay(per, new DateOnly(2010, 4, 27), new List<IStudentAvailabilityRestriction>());

            ReflectionHelper.SetUpdatedBy(note, changedLocallyBy);

            conflicts.Add(new PersistConflictMessageState(new DifferenceCollectionItem<IPersistableScheduleData>(note, null), null, eventMessage, removeMessage));

            using (mocks.Record())
            {
                view.SetupGridControl(model.Data);
            }
            using (mocks.Playback())
            {
                target.Initialize();
            }
            Assert.AreEqual(1, model.Data.Count());
            var oneModelRow = model.Data.First();
            Assert.AreEqual("roger moore", oneModelRow.Name);
            Assert.AreEqual(UserTexts.Resources.StudentAvailability, oneModelRow.ConflictType);
            Assert.AreEqual(UserTexts.Resources.Deleted, oneModelRow.LastModifiedName);
        }

        [Test]
        public void VerifyUndoClientChangesWhenModified()
        {
            var pDayOff = PersonDayOffFactory.CreatePersonDayOff();
            var dbData = pDayOff.EntityClone();
            conflicts.Add(new PersistConflictMessageState(new DifferenceCollectionItem<IPersistableScheduleData>(pDayOff, pDayOff.EntityClone()), dbData, eventMessage, removeMessage));
            ScheduleRange range = mocks.PartialMock<ScheduleRange>(schedDic, pDayOff);

            using(mocks.Record())
            {
                Expect.Call(schedDic[pDayOff.Person]).Return(range);
                range.UnsafeSnapshotUpdate(conflicts.First().DatabaseVersion, true);
                forgetAboutDebugAssertsOnMock(0);
                view.CloseForm(true);
            }
            using(mocks.Playback())
            {
                target.OnUndoClientChanges();
            }
            Assert.AreEqual(0, eventMessages.Count);
            Assert.AreEqual(2, modifiedData.Count);
            CollectionAssert.Contains(modifiedData, pDayOff);
            CollectionAssert.Contains(modifiedData, dbData);
        }

        [Test]
        public void VerifyUndoClientChangesWhenDeletedInDatabase()
        {
            var pDayOff = PersonDayOffFactory.CreatePersonDayOff();
            pDayOff.SetId(Guid.NewGuid());
            conflicts.Add(new PersistConflictMessageState(new DifferenceCollectionItem<IPersistableScheduleData>(pDayOff, pDayOff.EntityClone()), null, eventMessage, removeMessage));
            ScheduleRange range = mocks.PartialMock<ScheduleRange>(schedDic, pDayOff);
            
            using (mocks.Record())
            {
                Expect.Call(schedDic[pDayOff.Person]).Return(range);
                range.UnsafeSnapshotDelete(conflicts.First().ClientVersion.OriginalItem.Id.Value, true);
                forgetAboutDebugAssertsOnMock(0);
                view.CloseForm(true);
            }
            using (mocks.Playback())
            {
                target.OnUndoClientChanges();
            }
            Assert.AreEqual(0, eventMessages.Count);
            Assert.AreEqual(1, modifiedData.Count);
            CollectionAssert.Contains(modifiedData, pDayOff);
        }

        [Test]
        public void VerifyOverwriteServerChangesWhenModified()
        {
            var dataOrg = (PersonDayOff)PersonDayOffFactory.CreatePersonDayOff();
            var dataCurrent = (PersonDayOff)dataOrg.EntityClone();
            var dataDb = dataCurrent.EntityClone();

            conflicts.Add(new PersistConflictMessageState(new DifferenceCollectionItem<IPersistableScheduleData>(dataOrg, dataCurrent), dataDb, eventMessage, removeMessage));
            ScheduleRange range = mocks.PartialMock<ScheduleRange>(schedDic, dataOrg);

            using (mocks.Record())
            {
                Expect.Call(schedDic[dataOrg.Person]).Return(range);
                range.UnsafeSnapshotUpdate(dataDb, false);
                forgetAboutDebugAssertsOnMock(1);
                view.CloseForm(true);
            }
            using (mocks.Playback())
            {
                target.OnOverwriteServerChanges();
            }
            Assert.AreEqual(0, eventMessages.Count);
            Assert.AreEqual(0, modifiedData.Count);
        }

        [Test]
        public void VerifyOverwriteServerChangesWhenDeletedOnClient()
        {
            var dataOrg = (PersonDayOff)PersonDayOffFactory.CreatePersonDayOff();
            var dataDb = dataOrg.EntityClone();

            conflicts.Add(new PersistConflictMessageState(new DifferenceCollectionItem<IPersistableScheduleData>(dataOrg, null), dataDb, eventMessage, removeMessage));
            ScheduleRange range = mocks.PartialMock<ScheduleRange>(schedDic, dataOrg);

            using (mocks.Record())
            {
                Expect.Call(schedDic[dataOrg.Person]).Return(range);
                range.UnsafeSnapshotUpdate(dataDb, false);
                forgetAboutDebugAssertsOnMock(1);
                view.CloseForm(true);
            }
            using (mocks.Playback())
            {
                target.OnOverwriteServerChanges();
            }
            Assert.AreEqual(0, eventMessages.Count);
            Assert.AreEqual(0, eventMessages.Count);
            Assert.AreEqual(0, modifiedData.Count);
        }

        [Test]
        public void VerifyOverwriteServerChangesWhenDeletedOnDatabase()
        {
            var dataOrg = (PersonDayOff)PersonDayOffFactory.CreatePersonDayOff();
            ((IEntity)dataOrg).SetId(Guid.NewGuid());
            var dataCurrent = (PersonDayOff)dataOrg.EntityClone();

            conflicts.Add(new PersistConflictMessageState(new DifferenceCollectionItem<IPersistableScheduleData>(dataOrg, dataCurrent), null, eventMessage, removeMessage));
            ScheduleRange range = mocks.PartialMock<ScheduleRange>(schedDic, dataOrg);

            using (mocks.Record())
            {
                Expect.Call(schedDic[dataOrg.Person]).Return(range);
                range.UnsafeSnapshotDelete(dataOrg.Id.Value, false);
                forgetAboutDebugAssertsOnMock(1);
                view.CloseForm(true);
            }
            using (mocks.Playback())
            {
                target.OnOverwriteServerChanges();
            }
            Assert.AreEqual(0, eventMessages.Count);
            Assert.AreEqual(0, eventMessages.Count);
            Assert.AreEqual(0, modifiedData.Count);
        }

        [Test]
        public void VerifyOnCancel()
        {
            using (mocks.Record())
            {
                view.CloseForm(false);
            }
            using (mocks.Playback())
            {
                target.OnCancel();
            }
            CollectionAssert.IsEmpty(model.ModifiedData);
        }
        [Test, SetCulture("sv-SE")]
        public void VerifyOnQueryCellInfo()
        {
            var assPer = new Person { Name = new Name("roger", "moore") };
            var changedByPer = new Person { Name = new Name("hubba", "bubba") };

            var ass = PersonAssignmentFactory.CreateAssignmentWithMainShift(new Scenario("sdf"), assPer, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
            ReflectionHelper.SetUpdatedBy(ass, changedByPer);
            conflicts.Add(new PersistConflictMessageState(new DifferenceCollectionItem<IPersistableScheduleData>(ass, ass.EntityClone()), ass.EntityClone(), eventMessage, removeMessage));

            using (mocks.Record())
            {
                view.SetupGridControl(model.Data);
            }
            using (mocks.Playback())
            {
                target.Initialize();
            }
            Assert.AreEqual(UserTexts.Resources.Name, target.OnQueryCellInfo(0, 1));
            Assert.AreEqual(UserTexts.Resources.Date, target.OnQueryCellInfo(0, 2));
            Assert.AreEqual(UserTexts.Resources.Type, target.OnQueryCellInfo(0, 3));
            Assert.AreEqual(UserTexts.Resources.OtherUser, target.OnQueryCellInfo(0, 4));

            Assert.AreEqual(assPer.Name.ToString(), target.OnQueryCellInfo(1, 1));
            Assert.AreEqual("2000-01-01", target.OnQueryCellInfo(1, 2));
            Assert.AreEqual(UserTexts.Resources.Shift, target.OnQueryCellInfo(1, 3));
            Assert.AreEqual(changedByPer.Name.ToString(), target.OnQueryCellInfo(1, 4));

            Assert.AreEqual(string.Empty, target.OnQueryCellInfo(-1, 4));
        }

        private void forgetAboutDebugAssertsOnMock(int noOfItems)
        {
            var diffRet = new DifferenceCollection<IPersistableScheduleData>();
            for (int i = 0; i < noOfItems; i++)
            {
                diffRet.Add(new DifferenceCollectionItem<IPersistableScheduleData>());
            }
            Expect.Call(schedDic.DifferenceSinceSnapshot()).Return(diffRet).Repeat.Any();
        }

        private void removeMessage(IEventMessage message)
        {
            eventMessages.Remove(message);
        }


        private class dummyScheduleData : IPersistableScheduleData
        {
            public DateTimePeriod Period
            {
                get { return new DateTimePeriod(2000,1,1,2000,1,2); }
            }

            public IPerson Person
            {
                get { return new Person{Name=new Name("Arne", "weise")}; }
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public IScenario Scenario { get; private set; }
            public object Clone()
            {
                return null;
            }

            public bool BelongsToPeriod(IDateOnlyAsDateTimePeriod dateAndPeriod)
            {
                return true;
            }

            public bool BelongsToPeriod(DateOnlyPeriod dateOnlyPeriod)
            {
                return true;
            }

            public bool BelongsToScenario(IScenario scenario)
            {
                return true;
            }

            public bool Equals(IEntity other)
            {
                return true;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public Guid? Id { get; private set; }
            public void SetId(Guid? newId)
            {
                throw new NotImplementedException();
            }

        	public void ClearId()
        	{
        		throw new NotImplementedException();
        	}

        	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public IAggregateRoot MainRoot { get; private set; }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public string FunctionPath { get; private set; }

            public IPersistableScheduleData CreateTransient()
            {
                throw new NotImplementedException();
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public int? Version { get; private set; }
            public void SetVersion(int version) {
                throw new NotImplementedException();
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public IPerson CreatedBy { get; private set; }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public DateTime? CreatedOn { get; private set; }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public IPerson UpdatedBy { get; private set; }
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            public DateTime? UpdatedOn { get; private set; }
        }
    }
}
