using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Data shared by hole application
	/// </summary>
	public interface IApplicationData : IDisposable
	{
		IDataSource Tenant(string tenantName);

		/// <summary>
		/// Gets the message broker.
		/// </summary>
		/// <value>The message broker.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-06-10
		/// </remarks>
		IMessageBrokerComposite Messaging { get; }

		//Do not use this one - should be removed!
		IDictionary<string, string> AppSettings { get; }

		///<summary>
		/// The password policy loading service
		///</summary>
		ILoadPasswordPolicyService LoadPasswordPolicyService { get; }

		void MakeSureDataSourceExists(string dataSourceName, IDictionary<string, string> applicationNhibConfiguration, string analyticsConnectionString);
		void DoOnAllTenants_AvoidUsingThis(Action<IDataSource> actionOnTenant);
	}
}