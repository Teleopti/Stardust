using System.Collections.Generic;
using System.Net.Http;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent;

namespace Teleopti.Ccc.Web.Areas.People.Core
{

	public interface IMultipartHttpContentExtractor
	{
		T ExtractFormModel<T>(IEnumerable<HttpContent> contents) where T : new();
		IEnumerable<FileData> ExtractFileData(IEnumerable<HttpContent> contents);
	}
}