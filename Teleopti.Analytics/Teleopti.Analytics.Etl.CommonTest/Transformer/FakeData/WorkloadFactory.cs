using System;
using System.Collections.Generic;
using System.Drawing;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData
{
	public static class WorkloadFactory
	{
		public static IList<IWorkload> CreateWorkloadList()
		{
			ISkillType skillType = SkillTypeFactory.CreateSkillType();
			skillType.SetId(Guid.NewGuid());

			ISkill skill1 = new Skill("skill 1", "desc skill 1", Color.FromArgb(0), 15, skillType);
			skill1.SetId(Guid.NewGuid());
			skill1.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));

			ISkill skill2 = new Skill("skill 2", "desc skill 2 [deleted]", Color.FromArgb(255), 15, skillType);
			skill2.SetId(Guid.NewGuid());
			// Set IsDeleted = True
			((IDeleteTag)skill2).SetDeleted();
			skill2.TimeZone = (TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));

			IWorkload workload1 = new Workload(skill1);
			IWorkload workload2 = new Workload(skill2);
			IWorkload workload3 = new Workload(skill1);

			workload1.SetId(Guid.NewGuid());
			workload1.Name = "workload 1";
			RaptorTransformerHelper.SetUpdatedOn(workload1, DateTime.Now);

			workload2.SetId(Guid.NewGuid());
			workload2.Name = "workload 2 [deleted]";
			RaptorTransformerHelper.SetUpdatedOn(workload2, DateTime.Now);
			// Set IsDeleted = True
			((IDeleteTag)workload2).SetDeleted();

			workload3.SetId(Guid.NewGuid());
			workload3.Name = "workload 3";
			RaptorTransformerHelper.SetUpdatedOn(workload3, DateTime.Now);

			workload1.QueueAdjustments = new QueueAdjustment
														{
															Abandoned = new Percent(-0.9),
															AbandonedAfterServiceLevel = new Percent(0.8),
															AbandonedShort = new Percent(0.7),
															AbandonedWithinServiceLevel = new Percent(0.6),
															OfferedTasks = new Percent(1.0),
															OverflowIn = new Percent(0.5),
															OverflowOut = new Percent(-1.0)
														};

			workload1.AddQueueSource(QueueSourceFactory.CreateQueueSource());
			workload2.AddQueueSource(QueueSourceFactory.CreateQueueSourceHelpdesk());
			workload3.AddQueueSource(QueueSourceFactory.CreateQueueSourceInrikes());

			return new List<IWorkload> { workload1, workload2, workload3 };
		}
	}
}