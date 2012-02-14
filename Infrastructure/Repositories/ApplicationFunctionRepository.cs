using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for application function 
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2007-11-12
    /// </remarks>
    public class ApplicationFunctionRepository : Repository<IApplicationFunction>, IApplicationFunctionRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationFunctionRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-11-12
        /// </remarks>
        public ApplicationFunctionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Finds all application functions sorted by name.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2007-12-07
        /// </remarks>
        public IList<IApplicationFunction> GetAllApplicationFunctionSortedByCode()
        {
            IList<IApplicationFunction> functions = Session.CreateCriteria(typeof(ApplicationFunction))
                                                           .AddOrder(Order.Asc("FunctionCode"))
                                                           .List<IApplicationFunction>();

            SynchronizeApplicationFunctions(functions);
            return functions;
        }

        public IEnumerable<IApplicationFunction> ExternalApplicationFunctions()
        {
            IList<IApplicationFunction> functions = Session.CreateCriteria(typeof (ApplicationFunction))
                .Add(Restrictions.Not(Restrictions.Eq("ForeignSource", DefinedForeignSourceNames.SourceRaptor)))
                .AddOrder(Order.Asc("FunctionCode"))
                .List<IApplicationFunction>();
            return functions;
        }

		public override bool ValidateUserLoggedOn
		{
			get
			{
				return false;
			}
		}

        /// <summary>
        /// Synchronizes the specified application function .
        /// </summary>
        /// <param name="applicationFunction">The application function.</param>
        protected static void SynchronizeRaptorApplicationFunctions(IApplicationFunction applicationFunction)
        {
            if (applicationFunction.ForeignSource == DefinedForeignSourceNames.SourceRaptor)
            {
                IApplicationFunction raptorCounterpart =
                    ApplicationFunction.FindByForeignId(new DefinedRaptorApplicationFunctionFactory().ApplicationFunctionList,
                                                        DefinedForeignSourceNames.SourceRaptor,
                                                        applicationFunction.ForeignId);


                // and we can override some properties if they are hard coded 
                if (raptorCounterpart == null)
                {
                    throw new PermissionException("The following Application Function has been removed recently from code but still exists in the database: " + applicationFunction.FunctionPath);
                }
                applicationFunction.IsPreliminary = raptorCounterpart.IsPreliminary;
                applicationFunction.FunctionCode = raptorCounterpart.FunctionCode;
                applicationFunction.FunctionDescription = raptorCounterpart.FunctionDescription;
                applicationFunction.SortOrder = raptorCounterpart.SortOrder;
            }
        }

        /// <summary>
        /// Synchronizes the specified application functions.
        /// </summary>
        /// <param name="applicationFunctions">The application functions.</param>
        protected static void SynchronizeApplicationFunctions(IEnumerable<IApplicationFunction> applicationFunctions)
        {
            foreach (IApplicationFunction applicationFunction in applicationFunctions)
            {
                SynchronizeRaptorApplicationFunctions(applicationFunction);
            }
        }
    }
}
