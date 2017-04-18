using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Grouping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Grouping
{
    [TestFixture]
    public class FindPersonPeriodHandlerTest
    {
        private FindPersonPeriodHandler _target;
        private IFindPersonsModel _findPersonsModel;
        private IFindPersonsView _findPersonsView;
        private MockRepository _mocks;


        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _findPersonsModel = _mocks.StrictMock<IFindPersonsModel>();
            _findPersonsView = _mocks.StrictMock<IFindPersonsView>();

            _target = new FindPersonPeriodHandler(_findPersonsModel, _findPersonsView);
        }

        [Test]
        public void ShouldReturnFalseWhenFromIsGreaterThanTo()
        {
            using(_mocks.Record())
            {
                Expect.Call(_findPersonsView.FromDate).Return(new DateTime(2011, 10, 10, 0, 0, 0, DateTimeKind.Utc));
                Expect.Call(_findPersonsView.ToDate).Return(new DateTime(2011, 9, 9, 0, 0, 0, DateTimeKind.Utc));
            }
            using(_mocks.Playback())
            {
                Assert.IsFalse(_target.CheckPeriod());
            }
        }

        [Test]
        public void ShouldReturnTrueWhenFromIsLessThanTo()
        {
            using (_mocks.Record())
            {
                Expect.Call(_findPersonsView.FromDate).Return(new DateTime(2011, 9, 9, 0, 0, 0, DateTimeKind.Utc));
                Expect.Call(_findPersonsView.ToDate).Return(new DateTime(2011, 10, 10, 0, 0, 0, DateTimeKind.Utc));
            }
            using (_mocks.Playback())
            {
                Assert.IsTrue(_target.CheckPeriod());
            }    
        }

        [Test]
        public void ShouldSetErrorWhenFromIsGreaterThanTo()
        {
            using(_mocks.Record())
            {
                Expect.Call(_findPersonsView.FromDate).Return(new DateTime(2011, 10, 10, 0, 0, 0, DateTimeKind.Utc));
                Expect.Call(_findPersonsView.ToDate).Return(new DateTime(2011, 9, 9, 0, 0, 0, DateTimeKind.Utc));
                Expect.Call(() => _findPersonsView.SetErrorOnStartDate(UserTexts.Resources.StartDateMustBeSmallerThanEndDate));
                Expect.Call(() => _findPersonsView.TextBoxFindEnabled = false);
            }

            using(_mocks.Playback())
            {
                _target.FromDateChanged();
            }
        }

        [Test]
        public void ShouldSetErrorWhenToIsLessThanFrom()
        {
            using (_mocks.Record())
            {
				Expect.Call(_findPersonsView.FromDate).Return(new DateTime(2011, 10, 10, 0, 0, 0, DateTimeKind.Utc)).Repeat.AtLeastOnce();
                Expect.Call(_findPersonsView.ToDate).Return(new DateTime(2011, 9, 9, 0, 0, 0, DateTimeKind.Utc)).Repeat.AtLeastOnce();
                Expect.Call(() => _findPersonsView.SetErrorOnEndDate(UserTexts.Resources.StartDateMustBeSmallerThanEndDate));
                Expect.Call(() => _findPersonsView.TextBoxFindEnabled = false);
            }

            using (_mocks.Playback())
            {
                _target.ToDateChanged();
            }
        }

        [Test]
        public void ShouldRebuildIndexWhenFromIsLessThanTo()
        {
            using(_mocks.Record())
            {
                Expect.Call(_findPersonsView.FromDate).Return(new DateTime(2011, 9, 9, 0, 0, 0, DateTimeKind.Utc)).Repeat.Twice();
                Expect.Call(_findPersonsView.ToDate).Return(new DateTime(2011, 10, 10, 0, 0, 0, DateTimeKind.Utc));
                Expect.Call(() => _findPersonsModel.FromDate = new DateOnly(new DateTime(2011, 9, 9, 0, 0, 0, DateTimeKind.Utc)));
                Expect.Call(() => _findPersonsView.ClearDateErrors());
                Expect.Call(() => _findPersonsView.TextBoxFindEnabled = true);
            }

            using(_mocks.Playback())
            {
                _target.FromDateChanged();
            }
        }

        [Test]
        public void ShouldRebuildIndexWhenToIsGreaterThanFrom()
        {
            using (_mocks.Record())
            {
				Expect.Call(_findPersonsView.FromDate).Return(new DateTime(2011, 9, 9, 0, 0, 0, DateTimeKind.Utc)).Repeat.AtLeastOnce();
				Expect.Call(_findPersonsView.ToDate).Return(new DateTime(2011, 10, 10, 0, 0, 0, DateTimeKind.Utc)).Repeat.AtLeastOnce();
				Expect.Call(() => _findPersonsModel.ToDate = new DateOnly(new DateTime(2011, 10, 10, 0, 0, 0, DateTimeKind.Utc)));
                Expect.Call(() => _findPersonsView.ClearDateErrors());
                Expect.Call(() => _findPersonsView.TextBoxFindEnabled = true);
            }

            using (_mocks.Playback())
            {
                _target.ToDateChanged();
            }
        }
    }
}
