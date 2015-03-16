using System;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public struct AreaWithPermissionPath
	{
		public AreaWithPermissionPath(string path, Func<string> name, string internalName)
			: this()
		{
			InternalName = internalName;
			Name = name;
			Path = path;
		}

		public string Path { get; private set; }
		public Func<string> Name { get; private set; }
		public string InternalName { get; private set; }
	}
}