//-----------------------------------------------------------------------
// <copyright file="IOpenIdHost.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An interface implemented by both providers and relying parties.
	/// </summary>
	internal interface IOpenIdHost {
		/// <summary>
		/// Gets the security settings.
		/// </summary>
		SecuritySettings SecuritySettings { get; }

		/// <summary>
		/// Gets the web request handler.
		/// </summary>
		IDirectWebRequestHandler WebRequestHandler { get; }
	}
}
