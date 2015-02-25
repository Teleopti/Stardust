using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
    public class NonVersionedRootTest : DatabaseTest
    {
        [Test]
        public void VerifyNoOptimisticLock()
        {
            CleanUpAfterTest();
            ISettingData setting = new PersonalSettingData("nyckel", ((IUnsafePerson)TeleoptiPrincipal.Current).Person);
            try
            {

                Session.Save(setting);
                UnitOfWork.PersistAll();
                using (IUnitOfWork uow2 = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
                {
                    PersonalSettingDataRepository rep2 = new PersonalSettingDataRepository(uow2);

                    ISettingData loaded2 = rep2.FindByKey("nyckel");

                    setting.SetValue(new dummy());
                    loaded2.SetValue(new dummy());
                    Assert.AreNotSame(setting, loaded2);
                    UnitOfWork.PersistAll();
                    //this shouldn't throw
                    uow2.PersistAll();
                }

            }
            finally
            {
                using(IUnitOfWork uowTemp = SetupFixtureForAssembly.DataSource.Application.CreateAndOpenUnitOfWork())
                {
                    new Repository(uowTemp).Remove(setting); //hack
                    uowTemp.PersistAll();                    
                }
            }
        }

        [Serializable]
        private class dummy : SettingValue{}
    }
}
