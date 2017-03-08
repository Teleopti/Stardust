using System.Collections.Generic;
using System.Net.Http;

namespace Teleopti.Ccc.Web.Areas.People.Core
{
	public class FileData
	{
		public string FileName { get; set; }
		public byte[] Data { get; set; }
	}

	public interface IMultipartHttpContentExtractor
	{
		T ExtractFormModel<T>(IEnumerable<HttpContent> contents) where T: new();
		IEnumerable<FileData> ExtractFileData(IEnumerable<HttpContent> contents);
	}
}