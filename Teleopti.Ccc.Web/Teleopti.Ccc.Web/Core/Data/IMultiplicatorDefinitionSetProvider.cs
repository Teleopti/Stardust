using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.Data
{
	public interface IMultiplicatorDefinitionSetProvider
	{
		IList<MultiplicatorDefinitionSetViewModel> GetAllOvertimeDefinitionSets();
		IList<MultiplicatorDefinitionSetViewModel> GetDefinitionSets(IPerson person, DateOnly date);
	}

	public class MultiplicatorDefinitionSetViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}