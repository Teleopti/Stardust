using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ApplicationFunctionRepository : Repository<IApplicationFunction>, IApplicationFunctionRepository
	{
		public static ApplicationFunctionRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new ApplicationFunctionRepository(currentUnitOfWork, null, null);
		}

		public static ApplicationFunctionRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new ApplicationFunctionRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		private static readonly DefinedRaptorApplicationFunctionFactory _definedRaptorApplicationFunctionFactory = new DefinedRaptorApplicationFunctionFactory();

		public ApplicationFunctionRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy) 
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
		{
		}

		public IList<IApplicationFunction> GetAllApplicationFunctionSortedByCode()
		{
			IList<IApplicationFunction> functions = Session.CreateCriteria(typeof(ApplicationFunction))
														   .AddOrder(Order.Asc("FunctionCode"))
														   .List<IApplicationFunction>();

			synchronizeApplicationFunctions(functions);
			return functions;
		}

		public IEnumerable<IApplicationFunction> ExternalApplicationFunctions()
		{
			IList<IApplicationFunction> functions = Session.CreateCriteria(typeof(ApplicationFunction))
				.Add(Restrictions.Not(Restrictions.Eq("ForeignSource", DefinedForeignSourceNames.SourceRaptor)))
				.AddOrder(Order.Asc("FunctionCode"))
				.List<IApplicationFunction>();
			return functions;
		}
		
		private void synchronizeRaptorApplicationFunctions(IApplicationFunction applicationFunction)
		{
			if (applicationFunction.ForeignSource == DefinedForeignSourceNames.SourceRaptor)
			{
				IApplicationFunction raptorCounterpart =
					ApplicationFunction.FindByForeignId(_definedRaptorApplicationFunctionFactory.ApplicationFunctions,
														DefinedForeignSourceNames.SourceRaptor,
														applicationFunction.ForeignId);


				// and we can override some properties if they are hard coded 
				if (raptorCounterpart != null)
				{
					applicationFunction.IsPreliminary = raptorCounterpart.IsPreliminary;
					applicationFunction.FunctionCode = raptorCounterpart.FunctionCode;
					applicationFunction.FunctionDescription = raptorCounterpart.FunctionDescription;
					applicationFunction.SortOrder = raptorCounterpart.SortOrder;
				}
			}
		}

		private void synchronizeApplicationFunctions(IEnumerable<IApplicationFunction> applicationFunctions)
		{
			foreach (IApplicationFunction applicationFunction in applicationFunctions)
			{
				synchronizeRaptorApplicationFunctions(applicationFunction);
			}
		}

		public IList<IApplicationFunction> GetChildFunctions(Guid id)
		{
			IList<IApplicationFunction> functions = Session.CreateCriteria(typeof(ApplicationFunction))
				.Add(Restrictions.Eq("Parent.Id",id))
				.List<IApplicationFunction>();
			return functions;
		}
	}
}
