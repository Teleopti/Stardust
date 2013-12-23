using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
	/// <summary>
	/// A factory for IUnitOfWork instances.
	/// Will also set mappings to data source.
	/// </summary>
	public interface IUnitOfWorkFactory : IDisposable
	{
		/// <summary>
		/// Creates and opens a unit of work.
		/// </summary>
		/// <returns>An UnitOfWork</returns>
		/// <remarks>
		/// It's the client's responsibility to close
		/// this UnitOfWork at appropriate time!
		/// </remarks>
		IUnitOfWork CreateAndOpenUnitOfWork(TransactionIsolationLevel isolationLevel = TransactionIsolationLevel.Default);

		IUnitOfWork CreateAndOpenUnitOfWork(IInitiatorIdentifier initiator);

		/// <summary>
		/// Creates and opens a stateless unit of work.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-04-22
		/// </remarks>
		IStatelessUnitOfWork CreateAndOpenStatelessUnitOfWork();

		/// <summary>
		/// Gets the name of the UnitOfWork factory.
		/// </summary>
		/// <value>The name.</value>
		/// <remarks>A unique name</remarks>
		string Name { get; }

		/// <summary>
		/// Closes and releases resources on this IUnitOfWorkFactory.
		/// It's the client's responsibility to close all IUnitOfWorks first!
		/// </summary>
		void Close();

		/// <summary>
		/// Gets the number of live unit of works.
		/// If running in release mode, null is returned
		/// </summary>
		/// <value>The number of live unit of works.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-09-04
		/// </remarks>
		long? NumberOfLiveUnitOfWorks { get; }

		/// <summary>
		/// Currents the unit of work.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2010-07-02
		/// </remarks>
		IUnitOfWork CurrentUnitOfWork();

		/// <summary>
		/// Returns if curent uow exists
		/// </summary>
		/// <returns></returns>
		bool HasCurrentUnitOfWork();

		/// <summary>
		/// Returns the audit setting delegate used by auditing
		/// </summary>
		IAuditSetter AuditSetting { get; }

		/// <summary>
		/// The connection string.
		/// </summary>
		string ConnectionString { get; }
	}
}