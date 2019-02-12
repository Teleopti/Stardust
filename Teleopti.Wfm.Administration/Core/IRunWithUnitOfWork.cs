using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Wfm.Administration.Core
{
	public interface IRunWithUnitOfWork
	{
		void WithGlobalScope(IDataSource dataSource, Action action);
		void WithBusinessUnitScope(IDataSource dataSource, IBusinessUnit businessUnit, Action action);
	}

	public class RunWithUnitOfWork : IRunWithUnitOfWork
	{
		private readonly IDataSourceScope _dataSourceScope;
		private readonly IUpdatedByScope _updatedByScope;
		private readonly IBusinessUnitScope _businessUnitScope;
		private readonly ITransactionHooksScope _transactionHooksScope;

		public RunWithUnitOfWork(IDataSourceScope dataSourceScope, IUpdatedByScope updatedByScope, IBusinessUnitScope businessUnitScope, ITransactionHooksScope transactionHooksScope)
		{
			_dataSourceScope = dataSourceScope;
			_updatedByScope = updatedByScope;
			_businessUnitScope = businessUnitScope;
			_transactionHooksScope = transactionHooksScope;
		}

		public void WithGlobalScope(IDataSource dataSource, Action action)
		{
			using (_dataSourceScope.OnThisThreadUse(dataSource))
			using (_transactionHooksScope.OnThisThreadExclude<MessageBrokerSender>())
			{
				var updatedBy = new Person();
				updatedBy.SetId(SystemUser.Id);
				_updatedByScope.OnThisThreadUse(updatedBy);

				using (var uow = CurrentUnitOfWorkFactory.Make().Current().CreateAndOpenUnitOfWork())
				{
					action();
					uow.PersistAll();
					uow.Flush();
					uow.Clear();
				}
				_updatedByScope.OnThisThreadUse(null);
			}
		}

		public void WithBusinessUnitScope(IDataSource dataSource, IBusinessUnit businessUnit, Action action)
		{
			using (_businessUnitScope.OnThisThreadUse(businessUnit))
				WithGlobalScope(dataSource, action);
		}
	}
}