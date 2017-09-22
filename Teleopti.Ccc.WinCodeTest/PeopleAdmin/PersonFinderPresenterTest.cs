using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Commands;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.WinCodeTest.PeopleAdmin
{
    [TestFixture]
    public class PersonFinderPresenterTest
    {
        private PersonFinderPresenter _target;
        private IPersonFinderView _view;
        private IPersonFinderModel _model;
        private MockRepository _mocks;
		  private IPeoplePersonFinderSearchCriteria _personFinderSearchCriteria;
        private ReadOnlyCollection<IPersonFinderDisplayRow> _displayRows;
        private IExecutableCommand _executableCommand;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IPersonFinderView>();
            _model = _mocks.StrictMock<IPersonFinderModel>();
				_personFinderSearchCriteria = _mocks.StrictMock<IPeoplePersonFinderSearchCriteria>();
            _displayRows = new ReadOnlyCollection<IPersonFinderDisplayRow>(new List<IPersonFinderDisplayRow>());
            _executableCommand = _mocks.StrictMock<IExecutableCommand>();
            _target = new PersonFinderPresenter(_view, _model);
        }

        [Test]
        public void ShouldReturnModel()
        {
            Assert.AreEqual(_model, _target.Model);   
        }

        [Test]
        public void ShouldFind()
        {
            using(_mocks.Record())
            {
                Expect.Call(() => _executableCommand.Execute());
                Expect.Call(_model.SearchCriteria).Return(_personFinderSearchCriteria);
                Expect.Call(_personFinderSearchCriteria.DisplayRows).Return(_displayRows);
                Expect.Call(() => _view.AddRows(_displayRows));
                Expect.Call(()=>_view.UpdatePageOfStatusText());
                Expect.Call(()=>_view.UpdatePreviousNextStatus());
                Expect.Call(() => _view.UpdateButtonOkStatus());
            }

            using(_mocks.Playback())
            {
                _target.Find(_executableCommand);   
            }
        }

        [Test]
        public void ShouldThrowExceptionOnNullCommand()
        {
            Assert.Throws<ArgumentNullException>(() => _target.Find(null));   
        }

        [Test]
        public void ShouldSetSortColumnAndOrderToCriteria()
        {
            Expect.Call(_model.SearchCriteria).Return(_personFinderSearchCriteria).Repeat.AtLeastOnce();
            Expect.Call(() => _personFinderSearchCriteria.SortColumn = 1).Repeat.Twice();
            Expect.Call(() => _personFinderSearchCriteria.SortDirection = 1);
            Expect.Call(() => _personFinderSearchCriteria.SortDirection = 2);
            Expect.Call(() => _view.AttemptDatabaseConnectionFind(new PersonFinderFindCommand(_model))).IgnoreArguments().Repeat.Twice();
            _mocks.ReplayAll();
            _target.ListViewColumnClick(1);
            _target.ListViewColumnClick(1);
            _mocks.VerifyAll();
        }
        
        [Test]
        public void ShouldFindOnButtonFindClick()
        {
            var command = new PersonFinderFindCommand(_model);

            using (_mocks.Record())
            {
                Expect.Call(_view.PersonFinderSearchCriteria).Return(_personFinderSearchCriteria);
                Expect.Call(() => _model.SearchCriteria = _personFinderSearchCriteria);
                Expect.Call(() => _view.AttemptDatabaseConnectionFind(command));
            }

            using (_mocks.Playback())
            {
                _target.ButtonFindClick(command);
            }
        }

        [Test]
        public void ShouldFindOnComboBoxRowsPerPageSelectedIndexChanged()
        {
            var command = new PersonFinderFindCommand(_model);

            using(_mocks.Record())
            {
                Expect.Call(_view.PersonFinderSearchCriteria).Return(_personFinderSearchCriteria);
                Expect.Call(() => _model.SearchCriteria = _personFinderSearchCriteria);
                Expect.Call(() => _view.AttemptDatabaseConnectionFind(command));
            }

            using(_mocks.Playback())
            {
                _target.ComboBoxRowsPerPageSelectedIndexChanged(command);
            }
        }

        [Test]
        public void ShouldFindPreviousOnLinkLabelPreviousClick()
        {
            var command = new PersonFinderPreviousCommand(_model);

            using (_mocks.Record())
            {
                Expect.Call(_view.PersonFinderSearchCriteriaNextPrevious).Return(_personFinderSearchCriteria);
                Expect.Call(() => _model.SearchCriteria = _personFinderSearchCriteria);
                Expect.Call(() => _view.AttemptDatabaseConnectionFind(command));
            }

            using (_mocks.Playback())
            {
                _target.LinkLabelPreviousLinkClicked(command);
            }
        }

        [Test]
        public void ShouldFindNextOnLinkLabelNextClick()
        {
            var command = new PersonFinderNextCommand(_model);

            using (_mocks.Record())
            {
                Expect.Call(_view.PersonFinderSearchCriteriaNextPrevious).Return(_personFinderSearchCriteria);
                Expect.Call(() => _model.SearchCriteria = _personFinderSearchCriteria);
                Expect.Call(() => _view.AttemptDatabaseConnectionFind(command));
            }

            using (_mocks.Playback())
            {
                _target.LinkLabelNextLinkClicked(command);
            }
        }
    }
}
