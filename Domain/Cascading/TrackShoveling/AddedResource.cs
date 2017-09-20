namespace Teleopti.Ccc.Domain.Cascading.TrackShoveling
{
	public class AddedResource
	{
		public AddedResource(CascadingSkillSet fromSkillSet, double resourcesMoved)
		{
			FromSkillSet = fromSkillSet;
			ResourcesMoved = resourcesMoved;
		}
		public CascadingSkillSet FromSkillSet { get; }
		public double ResourcesMoved { get; }
	}
}