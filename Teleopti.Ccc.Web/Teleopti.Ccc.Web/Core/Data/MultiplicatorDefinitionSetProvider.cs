using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Web.Core.Data
{
	public class MultiplicatorDefinitionSetProvider : IMultiplicatorDefinitionSetProvider
	{
		private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;

		public MultiplicatorDefinitionSetProvider(IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository)
		{
			_multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
		}

		public IList<MultiplicatorDefinitionSetViewModel> GetAllOvertimeDefinitionSets()
		{
			var definitionSets = _multiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions();

			return definitionSets.Select(s => new MultiplicatorDefinitionSetViewModel
			{
				Id = s.Id.GetValueOrDefault(),
				Name = s.Name
			}).ToList();
		}
	}
}