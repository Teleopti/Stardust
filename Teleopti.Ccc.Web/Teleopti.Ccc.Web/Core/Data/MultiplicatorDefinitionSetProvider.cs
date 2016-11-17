using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Core.Data
{
	public class MultiplicatorDefinitionSetProvider : IMultiplicatorDefinitionSetProvider
	{
		private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;

		public MultiplicatorDefinitionSetProvider(IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository)
		{
			_multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
		}

		public IList<MultiplicatorDefinitionSetViewModel> GetAll()
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