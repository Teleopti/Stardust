using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Data shared by hole application
	/// </summary>
	public interface IApplicationData : IDisposable
	{
		/// <summary>
		/// Gets the registered data sources kept in nhib files.
		/// </summary>
		/// <value>The registered data sources.</value>
		IEnumerable<IDataSource> RegisteredDataSourceCollection { get; }

		/// <summary>
		/// Gets the message broker.
		/// </summary>
		/// <value>The message broker.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-06-10
		/// </remarks>
		IMessageBroker Messaging { get; }

		/// <summary>
		/// Gets the application wide settings. This replaces ConfigurationManager.AppSettings, which should not be used
		/// </summary>
		/// <value>The app settings.</value>
		/// <remarks>
		/// Created by: Klas
		/// Created date: 2008-12-11
		/// </remarks>
		IDictionary<string, string> AppSettings { get; }

		///<summary>
		/// The password policy loading service
		///</summary>
		ILoadPasswordPolicyService LoadPasswordPolicyService { get; }

		/// <summary>
		/// Disposes and removes specified <paramref name="dataSource"/> 
		/// from <see cref="RegisteredDataSourceCollection"/>.
		/// Good to use after logon in hosts where only one database is used.
		/// </summary>
		/// <param name="dataSource"></param>
		void DisposeAllDataSourcesExcept(IDataSource dataSource);
	}
}