using System.Collections.Generic;
using System.Net.Http;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Web.Areas.Global.Core
{

	public interface IMultipartHttpContentExtractor
	{
		T ExtractFormModel<T>(IEnumerable<HttpContent> contents) where T : new();
		IEnumerable<ImportFileData> ExtractFileData(IEnumerable<HttpContent> contents);
	}
}