using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class ApplicationRolePersonRepository : IApplicationRolePersonRepository
    {
	    private readonly IStatelessUnitOfWork _unitOfWork;

        public ApplicationRolePersonRepository(IStatelessUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

	    public IList<IPersonInRole> GetPersonsInRole(Guid roleId)
        {
            var onDate = DateTime.Today;
            const string query = @"SELECT p.Id, FirstName, LastName , ISNULL(t.Name, '') Team
                        FROM Person p INNER JOIN PersonInApplicationRole a ON p.Id = a.Person 
                        AND a.ApplicationRole = :role
                        LEFT JOIN PersonPeriod ON Parent = p.Id
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
            var onDate = DateTime.Today;
            var ids = new List<Guid>();
            ids.AddRange(personsIds);
            const string query = @"SELECT p.Id, FirstName, LastName , ISNULL(t.Name, '') Team
                        FROM Person p LEFT JOIN PersonPeriod ON Parent = p.Id
                        AND StartDate <= :onDate AND EndDate >= :onDate
                        LEFT JOIN Team t ON t.Id = Team
                        WHERE  p.Id In (:persons)
                        AND p.Id Not IN (SELECT Person FROM PersonInApplicationRole WHERE ApplicationRole = :role)";
            return ((NHibernateStatelessUnitOfWork)_unitOfWork).Session.CreateSQLQuery(query)
                    .SetGuid("role", roleId)
                    .SetDateTime("onDate", onDate)
                    .SetParameterList("persons", ids)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(PersonInRole)))
                    .SetReadOnly(true)
                    .List<IPersonInRole>();
        }

        public IList<IPersonInRole> Persons()
        {
            var onDate = DateTime.Today;
            const string query = @"SELECT p.Id, FirstName, LastName , ISNULL(t.Name, '') Team
                        FROM Person p LEFT JOIN PersonPeriod ON Parent = p.Id
                        AND StartDate <= :onDate AND EndDate >= :onDate
                        LEFT JOIN Team t ON t.Id = Team";
            return ((NHibernateStatelessUnitOfWork)_unitOfWork).Session.CreateSQLQuery(query)
                    .SetDateTime("onDate", onDate)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(PersonInRole)))
                    .SetReadOnly(true)
                    .List<IPersonInRole>();
        }

        public IList<IRoleLight> RolesOnPerson(Guid selectedPerson)
        {
            const string query = @"SELECT r.Id, r.DescriptionText AS Name FROM ApplicationRole r
                                WHERE Id IN(SELECT ApplicationRole FROM PersonInApplicationRole 
                                WHERE Person = :person)
                                AND BuiltIn = 0 AND IsDeleted = 0";
            return ((NHibernateStatelessUnitOfWork)_unitOfWork).Session.CreateSQLQuery(query)
                    .SetGuid("person", selectedPerson)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(RoleLight)))
                    .SetReadOnly(true)
                    .List<IRoleLight>();
        }

        public IList<IFunctionLight> FunctionsOnPerson(Guid selectedPerson)
        {
            const string query = @"SELECT f.Id, FunctionCode AS Name, Parent, ForeignSource, FunctionDescription AS ResourceName, r.DescriptionText AS Role FROM ApplicationFunction f
                                INNER JOIN ApplicationFunctionInRole fir ON f.Id = fir.ApplicationFunction AND f.IsDeleted = 0
                                INNER JOIN ApplicationRole r ON r.Id = fir.ApplicationRole AND BuiltIn = 0 AND r.IsDeleted = 0
                                WHERE ApplicationRole IN(SELECT ApplicationRole FROM PersonInApplicationRole 
                                WHERE Person = :person)";
            return ((NHibernateStatelessUnitOfWork)_unitOfWork).Session.CreateSQLQuery(query)
                    .SetGuid("person", selectedPerson)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(FunctionLight)))
                    .SetReadOnly(true)
                    .List<IFunctionLight>();
        }

        public IList<IFunctionLight> Functions()
        {
            const string query = @"SELECT Id, FunctionCode AS Name, Parent, ForeignSource, FunctionDescription AS ResourceName, '' AS Role FROM ApplicationFunction
                                    WHERE IsDeleted = 0";
            return ((NHibernateStatelessUnitOfWork)_unitOfWork).Session.CreateSQLQuery(query)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(FunctionLight)))
                    .SetReadOnly(true)
                    .List<IFunctionLight>();
        }

        public IList<IPersonInRole> PersonsWithFunction(Guid selectedFunction)
        {
            var onDate = DateTime.Today;
            const string query = @"SELECT DISTINCT p.Id, FirstName, LastName , ISNULL(t.Name, '') Team
                                FROM Person p INNER JOIN PersonInApplicationRole a ON p.Id = a.Person AND IsDeleted = 0
                                AND ApplicationRole IN(SELECT ApplicationRole FROM ApplicationFunctionInRole WHERE ApplicationFunction = :function)
                                LEFT JOIN PersonPeriod ON Parent = p.Id
                                AND StartDate <= :onDate AND EndDate >= :onDate
                                LEFT JOIN Team t ON t.Id = Team";
            return ((NHibernateStatelessUnitOfWork)_unitOfWork).Session.CreateSQLQuery(query)
                    .SetDateTime("onDate", onDate)
                    .SetGuid("function", selectedFunction)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(PersonInRole)))
                    .SetReadOnly(true)
                    .List<IPersonInRole>();
        }

        public IList<IRoleLight> RolesWithFunction(Guid selectedFunction)
        {
            const string query = @"SELECT Id, DescriptionText as Name FROM ApplicationRole
                                 WHERE Id IN (SELECT ApplicationRole FROM ApplicationFunctionInRole WHERE ApplicationFunction = :function)
                                 AND BuiltIn = 0 AND IsDeleted = 0";
            return ((NHibernateStatelessUnitOfWork)_unitOfWork).Session.CreateSQLQuery(query)
                    .SetGuid("function", selectedFunction)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(RoleLight)))
                    .SetReadOnly(true)
                    .List<IRoleLight>();
        }

        public IList<Guid> AvailableData(Guid selectedPerson)
        {
            const string query = @"SELECT DISTINCT AvailableBusinessUnit AS Available
                                FROM AvailableUnitsInApplicationRole t
                                INNER JOIN AvailableData d ON d.Id = t.AvailableData
                                WHERE d.ApplicationRole IN(SELECT ApplicationRole FROM PersonInApplicationRole WHERE Person = :person)
                                UNION
                                SELECT DISTINCT AvailableSite
                                FROM dbo.AvailableSitesInApplicationRole t
                                INNER JOIN AvailableData d ON d.Id = t.AvailableData
                                WHERE d.ApplicationRole IN(SELECT ApplicationRole FROM PersonInApplicationRole WHERE Person = :person)
                                UNION
                                SELECT DISTINCT AvailableTeam 
                                FROM dbo.AvailableTeamsInApplicationRole t
                                INNER JOIN AvailableData d ON d.Id = t.AvailableData
                                WHERE d.ApplicationRole IN(SELECT ApplicationRole FROM PersonInApplicationRole WHERE Person = :person)";
            return ((NHibernateStatelessUnitOfWork)_unitOfWork).Session.CreateSQLQuery(query)
                    .SetGuid("person", selectedPerson)
                    .SetReadOnly(true)
                    .List<Guid>();
        }

        public IList<int> DataRangeOptions(Guid selectedPerson)
        {
            const string query = @"SELECT AvailableDataRange FROM AvailableData
                                WHERE ApplicationRole IN(SELECT ApplicationRole FROM PersonInApplicationRole WHERE Person = :person)";
            return ((NHibernateStatelessUnitOfWork)_unitOfWork).Session.CreateSQLQuery(query)
                    .SetGuid("person", selectedPerson)
                    .SetReadOnly(true)
                    .List<int>();
        }

        public IList<IPersonInRole> PersonsWithRoles(IList<Guid> roles)
        {
            var onDate = DateTime.Today.Date;
            const string query = @"SELECT p.Id, FirstName, LastName , ISNULL(t.Name, '') Team
                        FROM Person p INNER JOIN PersonInApplicationRole a ON p.Id = a.Person 
                        AND a.ApplicationRole IN( :roles )
                        AND IsDeleted = 0
                        LEFT JOIN PersonPeriod ON Parent = p.Id
                        AND StartDate <= :onDate AND EndDate >= :onDate
                        LEFT JOIN Team t ON t.Id = Team";
            return ((NHibernateStatelessUnitOfWork)_unitOfWork).Session.CreateSQLQuery(query)
                    .SetParameterList("roles", roles)
                    .SetDateTime("onDate", onDate)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(PersonInRole)))
                    .SetReadOnly(true)
                    .List<IPersonInRole>();
        }

        public IList<IRoleLight> RolesWithData(Guid id)
        {
            const string query = @"SELECT Id, DescriptionText as Name FROM ApplicationRole WHERE Id IN(
                                SELECT DISTINCT ApplicationRole 
                                FROM AvailableUnitsInApplicationRole t
                                INNER JOIN AvailableData d ON d.Id = t.AvailableData
                                WHERE t.AvailableBusinessUnit = :data
                                UNION
                                SELECT DISTINCT ApplicationRole
                                FROM dbo.AvailableSitesInApplicationRole t
                                INNER JOIN AvailableData d ON d.Id = t.AvailableData
                                WHERE t.AvailableSite = :data
                                UNION
                                SELECT DISTINCT ApplicationRole 
                                FROM dbo.AvailableTeamsInApplicationRole t
                                INNER JOIN AvailableData d ON d.Id = t.AvailableData
                                WHERE t.AvailableTeam = :data)";
            return ((NHibernateStatelessUnitOfWork)_unitOfWork).Session.CreateSQLQuery(query)
                    .SetGuid("data", id)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(RoleLight)))
                    .SetReadOnly(true)
                    .List<IRoleLight>();
        }

        public IList<IRoleLight> RolesWithDataRange(int range)
        {
            const string query = @"SELECT Id, DescriptionText as Name FROM ApplicationRole WHERE Id IN(
                                SELECT ApplicationRole FROM AvailableData
                                WHERE AvailableDataRange = :range)
                                AND BuiltIn = 0";
            return ((NHibernateStatelessUnitOfWork)_unitOfWork).Session.CreateSQLQuery(query)
                    .SetInt32("range", range)
                    .SetResultTransformer(Transformers.AliasToBean(typeof(RoleLight)))
                    .SetReadOnly(true)
                    .List<IRoleLight>();
        }
    }

    public class FunctionLight : IFunctionLight
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid Parent { get; set; }
        public string ForeignSource { get; set; }
        public string ResourceName { get; set; }
        public string Role { get; set; }
    }

    public class RoleLight : IRoleLight
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }

    public class PersonInRole : IPersonInRole
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Team { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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