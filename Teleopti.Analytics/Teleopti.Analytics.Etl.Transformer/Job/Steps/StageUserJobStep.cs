using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    public class StageUserJobStep : JobStepBase
    {
        public StageUserJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
            Name = "stg_user";
            IsBusinessUnitIndependent = true;
            UserInfrastructure.AddColumnsToDataTable(BulkInsertDataTable1);
        }

        protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
        {
            IList<IPerson> rootList = _jobParameters.StateHolder.UserCollection;

            //Transform data from Raptor to Matrix format
            var raptorTransformer = new UserTransformer();
            raptorTransformer.Transform(rootList, BulkInsertDataTable1);

            int affectedRows = _jobParameters.Helper.Repository.PersistUser(BulkInsertDataTable1);

            return affectedRows;
        }
    }
}