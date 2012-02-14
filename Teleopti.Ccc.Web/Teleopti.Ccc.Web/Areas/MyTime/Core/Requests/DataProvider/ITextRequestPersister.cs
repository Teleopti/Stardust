using System;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public interface ITextRequestPersister
	{
		RequestViewModel Persist(TextRequestForm form);
		void Delete(Guid id);
	}
}