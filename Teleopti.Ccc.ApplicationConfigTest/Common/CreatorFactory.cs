using System.Globalization;
using NHibernate;
using Teleopti.Ccc.ApplicationConfig.Creators;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfigTest.Common
{
    public static class CreatorFactory
    {        
        public static IBusinessUnit CreateBusinessUnit(string name,IPerson person, ISessionFactory sessionFactory)
        {
            BusinessUnitCreator businessUnitCreator = new BusinessUnitCreator(person, sessionFactory);
            IBusinessUnit businessUnit = businessUnitCreator.Create(name);
            businessUnitCreator.Save(businessUnit);
            return businessUnit;
        }

        public static IApplicationRole CreateApplicationRole(IPerson person, IBusinessUnit businessUnit, ISessionFactory sessionFactory)
        {
            ApplicationRoleCreator applicationRoleCreator = new ApplicationRoleCreator(person, businessUnit, sessionFactory);
            IApplicationRole applicationRole = applicationRoleCreator.Create("Test", "Tuna", false);
            applicationRoleCreator.Save(applicationRole);
            return applicationRole;
        }
    }
}