using System.Collections.Generic;
using Toggle.Net.Specifications;

namespace Teleopti.Ccc.IocCommon.Toggle
{
	public class DefaultSpecification : IToggleSpecification
	{
		private readonly bool _trueByDefault;

		public DefaultSpecification(bool trueByDefault)
		{
			_trueByDefault = trueByDefault;
		}

		public bool IsEnabled(string currentUser, IDictionary<string, string> parameters)
		{
			return _trueByDefault;
		}

		public void Validate(string toggleName, IDictionary<string, string> parameters)
		{
		}
	}
}