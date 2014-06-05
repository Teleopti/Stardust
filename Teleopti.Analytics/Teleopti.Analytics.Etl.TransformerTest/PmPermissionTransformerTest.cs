using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.PerformanceManager;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.PerformanceManagerProxy;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Analytics.PM.PMServiceHost;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;
using JobResult = Teleopti.Analytics.Etl.Transformer.Job.JobResult;
using PersonFactory = Teleopti.Analytics.Etl.TransformerTest.FakeData.PersonFactory;

namespace Teleopti.Analytics.Etl.TransformerTest
{
    [TestFixture]
    public class PmPermissionTransformerTest
    {
        private PmPermissionTransformer _target;
        private IList<IPerson> _personCollection;
        private const string JobStepName = "Performance Manager permissions";
        private UserDto _userAb;
        private UserDto _userAb2;
        private UserDto _userCd;
        private UserDto _userCd2;
        private UserDto _userEf;
        private UserDto _userEf2;
        private MockRepository _mocks;
        private IPmPermissionExtractor _permissionExtractor;
	    private IUnitOfWorkFactory _unitOfWorkFactory;

	    [SetUp]
        public void Setup()
        {
            _target = new PmPermissionTransformer(null);
            _personCollection = PersonFactory.CreatePersonGraphCollection();

            _mocks = new MockRepository();
            _permissionExtractor = _mocks.StrictMock<IPmPermissionExtractor>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
        }

        [Test]
        public void ShouldGetUserWhichOnlyGotWindowsCredentials()
        {
            IPerson person = _personCollection[4];
            Assert.IsNotNullOrEmpty(person.AuthenticationInfo.Identity);
            Assert.That(person.ApplicationAuthenticationInfo, Is.Null);

            var applicationRole = person.PermissionInformation.ApplicationRoleCollection[0];
            Expect.Call(_permissionExtractor.ExtractPermission(applicationRole.ApplicationFunctionCollection,_unitOfWorkFactory)).Return(PmPermissionType.ReportDesigner);
            _mocks.ReplayAll();

            IList<UserDto> users = _target.GetUsersWithPermissionsToPerformanceManager(new List<IPerson> { person }, true, _permissionExtractor, _unitOfWorkFactory);
            _mocks.VerifyAll();

            Assert.AreEqual(1, users.Count);
            Assert.IsTrue(users[0].IsWindowsLogOn);
        }

        [Test]
        public void ShouldGetUserWhichOnlyGotApplicationCredentials()
        {
            IPerson person = _personCollection[6]; // Belongs to three roles
            Assert.That(person.AuthenticationInfo, Is.Null);

            Assert.IsNotNullOrEmpty(person.ApplicationAuthenticationInfo.ApplicationLogOnName);
            Assert.IsNotNullOrEmpty(person.ApplicationAuthenticationInfo.Password);

            var applicationRole1 = person.PermissionInformation.ApplicationRoleCollection[0];
            var applicationRole2 = person.PermissionInformation.ApplicationRoleCollection[1];
            var applicationRole3 = person.PermissionInformation.ApplicationRoleCollection[2];

            Expect.Call(_permissionExtractor.ExtractPermission(applicationRole1.ApplicationFunctionCollection,_unitOfWorkFactory)).Return(PmPermissionType.ReportDesigner);
            Expect.Call(_permissionExtractor.ExtractPermission(applicationRole2.ApplicationFunctionCollection,_unitOfWorkFactory)).Return(PmPermissionType.GeneralUser);
            Expect.Call(_permissionExtractor.ExtractPermission(applicationRole3.ApplicationFunctionCollection,_unitOfWorkFactory)).Return(PmPermissionType.None);
            _mocks.ReplayAll();

            IList<UserDto> users = _target.GetUsersWithPermissionsToPerformanceManager(new List<IPerson> { person }, false, _permissionExtractor, _unitOfWorkFactory);
            _mocks.VerifyAll();

            Assert.AreEqual(1, users.Count);
            Assert.IsFalse(users[0].IsWindowsLogOn);
        }

        [Test]
        public void ShouldGetUserWhichGotBothWindowsAndApplicationCredentials()
        {
            IPerson person = _personCollection[3];
            Assert.IsNotNullOrEmpty(person.AuthenticationInfo.Identity);
            Assert.IsNotNullOrEmpty(person.ApplicationAuthenticationInfo.ApplicationLogOnName);
            Assert.IsNotNullOrEmpty(person.ApplicationAuthenticationInfo.Password);

            var applicationRole = person.PermissionInformation.ApplicationRoleCollection[0];

            Expect.Call(_permissionExtractor.ExtractPermission(applicationRole.ApplicationFunctionCollection,_unitOfWorkFactory)).Return(PmPermissionType.GeneralUser).Repeat.Twice();
            _mocks.ReplayAll();

            IList<UserDto> windowUsers = _target.GetUsersWithPermissionsToPerformanceManager(new List<IPerson> { person }, true, _permissionExtractor, _unitOfWorkFactory);
            IList<UserDto> applicationUsers = _target.GetUsersWithPermissionsToPerformanceManager(new List<IPerson> { person }, false, _permissionExtractor, _unitOfWorkFactory);
            _mocks.VerifyAll();

            Assert.AreEqual(1, windowUsers.Count);
            Assert.AreEqual(1, applicationUsers.Count);
            Assert.IsTrue(windowUsers[0].IsWindowsLogOn);
            Assert.IsFalse(applicationUsers[0].IsWindowsLogOn);
        }

        [Test]
        public void ShouldOnlyReturnUsersWithWindowsLogOn()
        {
            ensureBothWindowsAndApplicationUsersExistInList();

            Expect.Call(_permissionExtractor.ExtractPermission(new List<IApplicationFunction>(),_unitOfWorkFactory)).Return(
                PmPermissionType.GeneralUser).Repeat.AtLeastOnce().IgnoreArguments();
            _mocks.ReplayAll();

            IList<UserDto> users = _target.GetUsersWithPermissionsToPerformanceManager(_personCollection, true, _permissionExtractor, _unitOfWorkFactory);
            _mocks.VerifyAll();

            Assert.Greater(users.Count(u => u.IsWindowsLogOn), 0);
            Assert.AreEqual(0, users.Count(u => !u.IsWindowsLogOn));
        }

        [Test]
        public void ShouldOnlyReturnUsersWithApplicationLogOn()
        {
            ensureBothWindowsAndApplicationUsersExistInList();

            Expect.Call(_permissionExtractor.ExtractPermission(new List<IApplicationFunction>(),_unitOfWorkFactory)).Return(
                PmPermissionType.GeneralUser).Repeat.AtLeastOnce().IgnoreArguments();
            _mocks.ReplayAll();

            IList<UserDto> users = _target.GetUsersWithPermissionsToPerformanceManager(_personCollection, false, _permissionExtractor, _unitOfWorkFactory);
            _mocks.VerifyAll();

            Assert.Greater(users.Count(u => !u.IsWindowsLogOn), 0);
            Assert.AreEqual(0, users.Count(u => u.IsWindowsLogOn));
        }

        [Test]
        public void ShouldNotGetUsersThatLacksPermissionToPm()
        {
            IPerson person = _personCollection[7]; // Does not belong to a ApplicationRole
            Assert.IsNotNullOrEmpty(person.AuthenticationInfo.Identity);
            Assert.AreEqual(0, person.PermissionInformation.ApplicationRoleCollection.Count);

            IList<UserDto> users = _target.GetUsersWithPermissionsToPerformanceManager(new List<IPerson> { person }, true, _permissionExtractor, _unitOfWorkFactory);
            Assert.AreEqual(0, users.Count);
        }

        [Test]
        public void ShouldCheckThatUserHasViewPermission()
        {
            IPerson person = _personCollection[3];
            var applicationRole = person.PermissionInformation.ApplicationRoleCollection[0];

            Expect.Call(_permissionExtractor.ExtractPermission(applicationRole.ApplicationFunctionCollection,_unitOfWorkFactory)).Return(PmPermissionType.GeneralUser);
            _mocks.ReplayAll();

            IList<UserDto> userDtoCollection = _target.GetUsersWithPermissionsToPerformanceManager(new List<IPerson> { person }, true, _permissionExtractor, _unitOfWorkFactory);
            _mocks.VerifyAll();
            Assert.AreEqual(1, userDtoCollection[0].AccessLevel);
        }

        [Test]
        public void ShouldCheckThatUserHasCreatePermission()
        {
            IPerson person = _personCollection[4];
            var applicationRole = person.PermissionInformation.ApplicationRoleCollection[0];

            Expect.Call(_permissionExtractor.ExtractPermission(applicationRole.ApplicationFunctionCollection,_unitOfWorkFactory)).Return(PmPermissionType.ReportDesigner);
            _mocks.ReplayAll();

            IList<UserDto> userDtoCollection = _target.GetUsersWithPermissionsToPerformanceManager(new List<IPerson> { person }, true, _permissionExtractor, _unitOfWorkFactory);
            _mocks.VerifyAll();

            Assert.AreEqual(2, userDtoCollection[0].AccessLevel);
        }



        private void ensureBothWindowsAndApplicationUsersExistInList()
        {
            int windowsUserCount =
                _personCollection.Count(
                    p => p.AuthenticationInfo != null);

            int applicationUserCount =
                _personCollection.Count(
                    p => p.ApplicationAuthenticationInfo != null);

            Assert.Greater(windowsUserCount, 0);
            Assert.Greater(applicationUserCount, 0);
        }

        [Test]
        public void VerifyGetPmUsersForAllBusinessUnits()
        {
            // Create user collection
            _userAb = new UserDto { AccessLevel = 2, UserName = "a\\b" };   // pmUser=true
            _userCd2 = new UserDto { AccessLevel = 2, UserName = "c\\d" };  // pmUser=false
            _userEf2 = new UserDto { AccessLevel = 1, UserName = "e\\f" };  // pmUser=false
            IList<UserDto> pmUserCurrentBuCollection = new List<UserDto> { _userAb, _userEf2, _userCd2 };

            IList<UserDto> pmUsers = _target.GetPmUsersForAllBusinessUnits(JobStepName, createJobResultCollectionWithPmUsers(), pmUserCurrentBuCollection);

            Assert.IsTrue(pmUsers.Contains(_userAb));
            Assert.IsFalse(pmUsers.Contains(_userAb2));
            Assert.IsTrue(pmUsers.Contains(_userCd));
            Assert.IsFalse(pmUsers.Contains(_userCd2));
            Assert.IsTrue(pmUsers.Contains(_userEf));
            Assert.IsFalse(pmUsers.Contains(_userEf2));
            Assert.AreEqual(3, pmUsers.Count);
        }

        [Test]
        public void ShouldSynchronizeUserPermissionForOneUser()
        {
            var mocks = new MockRepository();
            var pmProxyFactory = mocks.StrictMock<IPmProxyFactory>();
            var pmProxy = mocks.StrictMock<IPmProxy>();
            _target = new PmPermissionTransformer(pmProxyFactory);
            var userCollection = new List<UserDto> { new UserDto { UserName = "name", AccessLevel = 1, IsWindowsLogOn = false } };
            UserDto[] userArray = userCollection.ToArray();
            var result = new ResultDto { Success = true };

            using (mocks.Record())
            {
                Expect.Call(pmProxyFactory.CreateProxy()).Return(pmProxy);
                Expect.Call(() => pmProxy.ResetUserLists());
                Expect.Call(pmProxy.AddUsersToSynchronize(userArray)).Return(result).Repeat.Once();
                Expect.Call(pmProxy.SynchronizeUsers("olaServer", "olapDb")).Return(result).Repeat.Once();
                Expect.Call(() => pmProxy.ResetUserLists());
                pmProxy.Close();
                pmProxy.Abort();
            }
            using (mocks.Playback())
            {
                _target.SynchronizeUserPermissions(userCollection, "olaServer", "olapDb");
            }
        }

        [Test]
        public void ShouldSynchronizeUserPermissionForMoreThan100Users()
        {
            var mocks = new MockRepository();
            var pmProxyFactory = mocks.StrictMock<IPmProxyFactory>();
            var pmProxy = mocks.StrictMock<IPmProxy>();
            _target = new PmPermissionTransformer(pmProxyFactory);
            var userCollection = createLotsOfUsers(210);
            var result = new ResultDto { Success = true };

            using (mocks.Record())
            {
                Expect.Call(pmProxyFactory.CreateProxy()).Return(pmProxy);
                Expect.Call(() => pmProxy.ResetUserLists());
                foreach (IEnumerable<UserDto> userDtos in userCollection.Batch(100))
                {
                    Expect.Call(pmProxy.AddUsersToSynchronize(userDtos.ToArray())).Return(result);
                }
                Expect.Call(pmProxy.SynchronizeUsers("olaServer", "olapDb")).Return(result);
                Expect.Call(() => pmProxy.ResetUserLists());
                pmProxy.Close();
                pmProxy.Abort();
            }
            using (mocks.Playback())
            {
                _target.SynchronizeUserPermissions(userCollection, "olaServer", "olapDb");
            }
        }

        [Test]
        public void ShouldStopSynchronizingUserPermissionWhenOneBatchIsNotSuccessful()
        {
            var mocks = new MockRepository();
            var pmProxyFactory = mocks.StrictMock<IPmProxyFactory>();
            var pmProxy = mocks.StrictMock<IPmProxy>();
            _target = new PmPermissionTransformer(pmProxyFactory);
            IList<UserDto> userCollection1 = createLotsOfUsers(100);
            var userCollectionConcat = new List<UserDto>(userCollection1);
            userCollectionConcat.AddRange(createLotsOfUsers(10));

            var result = new ResultDto { Success = false };

            using (mocks.Record())
            {
                Expect.Call(pmProxyFactory.CreateProxy()).Return(pmProxy);
                Expect.Call(() => pmProxy.ResetUserLists());
                Expect.Call(pmProxy.AddUsersToSynchronize(userCollection1.ToArray())).Return(result).Repeat.Once();

                pmProxy.Abort();
            }
            using (mocks.Playback())
            {
                _target.SynchronizeUserPermissions(userCollectionConcat, "olaServer", "olapDb");
                Assert.IsFalse(result.Success);
            }
        }

        [Test, ExpectedException(typeof(PmSynchronizeException))]
        public void VerifySynchronizeUserPermissionsException()
        {
            var mocks = new MockRepository();
            var pmProxyFactory = mocks.StrictMock<IPmProxyFactory>();
            var pmProxy = mocks.StrictMock<IPmProxy>();
            _target = new PmPermissionTransformer(pmProxyFactory);
            var userCollection = new List<UserDto> { new UserDto { UserName = "name", AccessLevel = 1, IsWindowsLogOn = false } };
            UserDto[] userArray = userCollection.ToArray();

            using (mocks.Record())
            {
                Expect.Call(pmProxyFactory.CreateProxy()).Return(pmProxy);
                Expect.Call(() => pmProxy.ResetUserLists());
                Expect.Call(pmProxy.AddUsersToSynchronize(userArray)).Return(new ResultDto { Success = true }).Repeat.Once();
                Expect.Call(pmProxy.SynchronizeUsers("olaServer", "olapDb")).Throw(new PmSynchronizeException("Only a sample exception..."));
                pmProxy.Abort();
            }
            using (mocks.Playback())
            {
                _target.SynchronizeUserPermissions(userCollection, "olaServer", "olapDb");
            }
        }

        private IList<IJobResult> createJobResultCollectionWithPmUsers()
        {
            IList<IJobResult> returnCollection = new List<IJobResult>();

            // Create BUs
            IBusinessUnit bu1 = new BusinessUnit("bu1");
            bu1.SetId(Guid.NewGuid());
            IBusinessUnit bu2 = new BusinessUnit("bu2");
            bu2.SetId(Guid.NewGuid());
            IBusinessUnit bu3 = new BusinessUnit("bu3");
            bu3.SetId(Guid.NewGuid());

            // Create user collection
            _userAb2 = new UserDto { AccessLevel = 1, UserName = "a\\b" };  // pmUser=false
            _userCd = new UserDto { AccessLevel = 2, UserName = "c\\d" };   // pmUser=true
            _userEf = new UserDto { AccessLevel = 2, UserName = "e\\f" };   // pmUser=true
            IList<UserDto> pmUserCollection = new List<UserDto> { _userAb2, _userCd, _userEf };

            // Create JobResults/JobStepResults with no PM users
            IJobResult jobResult1 = new JobResult(bu1, returnCollection);
            jobResult1.JobStepResultCollection.Add(new JobStepResult(JobStepName, 0, 0, bu1, returnCollection));
            returnCollection.Add(jobResult1);

            // Create JobResults/JobStepResults with two PM users
            IJobStepResult jobStepResult2 = new JobStepResult(JobStepName, 0, 0, bu2, returnCollection);
            jobStepResult2.SetPmUsersForCurrentBusinessUnit(pmUserCollection);
            IJobResult jobResult2 = new JobResult(bu2, returnCollection);
            jobResult2.JobStepResultCollection.Add(jobStepResult2);
            returnCollection.Add(jobResult2);

            // Create JobResults/JobStepResults with no PM users
            IJobStepResult jobStepResult3 = new JobStepResult(JobStepName, 0, 0, bu3, returnCollection);
            jobStepResult3.SetPmUsersForCurrentBusinessUnit(new List<UserDto>());
            IJobResult jobResult3 = new JobResult(bu3, returnCollection);
            jobResult3.JobStepResultCollection.Add(jobStepResult3);
            returnCollection.Add(jobResult3);

            return returnCollection;
        }

        [Test]
        public void VerifyTransform()
        {
            using (var table = new DataTable())
            {
                var windowsUserWithViewPermission = new UserDto { UserName = @"domain1\kalle", AccessLevel = 1 };
                var windowsUserWithCreatePermission = new UserDto { UserName = @"domain1\lotta", AccessLevel = 2 };
                var applicationUserWithViewPermission = new UserDto { UserName = "CarlH", AccessLevel = 2 };
                IList<UserDto> users = new List<UserDto> { windowsUserWithViewPermission, windowsUserWithCreatePermission, applicationUserWithViewPermission };

                table.Locale = Thread.CurrentThread.CurrentCulture;
                PmUserInfrastructure.AddColumnsToDataTable(table);
                _target.Transform(users, table);

                Assert.IsNotNull(table);
                Assert.AreEqual(3, table.Rows.Count);
                Assert.AreEqual(windowsUserWithViewPermission.UserName, table.Rows[0]["user_name"]);
                Assert.AreEqual(windowsUserWithCreatePermission.UserName, table.Rows[1]["user_name"]);
                Assert.AreEqual(applicationUserWithViewPermission.UserName, table.Rows[2]["user_name"]);
            }
        }

        [Test]
        public void ShouldGetTheMostGenerousPermissionOutOfThreeRoles()
        {
            // Person has 3 roles with following PM permissions. 1st create, 2nd view, 3rd none 
            IPerson person = _personCollection[6];
            var applicationRole1 = person.PermissionInformation.ApplicationRoleCollection[0];
            var applicationRole2 = person.PermissionInformation.ApplicationRoleCollection[1];
            var applicationRole3 = person.PermissionInformation.ApplicationRoleCollection[2];

            Expect.Call(_permissionExtractor.ExtractPermission(applicationRole1.ApplicationFunctionCollection,_unitOfWorkFactory)).Return(PmPermissionType.ReportDesigner);
            Expect.Call(_permissionExtractor.ExtractPermission(applicationRole2.ApplicationFunctionCollection,_unitOfWorkFactory)).Return(PmPermissionType.GeneralUser);
            Expect.Call(_permissionExtractor.ExtractPermission(applicationRole3.ApplicationFunctionCollection,_unitOfWorkFactory)).Return(PmPermissionType.None);
            _mocks.ReplayAll();

            IList<UserDto> users = _target.GetUsersWithPermissionsToPerformanceManager(new List<IPerson> { person }, false, _permissionExtractor, _unitOfWorkFactory);
            _mocks.VerifyAll();

            Assert.AreEqual(1, users.Count);
            Assert.AreEqual(2, users[0].AccessLevel); // AccessLevel = 2 is Create
        }

        [Test]
        public void ShouldJoinSeveralResultsToOneResult()
        {
            var resultDto1 = new ResultDto { AffectedUsersCount = 3, Success = true };
            var resultDto2 = new ResultDto { AffectedUsersCount = 3, Success = true };
            resultDto1.AddRangeUsersInserted(new List<UserDto> { new UserDto { UserName = "A/I" } });
            resultDto1.AddRangeUsersUpdated(new List<UserDto> { new UserDto { UserName = "A/U" } });
            resultDto1.AddRangeUsersDeleted(new List<UserDto> { new UserDto { UserName = "A/D" } });
            resultDto2.AddRangeUsersInserted(new List<UserDto> { new UserDto { UserName = "B/I" } });
            resultDto2.AddRangeUsersUpdated(new List<UserDto> { new UserDto { UserName = "B/U" } });
            resultDto2.AddRangeUsersDeleted(new List<UserDto> { new UserDto { UserName = "B/D" } });

            ResultDto joinedResult = _target.JoinResults(new List<ResultDto> { resultDto1, resultDto2 });

            Assert.AreEqual(2, joinedResult.UsersInserted.Count);
            Assert.AreEqual(2, joinedResult.UsersUpdated.Count);
            Assert.AreEqual(2, joinedResult.UsersDeleted.Count);
            Assert.AreEqual(6, joinedResult.AffectedUsersCount);
            Assert.IsNull(joinedResult.ErrorMessage);
            Assert.IsNull(joinedResult.ErrorType);
            Assert.IsTrue(joinedResult.Success);
        }

        [Test]
        public void ShouldReturnEmptySuccessResultWhenTryingToJoinZeroResults()
        {
            ResultDto joinedResult = _target.JoinResults(new List<ResultDto>());

            Assert.AreEqual(0, joinedResult.UsersInserted.Count);
            Assert.AreEqual(0, joinedResult.UsersUpdated.Count);
            Assert.AreEqual(0, joinedResult.UsersDeleted.Count);
            Assert.AreEqual(0, joinedResult.AffectedUsersCount);
            Assert.IsNull(joinedResult.ErrorMessage);
            Assert.IsNull(joinedResult.ErrorType);
            Assert.IsTrue(joinedResult.Success);
        }

        private static List<UserDto> createLotsOfUsers(int numberOfUsers)
        {
            List<UserDto> userCollection = new List<UserDto>();
            for (int i = 1; i <= numberOfUsers; i++)
            {
                userCollection.Add(new UserDto { UserName = "user " + i, AccessLevel = 1 });
            }

            return userCollection;
        }
    }
}
