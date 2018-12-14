using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Persisters.Plans
{
	[DatabaseTest]
	public class PlanningGroupModelPersisterTest
	{
		public WithUnitOfWork WithUnitOfWork;
		
		public IPlanningGroupModelPersister Target;
		public IPlanningGroupRepository PlanningGroupRepository;
		
		[Test]
		public void ShouldSavePlanningGroupSetting()
		{
			var planningGroupModel = new PlanningGroupModel
			{
				Name = "pg1",
				Settings = new[]
				{
					new PlanningGroupSettingsModel
					{
						Default = true,
					},
					new PlanningGroupSettingsModel
					{
						Default = false,
						Name = "pgs1"
					}
				}
			};
			
			WithUnitOfWork.Do(() =>
			{
				Target.Persist(planningGroupModel);
			});

			WithUnitOfWork.Do(() =>
			{
				var inDb = PlanningGroupRepository.LoadAll().Single();
				inDb.Name.Should().Be.EqualTo(planningGroupModel.Name);
				inDb.Settings.Single(x => !x.Default).Name.Should().Be
					.EqualTo(planningGroupModel.Settings.Single(x => !x.Default).Name);
			});
			
		}
	}
}