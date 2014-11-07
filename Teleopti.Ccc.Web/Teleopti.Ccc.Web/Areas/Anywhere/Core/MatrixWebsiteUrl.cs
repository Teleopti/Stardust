using System.Configuration;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class MatrixWebsiteUrl : IMatrixWebsiteUrl
	{

		public string Build()
		{
			var matrixWebsiteUrl = ConfigurationManager.AppSettings["MatrixWebSiteUrl"];
			if (!string.IsNullOrEmpty(matrixWebsiteUrl) && !matrixWebsiteUrl.EndsWith("/"))
			{
				matrixWebsiteUrl += "/";
			}

			return matrixWebsiteUrl;
		}
	}
}