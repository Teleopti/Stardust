namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config
{
	public class NhibFilePathFixed : INhibFilePath
	{
		private readonly string _path;

		public NhibFilePathFixed(string path)
		{
			_path = path;
		}

		public string Path()
		{
			return _path;
		}
	}
}