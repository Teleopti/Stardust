using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Core.Data
{
	public interface IMultiplicatorDefinitionSetProvider
	{
		IList<MultiplicatorDefinitionSetViewModel> GetAllOvertimeDefinitionSets();
		IList<MultiplicatorDefinitionSetViewModel> GetDefinitionSets(IPerson person, DateOnly date);
		IList<MultiplicatorDefinitionSetViewModel> GetDefinitionSetsForCurrentUser();
	}

	public class MultiplicatorDefinitionSetViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}