using System.Collections.Generic;
using System.Windows.Input;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.WinCode.Scheduling;
using System.Collections.ObjectModel;

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

        [SetUp]
        public void Setup()
        {
            _mockRep = new MockRepository();
            _part = _mockRep.StrictMock<IScheduleDay>();
            IPerson person = PersonFactory.CreatePerson("Kalle");
            var noteDate = new DateOnly(2010, 4, 1);
            IScenario scenario = ScenarioFactory.CreateScenarioAggregate("Default", true, true);
            _note = new Note(person, noteDate, scenario, "note");
            _publicNote = new PublicNote(person, noteDate, scenario, "public note");
            _model1 = null;
            _model2 = null;
            _model3 = null;
        }

        [Test]
        public void VerifyCanCreateModel()
        {
            using (_mockRep.Record())
            {
                Expect.Call(_part.NoteCollection()).Return(new ReadOnlyCollection<INote>(new List<INote>())).Repeat.Once();
                Expect.Call(_part.PublicNoteCollection()).Return(new ReadOnlyCollection<IPublicNote>(new List<IPublicNote>())).Repeat.Once();
                Expect.Call(_part.NoteCollection()).Return(new ReadOnlyCollection<INote>(new List<INote> { _note })).Repeat.Once();
                Expect.Call(_part.PublicNoteCollection()).Return(new ReadOnlyCollection<IPublicNote>(new List<IPublicNote> { _publicNote })).Repeat.Once();
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
            Assert.IsNullOrEmpty(_model1.ScheduleNote);
            Assert.IsNullOrEmpty(_model1.PublicScheduleNote);

            Assert.IsNotNullOrEmpty(_model2.ScheduleNote);
            Assert.IsNotNullOrEmpty(_model2.PublicScheduleNote);

            Assert.IsNullOrEmpty(_model3.ScheduleNote);
            Assert.IsNullOrEmpty(_model3.PublicScheduleNote);
        }

        [Test]
        public void VerifyCanLoadSchedulePart()
        {
            using (_mockRep.Record())
            {
                Expect.Call(_part.NoteCollection()).Return(new ReadOnlyCollection<INote>(new List<INote>())).Repeat.Once();
                Expect.Call(_part.PublicNoteCollection()).Return(new ReadOnlyCollection<IPublicNote>(new List<IPublicNote>())).Repeat.Once();
                Expect.Call(_part.NoteCollection()).Return(new ReadOnlyCollection<INote>(new List<INote> { _note })).Repeat.Once();
                Expect.Call(_part.PublicNoteCollection()).Return(new ReadOnlyCollection<IPublicNote>(new List<IPublicNote> { _publicNote })).Repeat.Once();
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
            Assert.IsNull(_model2.SchedulePart);
            Assert.IsNotNullOrEmpty(_model3.ScheduleNote);
            Assert.IsNotNullOrEmpty(_model3.PublicScheduleNote);

        }

        [Test]
        public void VerifyScheduleNote()
        {
            using (_mockRep.Record())
            {
                Expect.Call(_part.NoteCollection()).Return(new ReadOnlyCollection<INote>(new List<INote> { _note })).Repeat.Twice();
                Expect.Call(_part.PublicNoteCollection()).Return(new ReadOnlyCollection<IPublicNote>(new List<IPublicNote> { _publicNote })).Repeat.Twice();
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
                Expect.Call(_part.NoteCollection()).Return(new ReadOnlyCollection<INote>(new List<INote>())).Repeat.Twice();
                Expect.Call(() => _part.CreateAndAddNote("newNote")).Repeat.Once();
                Expect.Call(_part.PublicNoteCollection()).Return(new ReadOnlyCollection<IPublicNote>(new List<IPublicNote>())).Repeat.Twice();
                Expect.Call(() => _part.CreateAndAddPublicNote("newPublicNote")).Repeat.Once();
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
                Expect.Call(_part.NoteCollection()).Return(new ReadOnlyCollection<INote>(new List<INote> { _note })).Repeat.Twice();
                Expect.Call(_part.PublicNoteCollection()).Return(new ReadOnlyCollection<IPublicNote>(new List<IPublicNote>()));
            }

            using (_mockRep.Playback())
            {
                _model1 = new NotesEditorViewModel(_part) { NotesIsAltered = false };
                _model1.NoteRemoved();
            }

            Assert.IsNullOrEmpty(_model1.ScheduleNote);
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
                Expect.Call(_part.NoteCollection()).Return(new ReadOnlyCollection<INote>(new List<INote> { _note })).Repeat.Twice();
                Expect.Call(_part.PublicNoteCollection()).Return(new ReadOnlyCollection<IPublicNote>(new List<IPublicNote>()));
            }

            using (_mockRep.Playback())
            {
                _model1 = new NotesEditorViewModel(_part);
                CommandModel deleteCommand = _model1.DeleteCommandModel;
                deleteCommand.OnQueryEnabled(_model1, args);
                deleteCommand.OnExecute(_model1, null);

                Assert.AreEqual(UserTexts.Resources.Delete, deleteCommand.Text);
                Assert.IsTrue(args.CanExecute);
                Assert.IsNullOrEmpty(_model1.ScheduleNote);
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
                Expect.Call(_part.NoteCollection()).Return(new ReadOnlyCollection<INote>(new List<INote>()));
                Expect.Call(_part.PublicNoteCollection()).Return(new ReadOnlyCollection<IPublicNote>(new List<IPublicNote> {_publicNote})).Repeat.Twice();
            }

            using (_mockRep.Playback())
            {
                _model1 = new NotesEditorViewModel(_part);
                CommandModel deleteCommand = _model1.DeletePublicNoteCommandModel;
                deleteCommand.OnQueryEnabled(_model1, args);
                deleteCommand.OnExecute(_model1, null);

                Assert.AreEqual(UserTexts.Resources.Delete, deleteCommand.Text);
                Assert.IsTrue(args.CanExecute);
                Assert.IsNullOrEmpty(_model1.PublicScheduleNote);
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
                Expect.Call(_part.NoteCollection()).Return(new ReadOnlyCollection<INote>(new List<INote>()));
                Expect.Call(_part.PublicNoteCollection()).Return(new ReadOnlyCollection<IPublicNote>(new List<IPublicNote> {_publicNote}));
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
