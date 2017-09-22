using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Infrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.Common.Interfaces.PerformanceManager;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class PmPermissionJobStep : JobStepBase
	{
		private readonly bool _checkIfNeeded;

		public PmPermissionJobStep(IJobParameters jobParameters, bool checkIfNeeded = false)
			: base(jobParameters)
		{
			_checkIfNeeded = checkIfNeeded;
			Name = "Performance Manager permissions";
			Transformer = new PmPermissionTransformer();
			PermissionExtractor = new PmPermissionExtractor(new LicensedFunctionsProvider(new DefinedRaptorApplicationFunctionFactory()));
			PmUserInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
		}

		protected virtual IUnitOfWorkFactory UnitOfWorkFactory
		{
			get { return Ccc.Infrastructure.UnitOfWork.UnitOfWorkFactory.Current; }
		}

		public IPmPermissionTransformer Transformer { get; set; }

		public IPmPermissionExtractor PermissionExtractor { get; set; }


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
				var pmUsers = Transformer.GetUsersWithPermissionsToPerformanceManager(personList, PermissionExtractor, UnitOfWorkFactory);

				Transformer.Transform(pmUsers, BulkInsertDataTable1);

				return _jobParameters.Helper.Repository.PersistPmUser(BulkInsertDataTable1);
			}
		}
	}
}