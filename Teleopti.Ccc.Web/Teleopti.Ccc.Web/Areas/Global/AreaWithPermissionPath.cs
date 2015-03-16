using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Models.Shared;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public struct AreaWithPermissionPath
	{
		public AreaWithPermissionPath(string path, Func<string> name, string internalName, params Link[] links) : this()
		{
			InternalName = internalName;
			Name = name;
			Path = path;
			Links = links;
		}

		public string Path { get; private set; }
		public Func<string> Name { get; private set; }
		public string InternalName { get; private set; }
		public IEnumerable<Link> Links { get; private set; }
	}
}