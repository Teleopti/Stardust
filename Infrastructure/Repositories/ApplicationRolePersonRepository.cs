using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class ApplicationRolePersonRepository
    {
        private readonly IStatelessUnitOfWork _unitOfWork;

        public ApplicationRolePersonRepository(IStatelessUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IList<IPersonInRole> GetPersonsInRole(Guid roleId)
        {
            var onDate = DateTime.Today.Date;
            const string query = @"SELECT p.Id, FirstName, LastName , ISNULL(t.Name, '') Team
                        FROM Person p INNER JOIN PersonInApplicationRole a ON p.Id = a.Person 
                        AND a.ApplicationRole = :role
                        AND BuiltIn = 0
                        LEFT JOIN PersonPeriodWithEndDate ON Parent = p.Id
                        AND StartDate <= :onDate AND EndDate >= :onDate
                        LEFT JOIN Team t ON t.Id = Team";
            return ((NHibernateStatelessUnitOfWork)_unitOfWork).Session.CreateSQLQuery(query)
                    .SetGuid("role", roleId)
                    .SetDateTime("onDate", onDate)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(PersonInRole)))
                    .SetReadOnly(true)
                    .List<IPersonInRole>();
        }

        public IList<IPersonInRole> GetPersonsNotInRole(Guid roleId, ICollection<Guid> personsIds)
        {
            var onDate = DateTime.Today.Date;
            var ids = new List<Guid>();
            ids.AddRange(personsIds);
            const string query = @"SELECT p.Id, FirstName, LastName , ISNULL(t.Name, '') Team
                        FROM Person p LEFT JOIN PersonPeriodWithEndDate ON Parent = p.Id
                        AND StartDate <= :onDate AND EndDate >= :onDate
                        LEFT JOIN Team t ON t.Id = Team
                        WHERE  p.Id In (:persons) AND BuiltIn = 0
                        AND p.Id Not IN (SELECT Person FROM PersonInApplicationRole WHERE ApplicationRole = :role)";
            return ((NHibernateStatelessUnitOfWork)_unitOfWork).Session.CreateSQLQuery(query)
                    .SetGuid("role", roleId)
                    .SetDateTime("onDate", onDate)
                    .SetParameterList("persons", ids)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(PersonInRole)))
                    .SetReadOnly(true)
                    .List<IPersonInRole>();
        }
    }

    public interface IPersonInRole :IEquatable<IPersonInRole>
    {
        Guid Id { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Team { get; set; }
    }

    public class PersonInRole : IPersonInRole
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Team { get; set; }

        public bool Equals(IPersonInRole other)
        {
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}