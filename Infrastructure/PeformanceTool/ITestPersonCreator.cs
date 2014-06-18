namespace Teleopti.Ccc.Infrastructure.PeformanceTool
{
	public interface ITestPersonCreator
	{
		void CreatePersons(int numberOfPersons, int datasourceId = 6);
		void RemoveCreatedPersons();
	}
}