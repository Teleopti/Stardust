using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.PerformanceManager;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.PerformanceManagerProxy;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Analytics.PM.PMServiceHost;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
	public class PmPermissionJobStep : JobStepBase
	{
		private readonly bool _checkIfNeeded;

		public PmPermissionJobStep(IJobParameters jobParameters, bool checkIfNeeded = false)
			: base(jobParameters)
		{
			_checkIfNeeded = checkIfNeeded;
			Name = "Performance Manager permissions";
			Transformer = new PmPermissionTransformer(new PmProxyFactory());
			PermissionExtractor = new PmPermissionExtractor(new LicensedFunctionsProvider(new DefinedRaptorApplicationFunctionFactory()));
			PmWindowsUserSynchronizer = new PmWindowsUserSynchronizer();
			PmUserInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}
		
		protected virtual IUnitOfWorkFactory UnitOfWorkFactory
		{
			get { return Ccc.Infrastructure.UnitOfWork.UnitOfWorkFactory.Current; }
		}

		public IPmPermissionTransformer Transformer { get; set; }

		public IPmPermissionExtractor PermissionExtractor { get; set; }

		public IPmWindowsUserSynchronizer PmWindowsUserSynchronizer { get; set; }

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			if (!isLastBusinessUnit)
				return 0;

			if (_checkIfNeeded && !_jobParameters.StateHolder.PermissionsMustRun())
				return 0;

			var personList = _jobParameters.StateHolder.UserCollection;

			using (var uow = UnitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(personList);
				var applicationAuthUsers =
					(List<UserDto>)
						Transformer.GetUsersWithPermissionsToPerformanceManager(personList, false, PermissionExtractor, UnitOfWorkFactory);
				IList<UserDto> windowsAuthUsers = Transformer.GetUsersWithPermissionsToPerformanceManager(personList, true,
					PermissionExtractor, UnitOfWorkFactory);
				var windowsAuthValidatedUsers = PmWindowsUserSynchronizer.Synchronize(windowsAuthUsers, Transformer,
					_jobParameters.OlapServer, _jobParameters.OlapDatabase);

				var isPmWinAuth = Transformer.IsPmWindowsAuthenticated(_jobParameters.OlapServer, _jobParameters.OlapDatabase);
				if (isPmWinAuth.IsWindowsAuthentication)
					windowsAuthUsers = windowsAuthValidatedUsers;

				applicationAuthUsers.AddRange(windowsAuthUsers);
				Transformer.Transform(applicationAuthUsers, BulkInsertDataTable1);

				return _jobParameters.Helper.Repository.PersistPmUser(BulkInsertDataTable1);
			}
		}
	}
}