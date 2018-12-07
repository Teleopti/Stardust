using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces;

namespace Teleopti.Ccc.WinCodeTest.Payroll
{
    [TestFixture]
    public class PayrollHelperTest
    {
        private MockRepository _mockRepository;
        private IUnitOfWork _unitOfWork;
        private IPayrollHelper _target;
        private IRepositoryFactory _repositoryFactory;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _repositoryFactory = _mockRepository.StrictMock<IRepositoryFactory>();
            _unitOfWork = _mockRepository.StrictMock<IUnitOfWork>();

            _target = new PayrollHelper(_unitOfWork, _repositoryFactory);
        }

        [Test]
        public void VerifyCanAccessProperties()
        {
            _target = new PayrollHelper(_unitOfWork);
            Assert.AreEqual(_unitOfWork, _target.UnitOfWork);
        }

        [Test]
        public void VerifyDelete()
        {
            IMultiplicatorDefinitionSet multiplicatorDefinitionSet =
                _mockRepository.StrictMock<IMultiplicatorDefinitionSet>();
            IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository =
                _mockRepository.StrictMock<IMultiplicatorDefinitionSetRepository>();
            multiplicatorDefinitionSetRepository.Remove(multiplicatorDefinitionSet);
            Expect.Call(_repositoryFactory.CreateMultiplicatorDefinitionSetRepository(_unitOfWork)).Return(
                multiplicatorDefinitionSetRepository);
            _mockRepository.ReplayAll();
            _target.Delete(multiplicatorDefinitionSet);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyAdd()
        {
            IMultiplicatorDefinitionSet multiplicatorDefinitionSet =
                _mockRepository.StrictMock<IMultiplicatorDefinitionSet>();
            IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository =
                _mockRepository.StrictMock<IMultiplicatorDefinitionSetRepository>();
            multiplicatorDefinitionSetRepository.Add(multiplicatorDefinitionSet);
            Expect.Call(_repositoryFactory.CreateMultiplicatorDefinitionSetRepository(_unitOfWork)).Return(
                multiplicatorDefinitionSetRepository);
            _mockRepository.ReplayAll();
            _target.Save(multiplicatorDefinitionSet);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyLoadMultiplicatorDefinitionSets()
        {
            IList<IMultiplicatorDefinitionSet> multiplicatorDefinitionSets =
                new List<IMultiplicatorDefinitionSet>();
            IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository =
                _mockRepository.StrictMock<IMultiplicatorDefinitionSetRepository>();
            Expect.Call(_repositoryFactory.CreateMultiplicatorDefinitionSetRepository(_unitOfWork)).Return(
                multiplicatorDefinitionSetRepository);
            Expect.Call(multiplicatorDefinitionSetRepository.LoadAll()).Return(multiplicatorDefinitionSets);
            _mockRepository.ReplayAll();
            Assert.AreEqual(0,_target.LoadDefinitionSets().Count);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyLoadMultiplicatorList()
        {
            IList<IMultiplicator> multiplicators = new List<IMultiplicator>();
            IMultiplicatorRepository multiplicatorRepository = _mockRepository.StrictMock<IMultiplicatorRepository>();
            Expect.Call(_repositoryFactory.CreateMultiplicatorRepository(_unitOfWork)).Return(multiplicatorRepository);
            Expect.Call(multiplicatorRepository.LoadAllSortByName()).Return(multiplicators);

            _mockRepository.ReplayAll();
            Assert.AreEqual(0,_target.LoadMultiplicatorList().Count);
            _mockRepository.VerifyAll();
        }
    }
}
