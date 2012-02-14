using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.AuthorizationSteps;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security
{
    internal class AuthorizationEntityProviderTestClass
        : IAuthorizationEntityProvider<AuthorizationEntity>
    {

        private IList<IAuthorizationEntity> _parentEntityList;
        private IList<AuthorizationEntity> _resultEntityList;
        

        public void setResultEntityList(IList<AuthorizationEntity> list)
        {
            _resultEntityList = list;
        }

        #region IAuthorizationEntityProvider<AuthorizationEntity> Members

        public IList<AuthorizationEntity> ResultEntityList
        {
            get 
            {
                return _resultEntityList; 
            }
        }

        public IList<IAuthorizationEntity> InputEntityList
        {
            set { _parentEntityList = value ; }
        }

        #endregion
    }
}