using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teleopti.Ccc.Web.Core.Data
{
	public interface IMultiplicatorDefinitionSetProvider
	{
		IList<MultiplicatorDefinitionSetViewModel> GetAllOvertimeDefinitionSets();
	}

	public class MultiplicatorDefinitionSetViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}