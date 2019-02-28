using System;
using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{

    /// <summary>
    /// Used for persisting and retrieving XML based Licenses to the database
    /// </summary>
    /// <remarks>
    /// Created by: Klas
    /// Created date: 2008-12-03
    /// </remarks>
    public class LicenseRepository : Repository<ILicense>, ILicenseRepository
	{
		public static LicenseRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new LicenseRepository(currentUnitOfWork, null, null);
		}

		public static LicenseRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new LicenseRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public LicenseRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
	    {
	    }

        /// <summary>
        /// Adds the specified license to repository.
        /// Will be persisted when PersistAll is called (or sooner).
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-12-03
        /// </remarks>
        public override void Add(ILicense entity)
        {
            if(!entity.Id.HasValue)
            {
                foreach (ILicense license in LoadAll())
                {
                    Session.Delete(license);
                }
            }
            base.Add(entity);
        }

	    public IList<ActiveAgent> GetActiveAgents()
		{
			const string sql = @"select b.Name BusinessUnit, FirstName, LastName, Email, EmploymentNumber, MIN(StartDate) StartDate, p.TerminalDate LeavingDate from Person p
								INNER JOIN PersonPeriod pp ON pp.Parent=p.Id and pp.StartDate<GETDATE()
								INNER JOIN Contract c ON pp.Contract = c.Id
								INNER JOIN BusinessUnit b ON c.BusinessUnit = b.Id
								where (p.TerminalDate is null or p.TerminalDate>=GETDATE())
								and b.IsDeleted = 0
								and p.IsDeleted = 0 
								and p.id in(SELECT distinct Person from PersonAssignment pa inner join Scenario s ON pa.Scenario = s.Id
								where s.DefaultScenario = 1 AND pa.ShiftCategory is not null)
								GROUP BY b.Name, FirstName, LastName, Email, EmploymentNumber, p.TerminalDate, p.Id
								ORDER BY LastName, FirstName";

			return UnitOfWork.Session().CreateSQLQuery(sql)
				.SetResultTransformer(Transformers.AliasToBean(typeof(ActiveAgent)))
				.SetReadOnly(true)
				.List<ActiveAgent>();	
		}
    }

	
}
