using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.Matrix
{
    [TestFixture]
    public class MatrixReportsSynchronizerTest
    {
        private IList<MatrixReportInfo> _matrixReports;
        private IList<IApplicationFunction> _applicationFunctions;
        private IApplicationFunctionRepository _applicationFunctionRepository;
        private IApplicationRoleRepository _applicationRoleRepository;
        private IAvailableDataRepository _availableDataRepository;
        private IStatisticRepository _statisticRepository;
        private MatrixReportsSynchronizerTestClass _target;
        private MockRepository _mocks;
        private IRepositoryFactory _repositoryFactory;
        private IUnitOfWork _unitOfWork;
        private IList<IAvailableData> _availableDataList;
        private IApplicationRole _adminRole;
        private IApplicationRole _agentRole;
        private IApplicationRole _unitRole;
        private IApplicationRole _siteRole;
        private IApplicationRole _teamRole;
        private IList<IApplicationRole> _applicationRoles;

        [SetUp]
        public void Setup()
        {
            _matrixReports = new List<MatrixReportInfo>();
            
            _applicationRoles = ApplicationRoleFactory.CreateShippedRoles(out _adminRole, out _agentRole, out _unitRole, out _siteRole, out _teamRole);

            _availableDataList = CreateAvailableDatas();

            var idOne = "09DB7510-ED3C-49CE-B49C-D43D94EC7263";
            var idTwo = "1C2BDC8C-BFED-4BB3-AD13-6614488310BE";
            var idFour = "78AD4AF8-41E8-416F-8AE2-FCAC2D98B81F";

            _matrixReports.Add(new MatrixReportInfo(new Guid(idOne), "Agent List"));
            _matrixReports.Add(new MatrixReportInfo(new Guid(idTwo), "Site List"));
            _matrixReports.Add(new MatrixReportInfo(new Guid(idFour), "Team List"));

            _applicationFunctions = ApplicationFunctionFactory.CreateApplicationFunctionWithMatrixReports();

            _mocks = new MockRepository();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _applicationFunctionRepository = _mocks.StrictMock<IApplicationFunctionRepository>();
            _applicationRoleRepository = _mocks.StrictMock<IApplicationRoleRepository>();
            _statisticRepository = _mocks.StrictMock<IStatisticRepository>();
            _availableDataRepository = _mocks.StrictMock<IAvailableDataRepository>();
            _unitOfWork = _mocks.StrictMock<IUnitOfWork>();

            _target = new MatrixReportsSynchronizerTestClass(_repositoryFactory, _unitOfWork);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyCreateLocalRepositories()
        {
            _mocks.Record();

            Expect.Call(_repositoryFactory.CreateApplicationFunctionRepository(_unitOfWork)).Return(_applicationFunctionRepository).Repeat.Once();
            Expect.Call(_repositoryFactory.CreateApplicationRoleRepository(_unitOfWork)).Return(_applicationRoleRepository).Repeat.Once();
            Expect.Call(_repositoryFactory.CreateStatisticRepository()).Return(_statisticRepository).Repeat.Once();

            _mocks.ReplayAll();

            _target.CreateLocalRepositories();

            _mocks.VerifyAll();

        }

        [Test]
        public void VerifyLoadApplicationFunctions()
        {
            _mocks.Record();

            Expect.Call(_applicationFunctionRepository.GetAllApplicationFunctionSortedByCode()).Return(_applicationFunctions).Repeat.Once();

            _mocks.ReplayAll();

            _target.LoadApplicationFunctions(_applicationFunctionRepository);

            _mocks.VerifyAll();

        }

        [Test]
        public void VerifyLoadApplicationRoles()
        {
            _mocks.Record();

            Expect.Call(_applicationRoleRepository.LoadAllApplicationRolesSortedByName()).Return(_applicationRoles).Repeat.Once();

            _mocks.ReplayAll();

            _target.LoadApplicationRoles(_applicationRoleRepository);

            _mocks.VerifyAll();

        }

        [Test]
        public void VerifyLoadMatrixReports()
        {
            _mocks.Record();

            Expect.Call(_statisticRepository.LoadReports()).Return(_matrixReports).Repeat.Once();

            _mocks.ReplayAll();

            _target.LoadMatrixReports(_statisticRepository);

            _mocks.VerifyAll();

        }

        [Test]
        public void VerifyLoadAllAvailableData()
        {
            _mocks.Record();

            Expect.Call(_availableDataRepository.LoadAllAvailableData()).Return(_availableDataList).Repeat.Once();

            _mocks.ReplayAll();

            _target.LoadAvailableDataList(_availableDataRepository);

            _mocks.VerifyAll();

        }

        [Test]
        public void VerifyCreateMatrixReportApplicationFunction()
        {
            string reportDescription = "xxViewAgents";
            MatrixReportInfo info = new MatrixReportInfo(Guid.NewGuid(), reportDescription);
            IApplicationFunction app = _target.CreateMatrixReportApplicationFunction(_applicationFunctions,  info);
            Assert.AreEqual(app.FunctionDescription, reportDescription);
            Assert.AreEqual(DefinedForeignSourceNames.SourceMatrix, app.ForeignSource);
            Assert.AreEqual(info.ReportId, new Guid(app.ForeignId));
        }

        [Test]
        public void VerifyRemoveMatrixApplicationFunctionFromRoles()
        {

            IApplicationFunction function = ApplicationFunctionFactory.CreateApplicationFunction("TEST");

            _adminRole.AddApplicationFunction(function);
            _agentRole.AddApplicationFunction(function);

            Assert.AreEqual(1, _agentRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, _siteRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(1, _adminRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, _teamRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, _unitRole.ApplicationFunctionCollection.Count);

            _target.RemoveMatrixApplicationFunctionFromRoles(_applicationRoles, function);

            Assert.AreEqual(0, _agentRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, _siteRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, _adminRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, _teamRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, _unitRole.ApplicationFunctionCollection.Count);
        }

        [Test]
        public void VerifyNewMatrixReports()
        {
            IEnumerable<MatrixReportInfo> result = _target.AddedMatrixReports(_matrixReports, _applicationFunctions);
            IList<MatrixReportInfo> resultList = new List<MatrixReportInfo>(result);

            Assert.AreEqual(1, resultList.Count);
            Assert.AreSame(resultList[0], _matrixReports[2]);
        }

        [Test]
        public void VerifyDeletedMatrixReports()
        {
            IEnumerable<IApplicationFunction> result = _target.DeletedMatrixReports(_applicationFunctions, _matrixReports);
            IList<IApplicationFunction> resultList = new List<IApplicationFunction>(result);

            Assert.AreEqual(1, resultList.Count);
            Assert.AreSame(resultList[0], _applicationFunctions[6]);
        }

        [Test]
        public void VerifyUpdateMatrixReports()
        {
            IList<IApplicationFunction> matrixApplicationFunctionList = _target.FilterExistingMatrixReportApplicationFunctions(_applicationFunctions);
            matrixApplicationFunctionList[0].FunctionDescription = "Agent";

            Assert.AreEqual(_matrixReports[0].ReportId, new Guid(matrixApplicationFunctionList[0].ForeignId));
            Assert.AreNotEqual(_matrixReports[0].ReportName, matrixApplicationFunctionList[0].FunctionDescription);

            _target.UpdateMatrixApplicationFunctions(matrixApplicationFunctionList, _matrixReports);

            Assert.AreEqual(_matrixReports[0].ReportName, matrixApplicationFunctionList[0].FunctionDescription);
        }

        [Test]
        public void VerifyFilterExistingMatrixReportApplicationFunctions()
        {
            IList<IApplicationFunction> result = _target.FilterExistingMatrixReportApplicationFunctions(_applicationFunctions);
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(3, result.Count);
            Assert.AreSame(result[0], _applicationFunctions[3]);
            Assert.AreSame(result[1], _applicationFunctions[5]);
            Assert.AreSame(result[2], _applicationFunctions[6]);
        }

        [Test]
        public void VerifyDeleteApplicationFunctionFromRepository()
        {
            ApplicationFunction newApp = new ApplicationFunction("Test");

            _mocks.Record();

            _applicationFunctionRepository.Remove(newApp);
            
            _mocks.ReplayAll();

            _target.DeleteApplicationFunctionFromRepository(_applicationFunctionRepository, newApp);

            _mocks.VerifyAll();

        }

        [Test]
        public void VerifyAddApplicationFunctionToRepository()
        {
            ApplicationFunction newApp = new ApplicationFunction("Test");

            _mocks.Record();

            _applicationFunctionRepository.Add(newApp);

            _mocks.ReplayAll();

            _target.AddApplicationFunctionToRepository(_applicationFunctionRepository, newApp);

            _mocks.VerifyAll();

        }

        [Test]
        public void VerifyDigestMatrixReport()
        {
            _mocks.Record();

            Expect.Call(_repositoryFactory.CreateApplicationFunctionRepository(_unitOfWork)).Return(_applicationFunctionRepository).Repeat.Once();
            Expect.Call(_repositoryFactory.CreateApplicationRoleRepository(_unitOfWork)).Return(_applicationRoleRepository).Repeat.Once();
            Expect.Call(_repositoryFactory.CreateStatisticRepository()).Return(_statisticRepository).Repeat.Once();


            Expect.Call(_statisticRepository.LoadReports()).Return(_matrixReports).Repeat.Once();
            Expect.Call(_applicationFunctionRepository.GetAllApplicationFunctionSortedByCode()).Return(_applicationFunctions).Repeat.Once();
            Expect.Call(_applicationRoleRepository.LoadAllApplicationRolesSortedByName()).Return(_applicationRoles).Repeat.Once();
            
            _applicationFunctionRepository.Add(null);
            LastCall.IgnoreArguments().Repeat.Once();

            _applicationFunctionRepository.Remove(null);
            LastCall.IgnoreArguments().Repeat.Once();

            Expect.Call(_unitOfWork.PersistAll()).Return(null).IgnoreArguments().Repeat.Once();

            _mocks.ReplayAll();

            _target.SynchronizeMatrixReports();

            _mocks.VerifyAll();

        }

        private IList<IAvailableData> CreateAvailableDatas()
        {
            // create three available data objects and add them to a list
            var adminAvailableData = new AvailableData();
            var agentAvailableData = new AvailableData();
            var siteAvailableData = new AvailableData();
            IList<IAvailableData> availableDatas = new List<IAvailableData>();
            availableDatas.Add(adminAvailableData);
            availableDatas.Add(agentAvailableData);
            availableDatas.Add(siteAvailableData);
            return availableDatas;
        }

    }
}