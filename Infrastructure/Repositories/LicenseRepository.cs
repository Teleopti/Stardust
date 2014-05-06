﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

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
        /// <summary>
        /// Initializes a new instance of the <see cref="LicenseRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork used for retrieving a license.</param>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-12-03
        /// </remarks>
        public LicenseRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public LicenseRepository(IUnitOfWorkFactory unitOfWorkFactory) : base(unitOfWorkFactory)
        {
        }

				public LicenseRepository(ICurrentUnitOfWork currentUnitOfWork)
					: base(currentUnitOfWork)
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

        /// <summary>
        /// Adds the specified license collection to repository.
        /// Will be persisted when PersistAll is called (or sooner).
        /// </summary>
        /// <param name="entityCollection">The entity collection.</param>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-12-03
        /// </remarks>
        public override void AddRange(IEnumerable<ILicense> entityCollection)
        {
            if (entityCollection.Count() > 1)
                throw new DataSourceException("Attempted to add more than one license");
            if (entityCollection.Any())
            {
                foreach (ILicense license in entityCollection)
                {
                    Add(license);
                }
            }
        }

        public override bool ValidateUserLoggedOn
        {
            get
            {
                return false;
            }
        }

		public IList<ActiveAgent> GetActiveAgents()
		{
			const string sql = @"select b.Name BusinessUnit, FirstName, LastName, Email, EmploymentNumber, MIN(StartDate) StartDate, p.TerminalDate LeavingDate from Person p
INNER JOIN PersonPeriod pp ON pp.Parent=p.Id and pp.StartDate<GETDATE()
INNER JOIN Contract c ON pp.Contract = c.Id
INNER JOIN BusinessUnit b ON c.BusinessUnit = b.Id
where (p.TerminalDate is null or p.TerminalDate>=GETDATE())
and p.IsDeleted = 0 
and p.id in(SELECT distinct Person from PersonAssignment pa inner join Scenario s ON pa.Scenario = s.Id
where s.DefaultScenario = 1 AND pa.ShiftCategory is not null)
GROUP BY b.Name, FirstName, LastName, Email, EmploymentNumber, p.TerminalDate
ORDER BY LastName, FirstName";

			using (var uow = UnitOfWorkFactory.CurrentUnitOfWorkFactory().LoggedOnUnitOfWorkFactory().CreateAndOpenStatelessUnitOfWork())
			{ 
				return ((NHibernateStatelessUnitOfWork)uow).Session.CreateSQLQuery(sql)
				.SetResultTransformer(Transformers.AliasToBean(typeof(ActiveAgent)))
				.SetReadOnly(true)
				.List<ActiveAgent>();
			}
		}
    }

	
}
