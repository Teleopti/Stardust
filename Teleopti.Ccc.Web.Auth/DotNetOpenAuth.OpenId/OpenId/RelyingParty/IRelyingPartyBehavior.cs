//-----------------------------------------------------------------------
// <copyright file="IRelyingPartyBehavior.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System.Diagnostics.Contracts;

	/// <summary>
	/// Applies a custom security policy to certain OpenID security settings and behaviors.
	/// </summary>
	[ContractClass(typeof(IRelyingPartyBehaviorContract))]
	public interface IRelyingPartyBehavior {
		/// <summary>
		/// Applies a well known set of security requirements to a default set of security settings.
		/// </summary>
		/// <param name="securitySettings">The security settings to enhance with the requirements of this profile.</param>
		/// <remarks>
		/// Care should be taken to never decrease security when applying a profile.
		/// Profiles should only enhance security requirements to avoid being
		/// incompatible with each other.
		/// </remarks>
		void ApplySecuritySettings(RelyingPartySecuritySettings securitySettings);
	}

	/// <summary>
	/// Contract class for the <see cref="IRelyingPartyBehavior"/> interface.
	/// </summary>
	[ContractClassFor(typeof(IRelyingPartyBehavior))]
	internal abstract class IRelyingPartyBehaviorContract : IRelyingPartyBehavior {
		/// <summary>
		/// Prevents a default instance of the <see cref="IRelyingPartyBehaviorContract"/> class from being created.
		/// </summary>
		private IRelyingPartyBehaviorContract() {
		}

		#region IRelyingPartyBehavior Members

		/// <summary>
		/// Applies a well known set of security requirements to a default set of security settings.
		/// </summary>
		/// <param name="securitySettings">The security settings to enhance with the requirements of this profile.</param>
		/// <remarks>
		/// Care should be taken to never decrease security when applying a profile.
		/// Profiles should only enhance security requirements to avoid being
		/// incompatible with each other.
		/// </remarks>
		void IRelyingPartyBehavior.ApplySecuritySettings(RelyingPartySecuritySettings securitySettings) {
			Requires.NotNull(securitySettings, "securitySettings");
		}
		
		#endregion
	}
}
