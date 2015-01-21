namespace Teleopti.Ccc.Infrastructure.PerformanceTool
{
	public interface IStateGenerator
	{
		void Generate(int count);
	}
	public class StateGenerator : IStateGenerator
	{
		public void Generate(int count)
		{
			throw new System.NotImplementedException();
		}
	}
}