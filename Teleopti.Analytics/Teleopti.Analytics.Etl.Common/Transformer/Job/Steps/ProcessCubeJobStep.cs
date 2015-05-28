using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;

namespace Teleopti.Analytics.Etl.Transformer.Job.Steps
{
    class ProcessCubeJobStep : JobStepBase
    {
        public ProcessCubeJobStep(IJobParameters jobParameters)
            : base(jobParameters)
        {
        }

        protected override int RunStep()
        {
            new CubeRepository().Process("localhost", "testcube");
            return -1;
        }
    }
}
