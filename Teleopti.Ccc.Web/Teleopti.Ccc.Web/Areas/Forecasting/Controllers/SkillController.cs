using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage)]
	public class SkillController : ApiController
	{
		private readonly ISkillRepository _skillRepository;

		public SkillController(ISkillRepository skillRepository)
		{
			_skillRepository = skillRepository;
		}

		[UnitOfWorkApiAction]
		public IEnumerable<NameWithId> Get()
		{
			return _skillRepository.LoadAll().Select(s => new NameWithId {Id = s.Id.GetValueOrDefault(), Name = s.Name});
		}
	}
}