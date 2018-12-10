using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MbCache.Core;
using Microsoft.Practices.Composite.Events;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Models;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Presenters;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Views;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Shifts
{
    [TestFixture]
    public class GeneralPresenterTest
    {
        private MockRepository _mock;
        //private IUnitOfWork _uow;
        private IExplorerView _explorerView;
        private ExplorerPresenter _explorerPresenter;
        private GeneralPresenter _target;
        private IList<IWorkShiftRuleSet> _ruleSetCollection;
        private IDataHelper _dataHelper;
    	private bool _eventFired;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IEventAggregator _eventAggregator;
        private IMbCacheFactory _mbCacheFactory;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _explorerView = _mock.StrictMock<IExplorerView>();
            _dataHelper = _mock.StrictMock<IDataHelper>();
            _unitOfWorkFactory = _mock.StrictMock<IUnitOfWorkFactory>();
            _eventAggregator = new EventAggregator();
            _mbCacheFactory = _mock.StrictMock<IMbCacheFactory>();
            _ruleSetCollection = new List<IWorkShiftRuleSet>();
            _ruleSetCollection.Add(WorkShiftRuleSetFactory.Create());

            _ruleSetCollection[0].AddAccessibilityDate(new DateOnly(2009, 02, 16));
            _ruleSetCollection[0].AddAccessibilityDate(new DateOnly(2009, 02, 17));
            var model = new ExplorerViewModel { DefaultSegment = 15 };
            model.SetRuleSetCollection(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection));
            model.SetFilteredRuleSetCollection(new ReadOnlyCollection<IWorkShiftRuleSet>(_ruleSetCollection));
				_explorerPresenter = new ExplorerPresenter(_explorerView, _dataHelper, new RuleSetProjectionEntityService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate())),
                _unitOfWorkFactory, _eventAggregator, _mbCacheFactory, model);
            
            _target = new GeneralPresenter(_explorerPresenter,_dataHelper);
        	_eventFired = false;
        }

        [Test]
        public void VerifyCanReadProperties()
        {
            Assert.IsNotNull(_target.GeneralTemplatePresenter);
            Assert.IsNotNull(_target.ActivityPresenter);
            Assert.IsNotNull(_target.AccessibilityDatePresenter);
            Assert.IsNotNull(_target.ActivityTimeLimiterPresenter);
            Assert.IsNotNull(_target.DaysOfWeekPresenter);
        }

        [Test]
        public void VerifyLoadModelCollection()
        {
            _target.LoadModelCollection();
            Assert.IsTrue(true);
        }

        [Test]
        public void VerifyCanValidate()
        {
            _target.LoadModelCollection();

            Assert.IsTrue(_target.Validate());

            _target.GeneralTemplatePresenter.ModelCollection[0].StartPeriodStartTime = TimeSpan.FromHours(9);
            _target.GeneralTemplatePresenter.ModelCollection[0].StartPeriodEndTime = TimeSpan.FromHours(8);

            Assert.IsFalse(_target.Validate());
        }

		[Test]
		public void ShouldInvokeGeneralTemplatePresenterEvent()
		{
			_target.GeneralTemplatePresenter.OnlyForRestrictionsChanged += GeneralTemplatePresenterOnlyForRestrictionsChanged;
			_target.GeneralTemplatePresenter.InvokeOnlyForRestrictionsCellChanged();
	
			Assert.IsTrue(_eventFired);

			_target.GeneralTemplatePresenter.OnlyForRestrictionsChanged -= GeneralTemplatePresenterOnlyForRestrictionsChanged;
		}

		void GeneralTemplatePresenterOnlyForRestrictionsChanged(object sender, EventArgs e)
		{
			_eventFired = true;
		}
    }
}
