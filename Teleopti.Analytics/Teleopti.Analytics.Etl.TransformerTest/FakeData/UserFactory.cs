using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.FakeData
{
    public static class UserFactory
    {
        public static IList<IPerson> CreatePersonUserCollection()
        {
            IList<IPerson> retList = new List<IPerson>();

            IPerson person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("kalle", "kula");
            person.SetId(Guid.NewGuid());
            person.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo
                                                       {ApplicationLogOnName = "karl", Password = "secret"};
            
            person.WindowsAuthenticationInfo = new WindowsAuthenticationInfo
                                                   {WindowsLogOnName = "winLogon", DomainName = "myDomain"};
            person.Email = "kalle.kula@myDomain.com";
            person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo(1033));
            person.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo(1053));
            RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);
            retList.Add(person);

            person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("pelle", "pilla");
            person.SetId(Guid.NewGuid());
            person.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo
                                                       {ApplicationLogOnName = "perra", Password = "ts"};
                
            person.WindowsAuthenticationInfo = new WindowsAuthenticationInfo { WindowsLogOnName = "pepi", DomainName = "Domain1" }; 
            person.Email = "pella.pilla@Domain1.com";
            //person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo(1033));
            //person.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo(1053));
            RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);
            retList.Add(person);

            return retList;
        }
    }
}