using Microsoft.AnalysisServices;

namespace AnalysisServicesManager
{
	public class CubeDimension
	{
		private readonly ServerConnectionInfo _analysisConnectionInfo;

		public CubeDimension(ServerConnectionInfo analysisConnectionInfo)
		{
			_analysisConnectionInfo = analysisConnectionInfo;
		}

		public void AddByFileName(string dimensionName)
		{
			using (var server = new Server())
			{
				server.Connect(_analysisConnectionInfo.ConnectionString);
				using (var targetDb = server.Databases.GetByName(_analysisConnectionInfo.DatabaseName))
				{
					Cube cube = targetDb.Cubes.FindByName(Repository.CubeName);

					Dimension dim = targetDb.Dimensions.GetByName(dimensionName);
					cube.Dimensions.Add(dim.Name, dim.Name, dim.Name);
					cube.Update(UpdateOptions.ExpandFull);
				}
			}
		}
	}
}