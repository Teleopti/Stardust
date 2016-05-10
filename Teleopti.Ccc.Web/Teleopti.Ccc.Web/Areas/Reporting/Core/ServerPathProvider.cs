using System.Web;

namespace Teleopti.Ccc.Web.Areas.Reporting.Core
{
	public class ServerPathProvider : IPathProvider
	{
		public string MapPath(string path)
		{
			return HttpContext.Current.Server.MapPath(path);
		}
	}
}