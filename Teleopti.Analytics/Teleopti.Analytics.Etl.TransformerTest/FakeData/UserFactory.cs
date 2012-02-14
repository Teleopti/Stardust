using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.AgentInfo;
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
            person.PermissionInformation.ApplicationAuthenticationInfo.ApplicationLogOnName = "karl";
            person.PermissionInformation.ApplicationAuthenticationInfo.Password = "secret";
            person.PermissionInformation.WindowsAuthenticationInfo.WindowsLogOnName = "winLogon";
            person.PermissionInformation.WindowsAuthenticationInfo.DomainName = "myDomain";
            person.Email = "kalle.kula@myDomain.com";
            person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo(1033));
            person.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo(1053));
            RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);
            retList.Add(person);

            person = Ccc.TestCommon.FakeData.PersonFactory.CreatePerson("pelle", "pilla");
            person.SetId(Guid.NewGuid());
            person.PermissionInformation.ApplicationAuthenticationInfo.ApplicationLogOnName = "perra";
            person.PermissionInformation.ApplicationAuthenticationInfo.Password = "ts";
            person.PermissionInformation.WindowsAuthenticationInfo.WindowsLogOnName = "pepi";
            person.PermissionInformation.WindowsAuthenticationInfo.DomainName = "Domain1";
            person.Email = "pella.pilla@Domain1.com";
            //person.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo(1033));
            //person.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo(1053));
            RaptorTransformerHelper.SetUpdatedOn(person, DateTime.Now);
            retList.Add(person);

            return retList;
        }
    }
}