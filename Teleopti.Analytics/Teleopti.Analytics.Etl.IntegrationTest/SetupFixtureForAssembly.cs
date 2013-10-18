//using System;
//using System.IO;
//using NUnit.Framework;
//using Teleopti.Ccc.Domain.Security;
//using Teleopti.Ccc.Domain.Security.Principal;
//using Teleopti.Ccc.Infrastructure.Foundation;
//using Teleopti.Ccc.Infrastructure.Repositories;
//using Teleopti.Ccc.Infrastructure.UnitOfWork;
//using Teleopti.Ccc.TestCommon;
//using Teleopti.Ccc.TestCommon.FakeData;
//using Teleopti.Ccc.TestCommon.TestData.Core;
//using Teleopti.Interfaces.Infrastructure;

//namespace Teleopti.Analytics.Etl.IntegrationTest
//{
//    [SetUpFixture]
//    public class SetupFixtureForAssembly
//    {
//        private ICurrentUnitOfWorkFactory _unitOfWorkFactory;

//        private void UnitOfWorkAction(Action<IUnitOfWork> action)
//        {
//            using (var uow = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
//            {
//                action(uow);
//                uow.PersistAll();
//            }
//        }

//        //[SetUp]
//        //public void Setup()
//        //{
//        //    var dataSource = DataSourceHelper.CreateDataSource(new IMessageSender[]{}, "TestData");

//        //    var personThatCreatesTestData = PersonFactory.CreatePersonWithBasicPermissionInfo("UserThatCreatesTestData", "password");

//        //    var businessUnitFromFakeState = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
//        //    businessUnitFromFakeState.Name = "BusinessUnit";

//        //    StateHolderProxyHelper.SetupFakeState(dataSource, personThatCreatesTestData, businessUnitFromFakeState, new ThreadPrincipalContext(new TeleoptiPrincipalFactory()));

//        //    _unitOfWorkFactory = UnitOfWorkFactory.CurrentUnitOfWorkFactory();

//        //    //DataSourceHelper.PersistAuditSetting();// TODO: Remove, its done in DataSourceHelper.CreateDataSource();

//        //    var dataFactory = new DataFactory(UnitOfWorkAction);

//        //    UnitOfWorkAction(uow =>
//        //        {

//        //            var personRepository = new PersonRepository(uow);
//        //            personRepository.Add(personThatCreatesTestData);

//        //            var license = new License { XmlString = File.ReadAllText("License.xml") };
//        //            var licenseRepository = new LicenseRepository(uow);
//        //            licenseRepository.Add(license);

//        //        });





//        //}
//    }
//}
