namespace Teleopti.Ccc.Domain.Cascading.TrackShoveling
{
	public class AddedResource
	{
		public AddedResource(CascadingSkillGroup fromSkillGroup, double resourcesMoved)
		{
			FromSkillGroup = fromSkillGroup;
			ResourcesMoved = resourcesMoved;
		}
		public CascadingSkillGroup FromSkillGroup { get; }
		public double ResourcesMoved { get; }
	}
}