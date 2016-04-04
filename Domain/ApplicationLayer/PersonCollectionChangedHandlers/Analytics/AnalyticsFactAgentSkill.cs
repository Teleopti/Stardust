namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	public class AnalyticsFactAgentSkill
	{
		public int PersonId { get; set; }
		public int SkillId { get; set; }
		public int HasSkill { get; set; }
		public bool Active { get; set; }
		public int BusinessUnitId { get; set; }
		public int DatasourceId { get; set; }
	}
}
