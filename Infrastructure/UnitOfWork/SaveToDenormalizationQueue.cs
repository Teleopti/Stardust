using System;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public class SaveToDenormalizationQueue : ISaveToDenormalizationQueue
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public void Execute<T>(T message, IRunSql runSql) where T : RaptorDomainMessage
		{
			var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity);

			message.BusinessUnitId = identity.BusinessUnit.Id.GetValueOrDefault();
			message.Datasource = identity.DataSource.Application.Name;
			message.Timestamp = DateTime.UtcNow;

			runSql.Create(
				"INSERT INTO dbo.DenormalizationQueue (BusinessUnit,Timestamp,[Message],Type) VALUES (:BusinessUnit,:Timestamp,:Message,:Type)")
				.SetDateTime("Timestamp", message.Timestamp)
				.SetGuid("BusinessUnit", message.BusinessUnitId)
				.SetString("Message", SerializationHelper.SerializeAsXml(message))
				.SetString("Type", message.GetType().AssemblyQualifiedName)
				.Execute();
		}
	}
}