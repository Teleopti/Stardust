namespace Teleopti.Ccc.Domain.Islands
{
	public class MergeIslandsSizeLimit
	{
		public int Limit  { get; private set; } = 50;
		
		public void TurnOff_UseOnlyFromTest()
		{
			Limit = 0;
		}
	}
}