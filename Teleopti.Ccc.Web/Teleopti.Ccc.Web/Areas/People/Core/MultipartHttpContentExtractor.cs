using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Teleopti.Ccc.Web.Areas.People.Core
{
	public class MultipartHttpContentExtractor : IMultipartHttpContentExtractor
	{
		public T ExtractFormModel<T>(IEnumerable<HttpContent> contents) where T: new()
		{
			var formModel = new T();
			var props = typeof(T).GetProperties();
			var hasFallBacks = false;

			foreach (var content in contents)
			{
				var fieldName = content.Headers.ContentDisposition.Name.Trim('\"');
				if (fieldName == "file") continue;

				var prop = props.FirstOrDefault(p => p.Name == fieldName);
				if (prop == null) continue;

				prop.SetValue(formModel, content.ReadAsStringAsync().Result);
				hasFallBacks = true;
			}
			

			return hasFallBacks ?  formModel : default(T);			
		}

		public IEnumerable<FileData> ExtractFileData(IEnumerable<HttpContent> contents)
		{
			return from content in contents
				where content.Headers.ContentDisposition.Name.Trim('\"') == "file"
				select new FileData
				{
					FileName = content.Headers.ContentDisposition.FileName.Trim('\"'),
					Data = content.ReadAsByteArrayAsync().Result
				};
		}
	}

	
}