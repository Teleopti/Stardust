using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
	/// <summary>
	/// Token based authentication
	/// </summary>
	public class AuthenticationInfo : AggregateEntity, IAuthenticationInfo
	{
		private string _identity;

		public virtual string Identity {
			get { return _identity ?? (_identity = string.Empty); }
			set { _identity = value; }
		}
	}
}