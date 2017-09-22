﻿//-----------------------------------------------------------------------
// <copyright file="SkipSecurityBindingElement.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.ChannelElements {
	using System.Diagnostics;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Spoofs security checks on incoming OpenID messages.
	/// </summary>
	internal class SkipSecurityBindingElement : IChannelBindingElement {
		#region IChannelBindingElement Members

		/// <summary>
		/// Gets or sets the channel that this binding element belongs to.
		/// </summary>
		/// <value></value>
		/// <remarks>
		/// This property is set by the channel when it is first constructed.
		/// </remarks>
		public Channel Channel { get; set; }

		/// <summary>
		/// Gets the protection commonly offered (if any) by this binding element.
		/// </summary>
		/// <value><see cref="MessageProtections.All"/></value>
		/// <remarks>
		/// This value is used to assist in sorting binding elements in the channel stack.
		/// </remarks>
		public MessageProtections Protection {
			get { return MessageProtections.All; }
		}

		/// <summary>
		/// Prepares a message for sending based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The message to prepare for sending.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public MessageProtections? ProcessOutgoingMessage(IProtocolMessage message) {
			Debug.Fail("SkipSecurityBindingElement.ProcessOutgoingMessage should never be called.");
			return null;
		}

		/// <summary>
		/// Performs any transformation on an incoming message that may be necessary and/or
		/// validates an incoming message based on the rules of this channel binding element.
		/// </summary>
		/// <param name="message">The incoming message to process.</param>
		/// <returns>
		/// The protections (if any) that this binding element applied to the message.
		/// Null if this binding element did not even apply to this binding element.
		/// </returns>
		/// <exception cref="ProtocolException">
		/// Thrown when the binding element rules indicate that this message is invalid and should
		/// NOT be processed.
		/// </exception>
		/// <remarks>
		/// Implementations that provide message protection must honor the
		/// <see cref="MessagePartAttribute.RequiredProtection"/> properties where applicable.
		/// </remarks>
		public MessageProtections? ProcessIncomingMessage(IProtocolMessage message) {
			var signedMessage = message as ITamperResistantOpenIdMessage;
			if (signedMessage != null) {
				Logger.Bindings.DebugFormat("Skipped security checks of incoming {0} message for preview purposes.", message.GetType().Name);
				return this.Protection;
			}

			return null;
		}

		#endregion
	}
}
