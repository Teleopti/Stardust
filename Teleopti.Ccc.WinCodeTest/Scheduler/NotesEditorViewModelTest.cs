using System;
using System.Windows.Input;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCodeTest.Common.Commands;

using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
    [TestFixture]
    public class NotesEditorViewModelTest
    {
        private NotesEditorViewModel _model1;
        private NotesEditorViewModel _model2;
        private NotesEditorViewModel _model3;
        private IScheduleDay _part;
        private MockRepository _mockRep;
        private INote _note;
        private IPublicNote _publicNote;
        private readonly DateOnly _noteDate = new DateOnly(2010, 4, 1);
        private IDateOnlyAsDateTimePeriod _period;
        private IPerson _person;
		private IDisposable auth;

		[SetUp]
        public void Setup()
        {
            _mockRep = new MockRepository();
            _part = _mockRep.StrictMock<IScheduleDay>();
            _person = PersonFactory.CreatePerson("Kalle");
            
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate("Default", true);
            _note = new Note(_person, _noteDate, scenario, "note");
            _period = new DateOnlyAsDateTimePeriod(_noteDate, TimeZoneInfo.Utc);
            _publicNote = new PublicNote(_person, _noteDate, scenario, "public note");
            _model1 = null;
            _model2 = null;
            _model3 = null;

			auth = CurrentAuthorization.ThreadlyUse(new FullPermission());
		}

		[TearDown]
		public void Teardown()
		{
			auth?.Dispose();
		}

        [Test]
        public void VerifyCanCreateModel()
        {
            using (_mockRep.Record())
            {
                Expect.Call(_part.NoteCollection()).Return(new INote[0]).Repeat.Once();
                Expect.Call(_part.PublicNoteCollection()).Return(new IPublicNote[0]).Repeat.Once();
                Expect.Call(_part.NoteCollection()).Return(new [] { _note }).Repeat.Once();
                Expect.Call(_part.PublicNoteCollection()).Return(new [] { _publicNote }).Repeat.Once();
                Expect.Call(_part.DateOnlyAsPeriod).Return(_period).Repeat.AtLeastOnce();
                Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_part.FullAccess).Return(true).Repeat.AtLeastOnce();
            }

            using (_mockRep.Playback())
            {
                _model1 = new NotesEditorViewModel(_part);
                _model2 = new NotesEditorViewModel(_part);
                _model3 = new NotesEditorViewModel(null);
            }

            Assert.IsNotNull(_model1.SchedulePart);
            Assert.IsNotNull(_model1.ChangedCommand);
            Assert.IsNotNull(_model1.ChangedPublicNoteCommand);
            Assert.IsNotNull(_model1.DeleteCommandModel);
            Assert.IsNotNull(_model1.DeletePublicNoteCommandModel);
            Assert.That(_model1.ScheduleNote, Is.Null.Or.Empty);
			Assert.That(_model1.PublicScheduleNote, Is.Null.Or.Empty);

			Assert.That(_model2.ScheduleNote, Is.Not.Null.And.Not.Empty);
			Assert.That(_model2.PublicScheduleNote, Is.Not.Null.And.Not.Empty);

			Assert.That(_model3.ScheduleNote, Is.Null.Or.Empty);
			Assert.That(_model3.PublicScheduleNote, Is.Null.Or.Empty);
		}

        [Test]
        public void VerifyCanLoadSchedulePart()
        {
            using (_mockRep.Record())
            {
                Expect.Call(_part.NoteCollection()).Return(new INote[0]).Repeat.Once();
                Expect.Call(_part.PublicNoteCollection()).Return(new IPublicNote[0]).Repeat.Once();
                Expect.Call(_part.NoteCollection()).Return(new [] { _note }).Repeat.Once();
                Expect.Call(_part.PublicNoteCollection()).Return(new [] { _publicNote }).Repeat.Once();
                Expect.Call(_part.DateOnlyAsPeriod).Return(_period).Repeat.AtLeastOnce();
                Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_part.FullAccess).Return(true).Repeat.AtLeastOnce();
            }

            using (_mockRep.Playback())
            {
                _model1 = new NotesEditorViewModel(null);
                _model1.Load(_part);

                _model2 = new NotesEditorViewModel(null);
                _model2.Load(null);

                _model3 = new NotesEditorViewModel(null) {NotesIsAltered = true, PublicNotesIsAltered = true};
                _model3.Load(_part);
            }

            Assert.IsNotNull(_model1.SchedulePart);
            Assert.IsTrue(_model1.IsEnabled);
            Assert.IsNull(_model2.SchedulePart);
            Assert.IsFalse(_model2.IsEnabled);
			Assert.That(_model3.ScheduleNote, Is.Not.Null.And.Not.Empty);

			Assert.That(_model3.PublicScheduleNote, Is.Not.Null.And.Not.Empty);
			Assert.IsTrue(_model3.IsEnabled);
        }

        [Test]
        public void ShouldHandleNoAccessToPeriod()
        {
            using (_mockRep.Record())
            {
                Expect.Call(_part.DateOnlyAsPeriod).Return(_period).Repeat.AtLeastOnce();
                Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_part.NoteCollection()).Return(new INote[0]).Repeat.Once();
                Expect.Call(_part.PublicNoteCollection()).Return(new IPublicNote[0]).Repeat.Once();
            }

            using (_mockRep.Playback())
            {
                using (CurrentAuthorization.ThreadlyUse(new NoPermission()))
                {
                    _model1 = new NotesEditorViewModel(null);
                    _model1.Load(_part);
                }
                _model1.IsEnabled.Should().Be.False();
            }
        }
        [Test]
        public void VerifyScheduleNote()
        {
            using (_mockRep.Record())
            {
                Expect.Call(_part.NoteCollection()).Return(new [] { _note }).Repeat.Twice();
                Expect.Call(_part.PublicNoteCollection()).Return(new [] { _publicNote }).Repeat.Twice();
                Expect.Call(_part.DateOnlyAsPeriod).Return(_period).Repeat.AtLeastOnce();
                Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_part.FullAccess).Return(true);
            }

            using (_mockRep.Playback())
            {
                _model1 = new NotesEditorViewModel(_part)
                              {
                                  NotesIsAltered = true,
                                  ScheduleNote = "altered 1",
                                  PublicNotesIsAltered = true,
                                  PublicScheduleNote = "altered 2"
                              };
            }

            Assert.IsFalse(_model1.NotesIsAltered);
            Assert.IsFalse(_model1.PublicNotesIsAltered);
        }

        [Test]
        public void VerifyAddNewNote()
        {
            using (_mockRep.Record())
            {
                Expect.Call(_part.NoteCollection()).Return(new INote[0]).Repeat.Twice();
                Expect.Call(() => _part.CreateAndAddNote("newNote")).Repeat.Once();
                Expect.Call(_part.PublicNoteCollection()).Return(new IPublicNote[0]).Repeat.Twice();
                Expect.Call(() => _part.CreateAndAddPublicNote("newPublicNote")).Repeat.Once();
                Expect.Call(_part.DateOnlyAsPeriod).Return(_period).Repeat.AtLeastOnce();
                Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_part.FullAccess).Return(true);
            }

            using (_mockRep.Playback())
            {
                _model1 = new NotesEditorViewModel(_part) {ScheduleNote = "newNote", PublicScheduleNote = "newPublicNote"};
            }
        }

        [Test]
        public void VerifyNotesIsAltered()
        {
            _model1 = new NotesEditorViewModel(null) { NotesIsAltered = true };
            Assert.IsTrue(_model1.NotesIsAltered);
        }

        [Test]
        public void VerifyNoteRemoved()
        {
            using (_mockRep.Record())
            {
                Expect.Call(_part.DeleteNote).Repeat.Once();
                Expect.Call(_part.NoteCollection()).Return(new [] { _note }).Repeat.Twice();
                Expect.Call(_part.PublicNoteCollection()).Return(new IPublicNote[0]);
                Expect.Call(_part.DateOnlyAsPeriod).Return(_period).Repeat.AtLeastOnce();
                Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_part.FullAccess).Return(true);
            }

            using (_mockRep.Playback())
            {
                _model1 = new NotesEditorViewModel(_part) { NotesIsAltered = false };
                _model1.NoteRemoved();
            }

	        Assert.That(_model1.ScheduleNote, Is.Null.Or.Empty);
        }

        [Test]
        public void VerifyChangedCommand()
        {
            _model1 = new NotesEditorViewModel(null);
            Assert.IsNotNull(_model1.ChangedCommand);
        }

        [Test]
        public void VerifyDeleteCommandModel()
        {
            _model1 = new NotesEditorViewModel(null);
            Assert.IsNotNull(_model1.DeleteCommandModel);
        }

        [Test]
        public void VerifyChangedCommandBehavior()
        {
            _model1 = new NotesEditorViewModel(null);
            ICommand changedCommand = _model1.ChangedCommand;

            changedCommand.Execute(null);
            Assert.IsTrue(changedCommand.CanExecute(null));
            Assert.IsTrue(_model1.NotesIsAltered);
        }

        [Test]
        public void VerifyDeleteCommand()
        {
            var models = new TesterForCommandModels();
            CanExecuteRoutedEventArgs args = models.CreateCanExecuteRoutedEventArgs();

            using (_mockRep.Record())
            {
                Expect.Call(_part.DeleteNote).Repeat.Once();
                Expect.Call(_part.NoteCollection()).Return(new [] { _note }).Repeat.Twice();
                Expect.Call(_part.PublicNoteCollection()).Return(new IPublicNote[0]);
                Expect.Call(_part.DateOnlyAsPeriod).Return(_period).Repeat.AtLeastOnce();
                Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_part.FullAccess).Return(true);
            }

            using (_mockRep.Playback())
            {
                _model1 = new NotesEditorViewModel(_part);
                CommandModel deleteCommand = _model1.DeleteCommandModel;
                deleteCommand.OnQueryEnabled(_model1, args);
                deleteCommand.OnExecute(_model1, null);

                Assert.AreEqual(UserTexts.Resources.Delete, deleteCommand.Text);
                Assert.IsTrue(args.CanExecute);
				Assert.That(_model1.ScheduleNote, Is.Null.Or.Empty);
            }
        }

        [Test]
        public void VerifyDeleteCommandForPublicNote()
        {
            var models = new TesterForCommandModels();
            CanExecuteRoutedEventArgs args = models.CreateCanExecuteRoutedEventArgs();

            using (_mockRep.Record())
            {
                Expect.Call(_part.DeletePublicNote).Repeat.Once();
                Expect.Call(_part.NoteCollection()).Return(new INote[0]);
                Expect.Call(_part.PublicNoteCollection()).Return(new [] {_publicNote}).Repeat.Twice();
                Expect.Call(_part.DateOnlyAsPeriod).Return(_period).Repeat.AtLeastOnce();
                Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_part.FullAccess).Return(true);
            }

            using (_mockRep.Playback())
            {
                _model1 = new NotesEditorViewModel(_part);
                CommandModel deleteCommand = _model1.DeletePublicNoteCommandModel;
                deleteCommand.OnQueryEnabled(_model1, args);
                deleteCommand.OnExecute(_model1, null);

                Assert.AreEqual(UserTexts.Resources.Delete, deleteCommand.Text);
                Assert.IsTrue(args.CanExecute);
	            Assert.That(_model1.PublicScheduleNote, Is.Null.Or.Empty);
            }
        }

        [Test]
        public void VerifyReadOnly()
        {

            _model1 = new NotesEditorViewModel(null) { IsEnabled = false };
            Assert.IsFalse(_model1.IsEnabled);

            _model1.IsEnabled = true;
            Assert.IsTrue(_model1.IsEnabled);
        }

		[Test]
		public void ScheduleDayShouldBeNullAfterCallingLoadWithNull()
		{
			 using (_mockRep.Record())
            {
                Expect.Call(_part.NoteCollection()).Return(new INote[0]);
                Expect.Call(_part.PublicNoteCollection()).Return(new [] {_publicNote});
                Expect.Call(_part.DateOnlyAsPeriod).Return(_period).Repeat.AtLeastOnce();
                Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_part.FullAccess).Return(true);
            }

			 using (_mockRep.Playback())
			 {
			 	_model1 = new NotesEditorViewModel(_part);
			 	_model1.Load(null);
			 }
			Assert.IsNull(_model1.SchedulePart);
		}
    }
}
