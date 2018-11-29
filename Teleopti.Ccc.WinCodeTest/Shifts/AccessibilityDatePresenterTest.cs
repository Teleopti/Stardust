using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;

using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Presenters;

namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture]
    public class AccessibilityDatePresenterTest
    {
        private MockRepository _mock;
        private AccessibilityDatePresenter _target;
        private IDataHelper _helper;
        private IExplorerPresenter _explorer;
        private IExplorerViewModel _model;
        private IList<IWorkShiftRuleSet> _ruleSetCollection;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _explorer = _mock.StrictMock<IExplorerPresenter>();
            _model = _mock.StrictMock<IExplorerViewModel>();
            _helper = _mock.StrictMock<IDataHelper>();

            _ruleSetCollection = new List<IWorkShiftRuleSet>();
            _ruleSetCollection.Add(WorkShiftRuleSetFactory.Create());

            _ruleSetCollection[0].AddAccessibilityDate(new DateOnly(2009, 02, 16));
            _ruleSetCollection[0].AddAccessibilityDate(new DateOnly(2009, 02, 17));

            _target = new AccessibilityDatePresenter(_explorer,_helper);
        }

        [Test]
        public void VerifyLoadModelCollection()
        {
            using (_mock.Record())
            {
                Expect
                    .On(_explorer)
                    .Call(_explorer.Model)
                    .Return(_model);
                Expect
                    .On(_model)
                    .Call(_model.FilteredRuleSetCollection)
                    .Return(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection));
            }
            using (_mock.Playback())
            {
                _target.LoadModelCollection();
                Assert.AreEqual(2, _target.ModelCollection.Count);
            }
        }

        [Test]
        public void VerifyAddAccessibilityDate()
        {
            using (_mock.Record())
            {
                Expect
                    .On(_explorer)
                    .Call(_explorer.Model)
                    .Return(_model).Repeat.Twice();
                Expect
                    .On(_model)
                    .Call(_model.FilteredRuleSetCollection)
                    .Return(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection)).Repeat.Twice();
            }
            using (_mock.Playback())
            {
                _target.LoadModelCollection();
                _target.AddAccessibilityDate();
                Assert.AreEqual(3, _target.ModelCollection.Count);
            }
        }

        [Test]
        public void VerifyRemoveSelectedAccessibilityDates()
        {
            using (_mock.Record())
            {
                Expect
                    .On(_explorer)
                    .Call(_explorer.Model)
                    .Return(_model).Repeat.Times(3);
                Expect
                    .On(_model)
                    .Call(_model.FilteredRuleSetCollection)
                    .Return(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection)).Repeat.Times(3);
            }
            using (_mock.Playback())
            {
                _target.LoadModelCollection();
                _target.RemoveSelectedAccessibilityDates(new ReadOnlyCollection<int>(new List<int> {0, 1}));
                Assert.AreEqual(0, _target.ModelCollection.Count);
            }
        }

        [Test]
        public void VerifySetAccessibilityDates()
        {
            _target.SetAccessibilityDates(new ReadOnlyCollection<IAccessibilityDateViewModel>(
                new List<IAccessibilityDateViewModel>
                    {
                    new AccessibilityDateViewModel(_ruleSetCollection[0], new DateTime(2009, 02, 20))
                } 
            ));
            Assert.AreEqual(1, _target.ModelCollection.Count);
        }
    }
}
