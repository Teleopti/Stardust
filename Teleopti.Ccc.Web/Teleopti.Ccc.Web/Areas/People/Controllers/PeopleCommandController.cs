using System;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.People.Core.Providers;

namespace Teleopti.Ccc.Web.Areas.People.Controllers
{
	public class PeopleCommandController : ApiController
	{
		private readonly IPersonInfoUpdater _personInfoUpdater;

		public PeopleCommandController(IPersonInfoUpdater personInfoUpdater)
		{
			_personInfoUpdater = personInfoUpdater;
		}

		[HttpPost, Route("api/PeopleCommand/updatePeople")]
		public virtual JsonResult<ResultModel> UpdatePersonInfo(PeopleCommandInput model)
		{
			var result = new ResultModel();
			try
			{
				result.SuccessCount = InternalUpdatePersonInfo(model);

				result.Success = true;
			}
			catch (Exception e)
			{
				result.Success = false;
				result.ErrorMsg = e.Message;
			}

			return Json(result);
		}

		[UnitOfWork]
		protected virtual int InternalUpdatePersonInfo(PeopleCommandInput model)
		{
			return _personInfoUpdater.UpdatePersonInfo(model);
		}
	}
}