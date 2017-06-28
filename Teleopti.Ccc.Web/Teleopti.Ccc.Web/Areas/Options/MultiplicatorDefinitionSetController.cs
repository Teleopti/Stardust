using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Core.Data;

namespace Teleopti.Ccc.Web.Areas.Options
{
	public class MultiplicatorDefinitionSetController : ApiController
	{
		private readonly IMultiplicatorDefinitionSetProvider _multiplicatorDefinitionSetProvider;

		public MultiplicatorDefinitionSetController(IMultiplicatorDefinitionSetProvider multiplicatorDefinitionSetProvider)
		{
			_multiplicatorDefinitionSetProvider = multiplicatorDefinitionSetProvider;
		}

		[UnitOfWork, HttpPost, Route("api/MultiplicatorDefinitionSet/Overtime")]
		public virtual IList<MultiplicatorDefinitionSetViewModel> FetchOvertimeMultiplicatorDefinitionSets()
		{
			return _multiplicatorDefinitionSetProvider.GetAllOvertimeDefinitionSets();
		}
	}
}
