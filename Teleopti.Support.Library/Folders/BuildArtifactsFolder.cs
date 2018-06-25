using System;

namespace Teleopti.Support.Library.Folders
{
	public class BuildArtifactsFolder
	{
		public string Path()
		{
			var repo = new RepositoryRootFolder();
			if (repo.IsRunningFromRepository())
			{
				var path = System.IO.Path.Combine(repo.Path(), "BuildArtifacts");
				return System.IO.Path.GetFullPath(path);
			}

			return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BuildArtifacts");
		}

		public override string ToString() => Path();
	}
}