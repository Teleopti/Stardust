using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Payroll
{
    [TestFixture]
    public class ExplorerPresenterTest
    {
        private MockRepository _mockRepository;
        private IUnitOfWork _unitOfWork;
        private IPayrollHelper _helper;
        private IExplorerView _explorerView;
        private IExplorerPresenter _target;

        private readonly IMultiplicatorDefinitionSet _definitionSet =
            new MultiplicatorDefinitionSet("Sample1", MultiplicatorType.OBTime);
        private readonly IMultiplicatorDefinitionSet _definitionSet1 =
            new MultiplicatorDefinitionSet("Sample2", MultiplicatorType.OBTime);
        private readonly IMultiplicatorDefinitionSet _definitionSet2 =
            new MultiplicatorDefinitionSet("Sample3", MultiplicatorType.OBTime);
        private readonly IMultiplicatorDefinitionSet _definitionSet3 =
            new MultiplicatorDefinitionSet("Sample4", MultiplicatorType.OBTime);
        private readonly IMultiplicatorDefinitionSet _definitionSet4 =
            new MultiplicatorDefinitionSet("Sample5", MultiplicatorType.OBTime);

        private readonly IMultiplicator multiplicator1 = 
            new Multiplicator(MultiplicatorType.OBTime);
        private readonly IMultiplicator multiplicator2 = 
            new Multiplicator(MultiplicatorType.Overtime);

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _unitOfWork = _mockRepository.StrictMock<IUnitOfWork>();
            _helper = _mockRepository.StrictMock<IPayrollHelper>();
            _explorerView = _mockRepository.StrictMock<IExplorerView>();

            using (_mockRepository.Record())
            {
                Expect
                    .On(_explorerView)
                    .Call(_explorerView.UnitOfWork)
                    .Return(_unitOfWork)
                    .Repeat.Any();

                Expect
                   .On(_helper)
                   .Call(_helper.UnitOfWork)
                   .Return(_unitOfWork)
                   .Repeat.Any();

                Expect
                    .On(_helper)
                    .Call(_helper.LoadDefinitionSets())
                    .Return(new List<IMultiplicatorDefinitionSet> { _definitionSet, _definitionSet1, _definitionSet2, _definitionSet3, _definitionSet4 })
                    .Repeat.Any();

                Expect
                    .On(_helper)
                    .Call(_helper.LoadMultiplicatorList())
                    .Return(new List<IMultiplicator> { multiplicator1, multiplicator2 })
                    .Repeat.Any();
            }
            _mockRepository.ReplayAll();
            _target = new ExplorerPresenter(_helper, _explorerView);
        }

        [Test]
        public void VerifyCanAccessProperties()
        {
            Assert.IsNotNull(_target.Model);
            Assert.IsNotNull(_target.DefinitionSetPresenter);
            Assert.IsNotNull(_target.VisualizePresenter);
            Assert.IsNotNull(_target.Helper);
            Assert.IsNotNull(_target.MultiplicatorDefinitionPresenter);
        }
    }
}
