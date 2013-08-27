using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using NUnit.Framework;
using Rhino.Mocks;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Restriction
{
    [TestFixture]
    public class RestrictionEditorViewModelTest
    {
        private MockRepository _mockRep;
        private DateTimePeriod _period;
        private DateTimePeriod _rangePeriod;
        private DateOnly _restrictionDateTimeWithinPart;
        private DateTime _baseDateTime;
        private IScheduleDay _mockedPart;
        private IPerson _mockedPerson;
        private RestrictionEditorViewModel _target;
        private TesterForCommandModels _models;
        private IPerson _person;
        private IScheduleDay _targetPart;

        [SetUp]
        public void Setup()
        {
            _models = new TesterForCommandModels();
            _baseDateTime = new DateTime(2001, 1, 1, 8, 0, 0, DateTimeKind.Utc);
            _rangePeriod = new DateTimePeriod(2000, 1, 1, 2001, 6, 1);
            _restrictionDateTimeWithinPart = new DateOnly(_baseDateTime.AddHours(5));
            _period = new DateTimePeriod(_baseDateTime, _baseDateTime.AddDays(4));
            _mockRep = new MockRepository();
            _mockedPart = _mockRep.StrictMock<IScheduleDay>();
            _mockedPerson = _mockRep.StrictMock<IPerson>();
            _person = PersonFactory.CreatePerson();
            _targetPart = createPart(_person, new DateOnly(2000,1,1));
            _target = new RestrictionEditorViewModel(null,new List<IActivity>(),new List<IDayOffTemplate>(),new List<IShiftCategory>());
        }

        [Test]
        public void VerifyLoadsFromSchedulePart()
        {
            RestrictionBase scheduleRestriction = new RotationRestriction();
            IList<IScheduleData> restrictions = new List<IScheduleData>();
            var personRestriction = _mockRep.StrictMock<IScheduleDataRestriction>();
            restrictions.Add(personRestriction);
            var retColl = new ReadOnlyCollection<IScheduleData>(restrictions);
            using (_mockRep.Record())
            {
                Expect.Call(_mockedPart.Period).Return(_period).Repeat.Any();
                Expect.Call(_mockedPart.Person).Return(_mockedPerson).Repeat.Any();
                Expect.Call(_mockedPart.PersonRestrictionCollection()).Return(retColl).Repeat.Times(3);
                Expect.Call(personRestriction.Restriction).Return(scheduleRestriction).Repeat.Any();
                Expect.Call(personRestriction.RestrictionDate).Return(_restrictionDateTimeWithinPart).Repeat.Any();
            }
            using (_mockRep.Playback())
            {
                _target.Load(_mockedPart);
                Assert.AreEqual(_mockedPart, _target.SchedulePart);
                Assert.AreEqual(1, _target.RestrictionModels.Count());
            }
        }

        [Test]
        public void VerifyCanAddPreferenceRestrictionToEditor()
        {
            var target = new RestrictionEditorViewModel(_targetPart,new List<IActivity>(),new List<IDayOffTemplate>(),new List<IShiftCategory>());
            CommandModel addPreference = target.AddPreferenceRestrictionCommand;
            
            //Check that we can add add:
            CanExecuteRoutedEventArgs args = _models.CreateCanExecuteRoutedEventArgs();
            addPreference.OnQueryEnabled(null, args);
            Assert.IsTrue(args.CanExecute);

            //Invoke the command (twice):
            ExecutedRoutedEventArgs executeArgs = _models.CreateExecutedRoutedEventArgs();
            addPreference.OnExecute(null,executeArgs);
            Assert.AreEqual(1, target.RestrictionModels.OfType<PreferenceRestrictionViewModel>().Count());
            addPreference.OnExecute(null, executeArgs);
            Assert.AreEqual(2, target.RestrictionModels.OfType<PreferenceRestrictionViewModel>().Count());
        }

        [Test]
        public void VerifyCanAddStudentAvailabilityRestrictionToEditor()
        {

            var target = new RestrictionEditorViewModel(_targetPart,new List<IActivity>(),new List<IDayOffTemplate>(),new List<IShiftCategory>());
            CommandModel addStudentAvailability = target.AddStudentAvailabilityCommand;

            //Check that we can add add:
            CanExecuteRoutedEventArgs args = _models.CreateCanExecuteRoutedEventArgs();
            addStudentAvailability.OnQueryEnabled(null, args);
            Assert.IsTrue(args.CanExecute);

            //Invoke the command (twice):
            ExecutedRoutedEventArgs executeArgs = _models.CreateExecutedRoutedEventArgs();
            addStudentAvailability.OnExecute(null, executeArgs);
            Assert.AreEqual(1, target.RestrictionModels.OfType<AvailableRestrictionViewModel>().Count(m => m.Description == UserTexts.Resources.StudentAvailability));
            addStudentAvailability.OnExecute(null, executeArgs);
            Assert.AreEqual(2, target.RestrictionModels.OfType<AvailableRestrictionViewModel>().Count(m => m.Description == UserTexts.Resources.StudentAvailability));
        }

        [Test]
        public void VerifyCanDeleteRestriction()
        {
            var rangePeriod = new DateTimePeriod(2000, 1, 1, 2001, 6, 1);
            IScheduleDay part = createPart(_person, new DateOnly(2000, 1, 1));
            var restriction = new PreferenceRestriction();
            var preferenceDay = new PreferenceDay(_person, new DateOnly(rangePeriod.StartDateTime), restriction);
            part.Add(preferenceDay);
			_target.Load(part);
            
            Assert.AreEqual(1, _target.RestrictionModels.Count());
            _target.RestrictionRemoved(_target.RestrictionModels[0]);
            Assert.AreEqual(0, _target.RestrictionModels.Count());
        }

        [Test]
        public void ShouldClearRestrictionsWhenSchedulePartIsNull()
        {
            var rangePeriod = new DateTimePeriod(2000, 1, 1, 2001, 6, 1);
            IScheduleDay part = createPart(_person, new DateOnly(2000, 1, 1));
            var restriction = new PreferenceRestriction();
            var preferenceDay = new PreferenceDay(_person, new DateOnly(rangePeriod.StartDateTime), restriction);
            part.Add(preferenceDay);
            _target.Load(part);
            Assert.AreEqual(1, _target.RestrictionModels.Count());
            _target.Load(null);
            Assert.AreEqual(0, _target.RestrictionModels.Count());
        }

        [Test]
        public void VerifyCanUpdateAllCommand()
        {
            //Create 2 restrictionviewmodels and add to _target, check the updates by changing startTimeLimits
            int timesRestrictionFired = 0;
            TimeSpan oldStartTime = TimeSpan.FromHours(8);
            TimeSpan newStartTime = oldStartTime.Add(TimeSpan.FromHours(1));
            var startTimeLimitation = new StartTimeLimitation(oldStartTime, null);
            IScheduleDay part = createPart(_person, new DateOnly(2000, 1, 1));
            var restriction = new PreferenceRestriction {StartTimeLimitation = startTimeLimitation};
            var preferenceDay1 = new PreferenceDay(_person, new DateOnly(_rangePeriod.StartDateTime),restriction);

            part.Add(preferenceDay1);
            _target.Load(part);

            //Do changes (it feels like this would be better off with mocks):
            _target.RestrictionModels[0].StartTimeLimits.StartTime = newStartTime.ToString();

            //Check that the changes arent done to the part:
            var res = (PreferenceDay)part.PersonRestrictionCollection()[0];
            Assert.AreEqual(startTimeLimitation,res.Restriction.StartTimeLimitation);

            //Verify that the UpdateAllCommand can run
            CanExecuteRoutedEventArgs args = _models.CreateCanExecuteRoutedEventArgs();
            _target.UpdateAllCommand.OnQueryEnabled(null,args);
            Assert.IsTrue(args.CanExecute);

            //Verify commands alters the underlying part, and fires RestrictionAltered once and restrictions are changed on the underlying part
            _target.RestrictionChanged += delegate { timesRestrictionFired++; };
            _target.UpdateAllCommand.OnExecute(null, _models.CreateExecutedRoutedEventArgs());

            //Assert.AreEqual(1, timesRestrictionFired);
            res = (PreferenceDay)part.PersonRestrictionCollection()[0];
            Assert.AreNotEqual(startTimeLimitation.StartTime,
                          res.Restriction.StartTimeLimitation.StartTime);

            //VerifyTexts:
            Assert.AreEqual(UserTexts.Resources.Update,_target.UpdateAllCommand.Text);
            Assert.AreEqual(UserTexts.Resources.Update,_target.UpdateAllCommand.DescriptionText);
        }

        [Test]
        public void VerifyRestrictionIsChangedIfTheRestrictionIsAlteredByEventCommand()
        {
            IScheduleDay targetPart = createPart(_person, new DateOnly(2000, 1, 1));
            var target = new RestrictionEditorViewModel(targetPart,new List<IActivity>(),new List<IDayOffTemplate>(),new List<IShiftCategory>());
            CommandModel addPreference = target.AddPreferenceRestrictionCommand;

            //Add RestrictionModel
            ExecutedRoutedEventArgs executeArgs = _models.CreateExecutedRoutedEventArgs();
            addPreference.OnExecute(null, executeArgs);

            ICommand command = target.RestrictionModels[0].UpdateOnEventCommand;
            //Check that we can run the command:
            Assert.IsTrue(command.CanExecute(null));
            Assert.IsFalse(target.RestrictionIsAltered);

            //Run the command, this would be the same as triggering a change from the GUI, like TextBox.TextChanged etc....
            command.Execute(null);
            Assert.IsTrue(target.RestrictionIsAltered,"One of the restrictions are altered, but the schedulepart is not updated");

            target.RestrictionAltered();
            Assert.IsFalse(target.RestrictionIsAltered);
        }

        [Test]
        public void VerifyChangedCommandChangesIsAltered()
        {
            Assert.IsFalse(_target.RestrictionIsAltered, "Restriction is not changed");
            Assert.IsTrue(_target.ChangedCommand.CanExecute(null),"Make sure we can fire the command");
            _target.ChangedCommand.Execute(null);
            Assert.IsTrue(_target.RestrictionIsAltered);

            //Updating all models should set it back to false
            _target.UpdateAllCommand.OnExecute(null, _models.CreateExecutedRoutedEventArgs());
            Assert.IsFalse(_target.RestrictionIsAltered);
        }

       

        [Test]
        public void VerifyLoadCollections()
        {
            IUnitOfWork uow = _mockRep.StrictMock<IUnitOfWork>();
            IUnitOfWorkFactory unitOfWorkFactory = _mockRep.StrictMock<IUnitOfWorkFactory>();
            IScheduleDay part = new SchedulePartFactoryForDomain().CreatePartWithMainShift();
            var repositoryFactory = _mockRep.StrictMock<IRepositoryFactory>();
            var activityRepository = _mockRep.StrictMock<IActivityRepository>();
            var shiftCategoryRepository = _mockRep.StrictMock<IShiftCategoryRepository>();
            var dayOffRepository = _mockRep.StrictMock<IDayOffTemplateRepository>();
            IList<IActivity> activities = new List<IActivity>();
            IList<IShiftCategory> shiftCategories = new List<IShiftCategory>();
            IList<IDayOffTemplate> dayOffTemplates = new List<IDayOffTemplate>();

            using (_mockRep.Record())
            {
                Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(uow);
                Expect.Call(repositoryFactory.CreateActivityRepository(uow)).Return(activityRepository);
                Expect.Call(repositoryFactory.CreateShiftCategoryRepository(uow)).Return(shiftCategoryRepository);
                Expect.Call(repositoryFactory.CreateDayOffRepository(uow)).Return(dayOffRepository);

                Expect.Call(activityRepository.LoadAll()).Return(activities);
                Expect.Call(shiftCategoryRepository.LoadAll()).Return(shiftCategories);
                Expect.Call(dayOffRepository.LoadAll()).Return(dayOffTemplates);
                Expect.Call(uow.Dispose).Repeat.Once();//Verify that uow is disposed
            }
            using (_mockRep.Playback())
            {
                _target = new RestrictionEditorViewModel(part, repositoryFactory, unitOfWorkFactory);
                Assert.AreEqual(activities,_target.Activities);
                Assert.AreEqual(shiftCategories,_target.ShiftCategories);
                Assert.AreEqual(dayOffTemplates,_target.DayOffTemplates);
            }
        }

        private static IScheduleDay createPart(IPerson person,DateOnly dateOnly)
        {
            IScheduleDictionary dictionaryNotUsed = new ScheduleDictionaryForTest(ScenarioFactory.CreateScenarioAggregate(),
                                                                                  new ScheduleDateTimePeriod(new DateTimePeriod(1800,1,1,2100,1,1)),
                                                                                  new Dictionary
                                                                                      <IPerson, IScheduleRange>());
            return ExtractedSchedule.CreateScheduleDay(dictionaryNotUsed, person, dateOnly);
        }
    }
}