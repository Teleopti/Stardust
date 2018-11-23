using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Web.Areas.Global.Core
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

				prop.SetValue(formModel, content.ReadAsStringAsync().GetAwaiter().GetResult());
				hasFallBacks = true;
			}
			

			return hasFallBacks ?  formModel : default(T);			
		}

		public IEnumerable<ImportFileData> ExtractFileData(IEnumerable<HttpContent> contents)
		{
			return from content in contents
				where content.Headers.ContentDisposition.Name.Trim('\"') == "file"
				select new ImportFileData
				{
					FileName = content.Headers.ContentDisposition.FileName.Trim('\"'),
					Data = content.ReadAsByteArrayAsync().GetAwaiter().GetResult()
				};
		}
	}

	
}