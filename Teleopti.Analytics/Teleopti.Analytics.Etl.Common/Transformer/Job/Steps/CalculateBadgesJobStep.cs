using Autofac;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class CalculateBadgesJobStep : JobStepBase
	{
		public CalculateBadgesJobStep(IJobParameters jobParameters) : base(jobParameters)
		{
			Name = "CalculateBadges";
		}

		protected virtual IUnitOfWorkFactory UnitOfWorkFactory
		{
			get { return Ccc.Infrastructure.UnitOfWork.UnitOfWorkFactory.Current; }
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			var performBadgeCalculation = _jobParameters.ContainerHolder.IocContainer.Resolve<IPerformBadgeCalculation>();
			var isTeamGamificationSettingsAvailable = _jobParameters.ContainerHolder.IocContainer.Resolve<IIsTeamGamificationSettingsAvailable>();
			using (var uow = UnitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				if (isTeamGamificationSettingsAvailable.Satisfy())
				{
					performBadgeCalculation.Calculate(RaptorTransformerHelper.CurrentBusinessUnit.Id.Value);
				}
				uow.PersistAll();
			}
			return 0;
		}
	}
}