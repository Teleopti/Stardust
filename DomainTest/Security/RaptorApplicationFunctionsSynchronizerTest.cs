using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security
{
    [TestFixture]
    public class RaptorApplicationFunctionsSynchronizerTest
    {

        private RaptorApplicationFunctionsSynchronizerTestClass _target;

        private IList<IApplicationFunction> _definedApplicationFunctions;
        private IList<IApplicationFunction> _databaseApplicationFunctions;
        private IRepositoryFactory _repositoryFactory;
        private IApplicationFunctionRepository _applicationFunctionRepository;
        private IApplicationRoleRepository _applicationRoleRepository;
        private IAvailableDataRepository _availableDataRepository;
        private ICurrentUnitOfWorkFactory _unitOfWorkFactory;
        private IUnitOfWork _unitOfWork;

        private IAvailableData _adminAvailableData;
        private IAvailableData _agentAvailableData;
        private IAvailableData _siteAvailableData;
        private IList<IAvailableData> _availableDataList;

        private IApplicationRole _adminRole;
        private IApplicationRole _agentRole;
        private IApplicationRole _unitRole;
        private IApplicationRole _siteRole;
        private IApplicationRole _teamRole;
        private IList<IApplicationRole> _applicationRoles;

        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _definedApplicationFunctions = CloneDefinedApplicationFunctions(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList);
            _databaseApplicationFunctions = CloneDefinedApplicationFunctions(_definedApplicationFunctions);
            _applicationRoles = ApplicationRoleFactory.CreateShippedRoles(out _adminRole, out _agentRole, out _unitRole, out _siteRole, out _teamRole);

            _availableDataList = CreateAvailableDatas(out _adminAvailableData, out _agentAvailableData, out _siteAvailableData);

            _mocks = new MockRepository();

            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _applicationFunctionRepository = _mocks.StrictMock<IApplicationFunctionRepository>();
            _applicationRoleRepository = _mocks.StrictMock<IApplicationRoleRepository>();
            _availableDataRepository = _mocks.StrictMock<IAvailableDataRepository>();
            _unitOfWorkFactory = _mocks.StrictMock<ICurrentUnitOfWorkFactory>();
            _unitOfWork = _mocks.StrictMock<IUnitOfWork>();

            _target = new RaptorApplicationFunctionsSynchronizerTestClass(_repositoryFactory, _unitOfWorkFactory);
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
            Expect.Call(_repositoryFactory.CreateAvailableDataRepository(_unitOfWork)).Return(_availableDataRepository).Repeat.Once();

            _mocks.ReplayAll();

            _target.CreateLocalRepositories(_unitOfWork);

            _mocks.VerifyAll();

        }

        [Test]
        public void VerifyLoadApplicationFunctions()
        {
            _mocks.Record();

            Expect.Call(_applicationFunctionRepository.GetAllApplicationFunctionSortedByCode()).Return(_databaseApplicationFunctions).Repeat.Once();

            _mocks.ReplayAll();

            _target.LoadApplicationFunctionsFromDatabase(_applicationFunctionRepository);

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
        public void VerifyLoadAllAvailableData()
        {
            _mocks.Record();

            Expect.Call(_availableDataRepository.LoadAllAvailableData()).Return(_availableDataList).Repeat.Once();

            _mocks.ReplayAll();

            _target.LoadAvailableDataList(_availableDataRepository);

            _mocks.VerifyAll();

        }

        /// <summary>
        /// Verifies the assign application function to roles.
        /// </summary>
        /// <remarks>
        /// Creates three application data with three application roles with two of them my unit
        /// type. Those must havew rights to the functions. 
        /// </remarks>
        [Test]
        public void VerifyAssignApplicationFunctionToRoles()
        {
            _adminAvailableData.ApplicationRole = _adminRole;
            _agentAvailableData.ApplicationRole = _agentRole;
            _siteAvailableData.ApplicationRole = _siteRole;

            _adminAvailableData.AvailableDataRange = AvailableDataRangeOption.MyBusinessUnit;
            _agentAvailableData.AvailableDataRange = AvailableDataRangeOption.MyOwn;
            _siteAvailableData.AvailableDataRange = AvailableDataRangeOption.MySite;

            _target.AssignApplicationFunctionToRoles(_availableDataList, new ApplicationFunction("Test"));

            Assert.AreEqual(0, _agentRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, _siteRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(1, _adminRole.ApplicationFunctionCollection.Count);
            // not role asigned
            Assert.AreEqual(0, _teamRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, _unitRole.ApplicationFunctionCollection.Count);

        }

        [Test]
        public void VerifyRemoveApplicationFunctionFromRoles()
        {

            IApplicationFunction function = ApplicationFunctionFactory.CreateApplicationFunction("TEST");

            _adminRole.AddApplicationFunction(function);
            _unitRole.AddApplicationFunction(function);

            Assert.AreEqual(0, _agentRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(1, _unitRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(1, _adminRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, _siteRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, _teamRole.ApplicationFunctionCollection.Count);

            _target.RemoveApplicationFunctionFromRoles(_applicationRoles, function);

            Assert.AreEqual(0, _agentRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, _unitRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, _adminRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, _siteRole.ApplicationFunctionCollection.Count);
            Assert.AreEqual(0, _teamRole.ApplicationFunctionCollection.Count);
        }

        [Test]
        public void VerifyFilterExistingDefinedApplicationFunctions()
        {

            // create test data
            _databaseApplicationFunctions = AddMatrixApplicationFunction(_databaseApplicationFunctions);

            int countBefore = _databaseApplicationFunctions.Count;

            IList<IApplicationFunction> result = _target.FilterExistingDefinedRaptorApplicationFunctions(_databaseApplicationFunctions);

            Assert.AreEqual(countBefore - 1, result.Count);
        }

        [Test]
        public void VerifyAddedDefinedApplicationFunctions()
        {
            _definedApplicationFunctions = AddDefinedApplicationFunction(_definedApplicationFunctions);

            IEnumerable<IApplicationFunction> result = _target.AddedRaptorApplicationFunctions(_databaseApplicationFunctions, _definedApplicationFunctions);
            IList<IApplicationFunction> resultList = new List<IApplicationFunction>(result);

            Assert.AreEqual(1, resultList.Count);
            // the last one is the added one
            Assert.AreSame(resultList[0], _definedApplicationFunctions[_definedApplicationFunctions.Count -1]);
            Assert.AreEqual("EXTRA", resultList[0].FunctionCode);
        }

        [Test]
        public void VerifyDeletedDefinedApplicationFunctions()
        {
            _definedApplicationFunctions = RemoveLastApplicationFunction(_definedApplicationFunctions);

            IEnumerable<IApplicationFunction> result = _target.DeletedRaptorApplicationFunctions(_definedApplicationFunctions, _databaseApplicationFunctions);
            IList<IApplicationFunction> resultList = new List<IApplicationFunction>(result);

            Assert.AreEqual(1, resultList.Count);

            // the last one is deleted
            Assert.AreSame(resultList[0], _databaseApplicationFunctions[_databaseApplicationFunctions.Count - 1]);
        }

        [Test]
        public void VerifyDeleteApplicationFunctionFromRepository()
        {
            IApplicationFunction newApp = new ApplicationFunction("Test");
            _mocks.Record();
            _applicationFunctionRepository.Remove(newApp);
            _mocks.ReplayAll();
            _target.DeleteApplicationFunctionFromRepository(_applicationFunctionRepository, newApp);
            _mocks.VerifyAll();

        }

        [Test]
        public void VerifyAddApplicationFunctionToRepository()
        {
            IApplicationFunction newApp = new ApplicationFunction("Test");

            _mocks.Record();

            _applicationFunctionRepository.Add(newApp);

            _mocks.ReplayAll();

            _target.AddApplicationFunctionToRepository(_applicationFunctionRepository, newApp);

            _mocks.VerifyAll();

        }

        [Test]
        public void VerifyCheckRaptorApplicationFunctionsDeleteFunction()
        {
            // create test data. We imitate a deletion by adding a function to the database functions
            _databaseApplicationFunctions = AddDefinedApplicationFunction(_definedApplicationFunctions);

            _mocks.Record();

            Expect.Call(_repositoryFactory.CreateApplicationFunctionRepository(_unitOfWork)).Return(_applicationFunctionRepository).Repeat.Once();

            Expect.Call(_applicationFunctionRepository.GetAllApplicationFunctionSortedByCode()).Return(_databaseApplicationFunctions).Repeat.Once();
			Expect.Call(_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork()).Return(_unitOfWork).Repeat.Once();
            _unitOfWork.Dispose();
            LastCall.Repeat.Once();

            _mocks.ReplayAll();

			var result = _target.CheckRaptorApplicationFunctions();
			Assert.IsFalse(result.Result);
			Assert.AreEqual(1, result.DeletedFunctions.Count());


            _mocks.VerifyAll();

        }

        [Test]
        public void VerifyCheckRaptorApplicationFunctionsAddFunction()
        {
            // create test data. We imitate a deletion by adding a function to the database functions
            _databaseApplicationFunctions = RemoveLastApplicationFunction(_definedApplicationFunctions);

            _mocks.Record();

            Expect.Call(_repositoryFactory.CreateApplicationFunctionRepository(_unitOfWork)).Return(_applicationFunctionRepository).Repeat.Once();

            Expect.Call(_applicationFunctionRepository.GetAllApplicationFunctionSortedByCode()).Return(_databaseApplicationFunctions).Repeat.Once();
			Expect.Call(_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork()).Return(_unitOfWork).Repeat.Once();
            _unitOfWork.Dispose();
            LastCall.Repeat.Once();

            _mocks.ReplayAll();

			var result = _target.CheckRaptorApplicationFunctions();

			Assert.IsFalse(result.Result);
			Assert.AreEqual(1, result.AddedFunctions.Count());

            _mocks.VerifyAll();

        }

        [Test]
        public void VerifyDigestApplicationFunctions()
        {
            // create test data
            _definedApplicationFunctions = RemoveLastApplicationFunction(_definedApplicationFunctions);
            _definedApplicationFunctions = AddDefinedApplicationFunction(_definedApplicationFunctions);
            _databaseApplicationFunctions = AddMatrixApplicationFunction(_databaseApplicationFunctions);

            _adminAvailableData.AvailableDataRange = AvailableDataRangeOption.MyBusinessUnit;
            _agentAvailableData.AvailableDataRange = AvailableDataRangeOption.MyOwn;
            _siteAvailableData.AvailableDataRange = AvailableDataRangeOption.MySite;

            _mocks.Record();

            Expect.Call(_repositoryFactory.CreateApplicationFunctionRepository(_unitOfWork)).Return(_applicationFunctionRepository).Repeat.Once();
            Expect.Call(_repositoryFactory.CreateApplicationRoleRepository(_unitOfWork)).Return(_applicationRoleRepository).Repeat.Once();
            Expect.Call(_repositoryFactory.CreateAvailableDataRepository(_unitOfWork)).Return(_availableDataRepository).Repeat.Once();
            
            Expect.Call(_availableDataRepository.LoadAllAvailableData()).Return(_availableDataList).Repeat.Once();
            Expect.Call(_applicationFunctionRepository.GetAllApplicationFunctionSortedByCode()).Return(_databaseApplicationFunctions).Repeat.Once();
            Expect.Call(_applicationRoleRepository.LoadAllApplicationRolesSortedByName()).Return(_applicationRoles).Repeat.Once();
			Expect.Call(_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork()).Return(_unitOfWork).Repeat.Once();
            _unitOfWork.Dispose();
            LastCall.Repeat.Once();
            _applicationFunctionRepository.Remove(null);
            LastCall.IgnoreArguments().Repeat.Once();
            _applicationFunctionRepository.Add(null);
            LastCall.IgnoreArguments().Repeat.Once();

            Expect.Call(_unitOfWork.PersistAll()).Return(null).IgnoreArguments().Repeat.Once();

            _mocks.ReplayAll();

            _target.DigestApplicationFunctions(_definedApplicationFunctions);

            _mocks.VerifyAll();

        }

        /// <summary>
        /// Creates the available datas.
        /// </summary>
        /// <param name="adminAvailableData">The admin available data.</param>
        /// <param name="agentAvailableData">The agent available data.</param>
        /// <param name="siteAvailableData">The site available data.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private IList<IAvailableData> CreateAvailableDatas(out IAvailableData adminAvailableData, out IAvailableData agentAvailableData, out IAvailableData siteAvailableData)
        {

            // create three available data objects and add them to a list
            adminAvailableData = new AvailableData();
            agentAvailableData = new AvailableData();
            siteAvailableData = new AvailableData();

            IList<IAvailableData> availableDatas = new List<IAvailableData>();
            availableDatas.Add(adminAvailableData);
            availableDatas.Add(agentAvailableData);
            availableDatas.Add(siteAvailableData);
            return availableDatas;
        }

        /// <summary>
        /// Clones the defined application functions.
        /// </summary>
        /// <param name="definedApplicationFunctions">The defined application functions.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private IList<IApplicationFunction> CloneDefinedApplicationFunctions(IEnumerable<IApplicationFunction> definedApplicationFunctions)
        {
            IList<IApplicationFunction> newList = new List<IApplicationFunction>();
            foreach (IApplicationFunction applicationFunction in definedApplicationFunctions)
            {
                IApplicationFunction newFunction = new ApplicationFunction();
                newFunction.FunctionCode = applicationFunction.FunctionCode;
                newFunction.FunctionDescription = applicationFunction.FunctionDescription;
                newFunction.Parent = applicationFunction.Parent;
                newFunction.ForeignSource = applicationFunction.ForeignSource;
                newFunction.ForeignId = applicationFunction.ForeignId;
                newList.Add(newFunction);
            }
            return newList;
        }

        /// <summary>
        /// Clones the defined application function and then removes and adds one.
        /// </summary>
        /// <param name="applicationFunctions">The application functions.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private IList<IApplicationFunction> AddDefinedApplicationFunction(IList<IApplicationFunction> applicationFunctions)
        {
            // add one more
            IApplicationFunction extraFunction = new ApplicationFunction();
            extraFunction.FunctionCode = "EXTRA";
            extraFunction.FunctionDescription = "Extra function";
            extraFunction.ForeignId = Guid.NewGuid().ToString();
            extraFunction.ForeignSource = DefinedForeignSourceNames.SourceRaptor;
            extraFunction.Parent = ApplicationFunction.FindByForeignId(applicationFunctions, DefinedForeignSourceNames.SourceRaptor, DefinedRaptorApplicationFunctionForeignIds.OpenRaptorApplication);
            applicationFunctions.Add(extraFunction);
            return applicationFunctions;
        }

        /// <summary>
        /// Clones the defined application function and then removes and adds one.
        /// </summary>
        /// <param name="applicationFunctions">The application functions.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private IList<IApplicationFunction> RemoveLastApplicationFunction(IList<IApplicationFunction> applicationFunctions)
        {
            // remove last
            applicationFunctions.RemoveAt(applicationFunctions.Count - 1);
            return applicationFunctions;
        }

        /// <summary>
        /// Adds a matrix application functions.
        /// </summary>
        /// <param name="applicationFunctions">The application functions.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private IList<IApplicationFunction> AddMatrixApplicationFunction(IList<IApplicationFunction> applicationFunctions)
        {
            // add one more
            IApplicationFunction extraFunction = new ApplicationFunction();
            extraFunction.FunctionCode = "REPORT1";
            extraFunction.FunctionDescription = "Report 1 function";
            extraFunction.Parent = ApplicationFunction.FindByForeignId(applicationFunctions, DefinedForeignSourceNames.SourceRaptor, DefinedRaptorApplicationFunctionForeignIds.OpenRaptorApplication);
            ((IEntity)extraFunction).SetId(Guid.NewGuid());
            extraFunction.ForeignId = "1";
            extraFunction.ForeignSource = DefinedForeignSourceNames.SourceMatrix;
            applicationFunctions.Add(extraFunction);
            return applicationFunctions;
        }

    }

}
