using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Web.Areas.Global
{
	
	public class ImportCommonAction
	{
		public static async Task<IEnumerable<HttpContent>> ReadAsMultipartAsync(HttpContent content)
		{
			try
			{
				var provider = new MultipartMemoryStreamProvider();
				await content.ReadAsMultipartAsync(provider);
				return provider.Contents;
			}
			catch (ArgumentNullException e)
			{
				throw new ArgumentNullException(e.ParamName, Resources.NoInput);
			}

		}
	}
}