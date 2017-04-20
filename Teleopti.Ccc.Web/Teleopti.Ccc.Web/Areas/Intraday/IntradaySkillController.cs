using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebIntraday)]
	public class IntradaySkillController : ApiController
	{
		private readonly FetchSkillInIntraday _fetchSkillInIntraday;

		public IntradaySkillController(FetchSkillInIntraday fetchSkillInIntraday)
		{
			_fetchSkillInIntraday = fetchSkillInIntraday;
		}

		[UnitOfWork, HttpGet, Route("api/intraday/skills")]
		public virtual IHttpActionResult GetAllSkills()
		{
			return Ok(_fetchSkillInIntraday.GetAll()
				.Select(x => new
				{
					x.Id,
					x.Name,
					x.DoDisplayData,
					x.SkillType
				})
				.ToArray());
		}
	}
}