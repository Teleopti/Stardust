using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class ApplicationFunctionRepository : Repository<IApplicationFunction>, IApplicationFunctionRepository
	{
		private static readonly DefinedRaptorApplicationFunctionFactory _definedRaptorApplicationFunctionFactory = new DefinedRaptorApplicationFunctionFactory();

		public ApplicationFunctionRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
			: base(unitOfWork)
#pragma warning restore 618
		{
		}

		public ApplicationFunctionRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork, null, null)
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
