#region Imports

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

#endregion

namespace Teleopti.Ccc.WinCodeTest.Payroll
{
    [TestFixture]
    public class DefinitionSetPresenterTest
    {
        private MockRepository _mockRepository;
        private IUnitOfWork _unitOfWork;
        private IPayrollHelper _helper;
        private IDefinitionSetPresenter _target;
        private IExplorerPresenter _explorerPresenter;

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

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _unitOfWork = _mockRepository.StrictMock<IUnitOfWork>();
            _helper = _mockRepository.StrictMock<IPayrollHelper>();
            _explorerPresenter = _mockRepository.StrictMock<IExplorerPresenter>();

            using(_mockRepository.Record())
            {
                Expect
                    .On(_helper)
                    .Call(_helper.UnitOfWork)
                    .Return(_unitOfWork)
                    .Repeat.Any();

                Expect
                    .On(_helper)
                    .Call(_helper.LoadDefinitionSets())
                    .Return(new List<IMultiplicatorDefinitionSet>() { _definitionSet, _definitionSet1, _definitionSet2, _definitionSet3, _definitionSet4 })
                    .Repeat.Any();

                Expect
                    .On(_explorerPresenter)
                    .Call(_explorerPresenter.Helper)
                    .Return(_helper)
                    .Repeat.Any();

                _helper.Save(_definitionSet);
                LastCall.IgnoreArguments().Repeat.Any();
                _helper.Delete(_definitionSet);
                LastCall.IgnoreArguments().Repeat.Any();

            }
            _mockRepository.ReplayAll();

            _target = new DefinitionSetPresenter(_explorerPresenter);
        }

        [Test]
        public void VerifyCanAccessProperties()
        {
            Assert.AreEqual(_explorerPresenter, ((CommonViewHolder<IDefinitionSetViewModel>) _target).ExplorerPresenter);
            Assert.AreEqual(_helper, ((CommonViewHolder<IDefinitionSetViewModel>)_target).Helper);
        }

        [Test]
        public void VerifyAddNewDefinitionSet()
        {
            _target.AddNewDefinitionSet(_definitionSet);
            Assert.AreEqual(1, _target.ModelCollection.Count);
        }

        [Test]
        public void VerifyRemoveDefinitionSet()
        {
            _target.AddNewDefinitionSet(_definitionSet);
            Assert.AreEqual(1, _target.ModelCollection.Count);

            _target.RemoveDefinitionSet(_definitionSet);
            Assert.AreEqual(0, _target.ModelCollection.Count);
        }

        [Test]
        public void VerifyLoadModel()
        {
            _target.LoadModel();
            Assert.AreEqual(5, _target.ModelCollection.Count);
        }

        [Test]
        public void VerifyRenameDefinitionSet()
        {
            string oldName = _definitionSet.Name;
            _target.RenameDefinitionSet(_definitionSet, "Changed Name");
            Assert.AreEqual("Changed Name", _definitionSet.Name);
            _target.RenameDefinitionSet(_definitionSet, oldName);
        }

    }
}
