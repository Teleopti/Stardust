using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class CurrentBusinessUnit : ICurrentBusinessUnit
	{
		private readonly ICurrentIdentity _identity;
		private readonly IIsHttpRequest _isHttpRequest;

		private static ICurrentBusinessUnit _instanceForEntities;

		public static ICurrentBusinessUnit Make()
		{
			var identity = new CurrentIdentity(new CurrentTeleoptiPrincipal());
			return new CurrentBusinessUnit(identity, new HttpRequestFalse());
		}

		public static ICurrentBusinessUnit InstanceForEntities
		{
			get
			{
				if (_instanceForEntities != null)
					return _instanceForEntities;

				var instance = Make();
				_instanceForEntities = instance;
				return _instanceForEntities;
			}
		}

		public static void SetInstanceFromContainer(ICurrentBusinessUnit instance)
		{
			_instanceForEntities = instance;
		}

		public CurrentBusinessUnit(ICurrentIdentity identity, IIsHttpRequest isHttpRequest)
		{
			_identity = identity;
			_isHttpRequest = isHttpRequest;
		}

		public IBusinessUnit Current()
		{
			IBusinessUnit businessUnit = null;
			if (_isHttpRequest.IsHttpRequest())
			{
				businessUnit = _isHttpRequest.BusinessUnitForRequest();
			}
			if (businessUnit == null)
			{
				var identity = _identity.Current();
				return identity == null ? null : identity.BusinessUnit;
			}
			return businessUnit;
		}
	}
}