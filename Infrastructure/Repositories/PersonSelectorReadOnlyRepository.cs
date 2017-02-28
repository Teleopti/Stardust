using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class PersonSelectorReadOnlyRepository : IPersonSelectorReadOnlyRepository
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public PersonSelectorReadOnlyRepository(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public IList<IPersonSelectorOrganization> GetOrganization(DateOnlyPeriod dateOnlyPeriod, bool loadUsers)
        {
            int cultureId = TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture.LCID;
            return _unitOfWorkFactory.Session().CreateSQLQuery(
                    "exec ReadModel.LoadOrganizationForSelector @type=:type,  @ondate=:ondate,@enddate=:enddate, @bu=:bu, @users=:users, @culture=:culture")
                    .SetString("type", "Organization")
					.SetDateOnly("ondate", dateOnlyPeriod.StartDate)
					.SetDateOnly("enddate", dateOnlyPeriod.EndDate)
                    .SetGuid("bu", ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.GetValueOrDefault())
                    .SetBoolean("users", loadUsers)
                    .SetInt32("culture", cultureId)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(PersonSelectorOrganization)))
                    .SetReadOnly(true)
                    .List<IPersonSelectorOrganization>();   
        }

        public IList<IPersonSelectorBuiltIn> GetBuiltIn(DateOnlyPeriod dateOnlyPeriod, PersonSelectorField loadType)
        {
            int cultureId = TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture.LCID;
            return _unitOfWorkFactory.Session().CreateSQLQuery(
                    "exec ReadModel.LoadOrganizationForSelector @type=:type,  @ondate=:ondate,@enddate=:enddate, @bu=:bu, @users=:users, @culture=:culture")
                    .SetString("type", loadType.ToString())
					.SetDateOnly("ondate", dateOnlyPeriod.StartDate)
					.SetDateOnly("enddate", dateOnlyPeriod.EndDate)
                    .SetGuid("bu", ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.GetValueOrDefault())
                    .SetBoolean("users", false)
                    .SetInt32("culture", cultureId)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(PersonSelectorBuiltIn)))
                    .SetReadOnly(true)
                    .List<IPersonSelectorBuiltIn>();
        }

        public IList<IPersonSelectorUserDefined> GetUserDefinedTab(DateOnly onDate, Guid value)
        {
            int cultureId = TeleoptiPrincipal.CurrentPrincipal.Regional.UICulture.LCID;
            return _unitOfWorkFactory.Session().CreateSQLQuery(
                    "exec ReadModel.LoadUserDefinedTab @tabid=:tabid, @bu=:bu,  @ondate=:ondate, @culture=:culture")
                    .SetGuid("tabid", value)
					.SetGuid("bu", ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.GetValueOrDefault())
					.SetDateOnly("ondate", onDate)
                    .SetInt32("culture", cultureId)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(PersonSelectorUserDefined)))
                    .SetReadOnly(true)
                    .List<IPersonSelectorUserDefined>();
        }

        public IList<IUserDefinedTabLight> GetUserDefinedTabs()
        {
            return _unitOfWorkFactory.Session().CreateSQLQuery(
                    "SELECT Id, Name FROM GroupPage WHERE IsDeleted = 0 AND BusinessUnit = :bu")
					.SetGuid("bu", ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.GetValueOrDefault())
                    .SetResultTransformer(Transformers.AliasToBean(typeof(UserDefinedTabLight)))
                    .SetReadOnly(true)
                    .List<IUserDefinedTabLight>();   
        }
    }

	public class PersonSelectorOrganization : IPersonSelectorOrganization
    {
        public Guid PersonId { get; set; }

        public Guid? TeamId { get; set; }
        public Guid? SiteId { get; set; }
        public Guid BusinessUnitId { get; set; }

        public string Team { get; set; }
        public string Site { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmploymentNumber { get; set; }
    }
    public class PersonSelectorBuiltIn : IPersonSelectorBuiltIn
    {
        public Guid PersonId { get; set; }

        public Guid? TeamId { get; set; }
        public Guid? SiteId { get; set; }
        public Guid BusinessUnitId { get; set; }

        public string Node { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmploymentNumber { get; set; }
    }

    public class PersonSelectorUserDefined : IPersonSelectorUserDefined
    {
        public Guid PersonId { get; set; }

        public Guid? TeamId { get; set; }
        public Guid? SiteId { get; set; }
        public Guid BusinessUnitId { get; set; }

        public Guid NodeId { get; set; }
        public string Node { get; set; }

        public Guid? ParentId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmploymentNumber { get; set; }

        public int Level { get; set; }
		public bool Show { get; set; }
    }

    public class UserDefinedTabLight : IUserDefinedTabLight
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}

