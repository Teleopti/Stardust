using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.Web.Core.Licensing
{
	public class QueryStringReader : IQueryStringReader
	{
		private readonly ICurrentHttpContext _currentHttpContext;

		public QueryStringReader(ICurrentHttpContext currentHttpContext)
		{
			_currentHttpContext = currentHttpContext;
		}

		public string GetValue(string name)
		{
			return _currentHttpContext.Current().Request.QueryString[name];
		}
	}
}