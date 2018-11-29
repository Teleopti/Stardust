using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces;


namespace Teleopti.Ccc.WinCodeTest.Payroll
{
    [TestFixture]
    public class VisualizePresenterTest
    {
        private VisualizePresenter _target;
        private MockRepository _mockRepository;
        private IExplorerPresenter _explorerPresenter;
        private IPayrollHelper _payrollHelper;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _explorerPresenter = _mockRepository.StrictMock<IExplorerPresenter>();
            _payrollHelper = _mockRepository.StrictMock<IPayrollHelper>();
            Expect.Call(_explorerPresenter.Helper).Return(_payrollHelper);
            _mockRepository.Replay(_explorerPresenter);
            _target = new VisualizePresenter(_explorerPresenter);
            _mockRepository.Verify(_explorerPresenter);
            _mockRepository.BackToRecord(_explorerPresenter);
        }
        
        [Test]
        public void VerifyLoadModel()
        {
            TimeZoneInfo timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            IList<IMultiplicatorDefinitionSet> multiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet>();
            multiplicatorDefinitionSets.Add(new MultiplicatorDefinitionSet("OB",MultiplicatorType.OBTime));
            multiplicatorDefinitionSets[0].AddDefinition(new DayOfWeekMultiplicatorDefinition(new Multiplicator(MultiplicatorType.OBTime),DayOfWeek.Monday,new TimePeriod(0,0,23,59)));
            IExplorerViewModel explorerViewModel = _mockRepository.StrictMock<IExplorerViewModel>();
            Expect.Call(_explorerPresenter.Model).Return(explorerViewModel).Repeat.AtLeastOnce();
            Expect.Call(explorerViewModel.FilteredDefinitionSetCollection).Return(multiplicatorDefinitionSets).Repeat.AtLeastOnce();
            _mockRepository.ReplayAll();
            _target.LoadModel(new DateOnly(2009,8,3),timeZone);
            Assert.AreEqual(2,_target.ModelCollection.Count);
            _mockRepository.VerifyAll();
        }
    }
}
