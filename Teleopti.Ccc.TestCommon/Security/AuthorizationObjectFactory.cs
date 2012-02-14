using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.Security
{
    public static class AuthorizationObjectFactory
    {
        /// <summary>
        /// Builds the expected Authorization Entity list.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public static IList<IAuthorizationEntity> CreateAuthorizationEntityList()
        {
            IList<IAuthorizationEntity> list = new List<IAuthorizationEntity>();
            list.Add(new AuthorizationEntity("KeyField", "NameField", "InfoField", "ValueField"));
            list.Add(new AuthorizationEntity("AnotherKeyField", "AnotherNameField" ,"AnotherInfoField", "AnotherValueField"));
            return list;
        }

        /// <summary>
        /// Builds the expected Authorization Foreign Entity list.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public static IList<IAuthorizationEntity> CreateAuthorizationForeignEntityList()
        {
            IList<IAuthorizationEntity> list = new List<IAuthorizationEntity>();
            list.Add(new AuthorizationForeignEntityTestClass("KeyFiled", "NameField", "InfoField", "ValueField"));
            list.Add(new AuthorizationForeignEntityTestClass("AnotherKeyField", "AnotherNameField", "AnotherInfoField", "AnotherValueField"));
            return list;
        }

        /// <summary>
        /// Builds an otherexpected Authorization Entity list. It has the param as the first value.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/27/2007
        /// </remarks>
        public static IList<IAuthorizationEntity> CreateAuthorizationEntityListWithThisFirstItem(IAuthorizationEntity firstItem)
        {
            IList<IAuthorizationEntity> list = new List<IAuthorizationEntity>();
            list.Add(firstItem);
            list.Add(new AuthorizationEntity("YetAnotherKeyField", "YetAnotherNameField", "YetAnotherInfoField", "YetAnotherValueField"));
            return list;
        }


        /// <summary>
        /// Creates the test steps structure with two steps with result lists that are
        /// overlapping each other.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 11/28/2007
        /// </remarks>
        public static IList<AuthorizationStepTestClass> CreateTestAuthorizationStepsStructure()
        {

            AuthorizationStepTestClass firstStep;
            AuthorizationStepTestClass secondStep;

            IList<IAuthorizationEntity> firstList = AuthorizationObjectFactory.CreateAuthorizationEntityList();
            firstStep = new AuthorizationStepTestClass("FirstParentStep");
            firstStep.SetRefreshOwnListResult(firstList);

            IList<IAuthorizationEntity> secondList = AuthorizationObjectFactory.CreateAuthorizationEntityListWithThisFirstItem(firstList[0]);
            secondStep = new AuthorizationStepTestClass("SecondParentStep");
            secondStep.SetRefreshOwnListResult(secondList);

            IList<AuthorizationStepTestClass> resultList = new List<AuthorizationStepTestClass>();
            resultList.Add(firstStep);
            resultList.Add(secondStep);
            return resultList;
        }
    }
}
