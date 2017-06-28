using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

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
			var multilicatorDefinitionSet = _multiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions();
			return convertToViewModel(multilicatorDefinitionSet);
		}

		public IList<MultiplicatorDefinitionSetViewModel> GetDefinitionSets(IPerson person, DateOnly date)
		{

			var multiplicatorDefinitionSets = person.Period(date)?.PersonContract?.Contract?.MultiplicatorDefinitionSetCollection;
			return multiplicatorDefinitionSets != null
				? convertToViewModel(multiplicatorDefinitionSets)
				: new List<MultiplicatorDefinitionSetViewModel>();
		}

		private IList<MultiplicatorDefinitionSetViewModel> convertToViewModel(
			IList<IMultiplicatorDefinitionSet> multilicatorDefinitionSet)
		{
			return multilicatorDefinitionSet.Select(s => new MultiplicatorDefinitionSetViewModel
			{
				Id = s.Id.GetValueOrDefault(),
				Name = s.Name
			}).ToList();
		}
	}
}