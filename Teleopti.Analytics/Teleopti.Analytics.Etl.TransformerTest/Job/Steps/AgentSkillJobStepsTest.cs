using System.Collections.Generic;
using System.Data;
using System.Threading;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.TransformerInfrastructure.DataTableDefinition;
using Teleopti.Analytics.Etl.TransformerTest.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Analytics.Etl.TransformerTest.Job.Steps
{
	[TestFixture]
	public class AgentSkillJobStepsTest
	{
		private IJobParameters _jobParameters;

		[SetUp]
		public void Setup()
		{
			_jobParameters = JobParametersFactory.SimpleParameters(false);
			_jobParameters.Helper = new JobHelper(new RaptorRepositoryForTest(), null, null, null);
		}

		[Test]
		public void VerifyAgentSkillJobSteps()
		{
			IList<IJobStep> jobStepList = new AgentSkillCollection(_jobParameters);
			foreach (IJobStep jobStep in jobStepList)
			{
				IJobStepResult jobStepResult = jobStep.Run(new List<IJobStep>(), null, null, false);
				Assert.IsNotNull(jobStepResult);
			}
			Assert.AreEqual(14, jobStepList.Count);
		}

		[Test]
		public void VerifyAgentSkillTransformer()
		{
			IList<IPerson> persons = PersonFactory.CreatePersonGraphCollection();
			DataTable dataTable = new DataTable {Locale = Thread.CurrentThread.CurrentCulture};
			PersonSkillInfrastructure.AddColumnsToDataTable(dataTable);
			AgentSkillTransform agentSkillTransform = new AgentSkillTransform();
			agentSkillTransform.Transform(persons, dataTable);
			Assert.AreEqual(1, dataTable.Rows.Count);

		}
	}
}
