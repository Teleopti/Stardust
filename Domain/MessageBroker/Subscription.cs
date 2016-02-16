using System;
using System.Collections.Specialized;
using System.Xml;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.MessageBroker
{
	[Serializable]
	public class Subscription
	{
		public const string Separator = "/";
		public const string ShortTerm = "ShortTerm";

		public Subscription()
		{
			BusinessUnitId = Guid.Empty.ToString();
		}

		public string Route()
		{
			var basicRoute = string.Join(
				Separator,
				new[]
				{
					dataSource(),
					businessUnit(),
					DomainType
				});
			
			if (DateTime.UtcNow.AddDays(3) > UpperBoundaryAsDateTime() && LowerBoundaryAsDateTime() > DateTime.UtcNow.AddDays(-3))
			{
				basicRoute = String.Join(Separator, new[] {basicRoute, ShortTerm});
			}

			if (!string.IsNullOrEmpty(DomainId))
			{
				return String.Join("/", new[] {basicRoute, "id", DomainId});
			}

			if (!string.IsNullOrEmpty(DomainReferenceId))
			{
				return String.Join("/", new[] { basicRoute, "ref", DomainReferenceId});
			}

			return basicRoute;
		}

		// DO NOT USE THIS, PLEASE MAKE SURE THE SUBSCRIPTION DOES NOT CONTAIN THE DATASOURCE INSTEAD!
		private static readonly StringCollection TypesWithDatasourceException = new StringCollection
		{
			typeof (AgentStateReadModel).Name,
			"SiteAdherenceMessage",
			"TeamAdherenceMessage",
			"AgentsAdherenceMessage"
		};
		// DO NOT USE THIS, PLEASE MAKE SURE THE SUBSCRIPTION DOES NOT CONTAIN THE BUSINESSUNIT INSTEAD!
		private static readonly StringCollection TypesWithBusinessUnitException = new StringCollection
		{
			typeof(IStatisticTask).Name
		};

		private string dataSource()
		{
			return TypesWithDatasourceException.Contains(DomainType) ? null : DataSource;
		}

		private string businessUnit()
		{
			var emptyId = IdToString(Guid.Empty);
			return (TypesWithBusinessUnitException.Contains(DomainType) || (TypesWithDatasourceException.Contains(DomainType) && emptyId.Equals(DomainId))) ? emptyId : BusinessUnitId;
		}

		public string MailboxId { get; set; }

		public string DataSource { get; set; }
		public string BusinessUnitId { get; set; }

		public string DomainType { get; set; }
		public string DomainId { get; set; }
		public string DomainReferenceId { get; set; }
		public string DomainReferenceType { get; set; }
		public string LowerBoundary { get; set; }
		public string UpperBoundary { get; set; }
		public bool Base64BinaryData { get; set; }


		public Guid BusinessUnitIdAsGuid()
		{
			return XmlConvert.ToGuid(BusinessUnitId);
		}

		public DateTime LowerBoundaryAsDateTime()
		{
			return LowerBoundary == null ? Consts.MinDate : AsDateTime(LowerBoundary);
		}

		public DateTime UpperBoundaryAsDateTime()
		{
			return UpperBoundary == null ? Consts.MaxDate : AsDateTime(UpperBoundary);
		}

		public static DateTime AsDateTime(string dateTime)
		{
			return XmlConvert.ToDateTime(dateTime.Substring(1), XmlDateTimeSerializationMode.Unspecified);
		}

		public static string IdToString(Guid id)
		{
			return XmlConvert.ToString(id);
		}

		public static string DateToString(DateTime date)
		{
			return "D" + XmlConvert.ToString(date, XmlDateTimeSerializationMode.Unspecified);
		}

	}
}