using System;
using System.Collections.Specialized;
using System.Xml;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.MessageBroker
{
	/// <summary>
	/// Subscription details for message broker
	/// </summary>
	[Serializable]
	public class Subscription
	{
		private static readonly StringCollection TypesWithDatasourceException = new StringCollection
		{
			typeof (IActualAgentState).Name,
			"SiteAdherenceMessage",
			"TeamAdherenceMessage",
			"AgentsAdherenceMessage",
			"TrackingMessage"
		};
		private static readonly StringCollection TypesWithBusinessUnitException = new StringCollection {typeof(IStatisticTask).Name};

		/// <summary>
		/// Separator used for message broker subscriptions
		/// </summary>
		public const string Separator = "/";

		/// <summary>
		/// Route name for changes in near time from now (+/- 3 days)
		/// </summary>
		public const string ShortTerm = "ShortTerm";

		/// <summary>
		/// Creates a new instance of <see cref="Subscription"/>
		/// </summary>
		public Subscription()
		{
			DomainId = null;
			DomainReferenceId = null;
			BusinessUnitId = Guid.Empty.ToString();
			LowerBoundary = DateToString(DateTime.MinValue);
			UpperBoundary = DateToString(DateTime.MinValue);
		}

		/// <summary>
		/// Gets the route for this subscription.
		/// </summary>
		/// <returns></returns>
		public string Route()
		{
			var stringArray = new[] { excludeDatasourceForCertainTypes(), excludeBusinessUnitForCertainTypes(), DomainType };
			var basicRoute = String.Join(Separator, stringArray);
			
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

		/// <summary>
		/// Gets or sets the domain type.
		/// </summary>
		public string DomainType { get; set; }

		/// <summary>
		/// Gets or sets the domain id.
		/// </summary>
		public string DomainId { get; set; }

		/// <summary>
		/// Gets or sets the domain reference id.
		/// </summary>
		public string DomainReferenceId { get; set; }

		/// <summary>
		/// Gets or sets the domain reference type.
		/// </summary>
		public string DomainReferenceType { get; set; }

		/// <summary>
		/// Gets or sets the lower boundary.
		/// </summary>
		public string LowerBoundary { get; set; }

		/// <summary>
		/// Gets or sets the upper boundary.
		/// </summary>
		public string UpperBoundary { get; set; }

		///<summary>
		/// Gets or sets the data source.
		///</summary>
		public string DataSource { get; set; }

		/// <summary>
		/// Gets or sets the business unit id.
		/// </summary>
		public string BusinessUnitId { get; set; }

		/// <summary>
		/// Gets the business unit id as guid.
		/// </summary>
		public Guid BusinessUnitIdAsGuid()
		{
			return XmlConvert.ToGuid(BusinessUnitId);
		}

		/// <summary>
		/// Gets the lower boundary as date time.
		/// </summary>
		public DateTime LowerBoundaryAsDateTime()
		{
			return AsDateTime(LowerBoundary);
		}

		/// <summary>
		/// Gets the upper boundary as date time.
		/// </summary>
		public DateTime UpperBoundaryAsDateTime()
		{
			return AsDateTime(UpperBoundary);
		}

		///<summary>
		/// Convert the string to date time.
		///</summary>
		///<param name="dateTime"></param>
		///<returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static DateTime AsDateTime(string dateTime)
		{
			return XmlConvert.ToDateTime(dateTime.Substring(1), XmlDateTimeSerializationMode.Unspecified);
		}

		/// <summary>
		/// Gets a guid as a string value.
		/// </summary>
		public static string IdToString(Guid id)
		{
			return XmlConvert.ToString(id);
		}

		/// <summary>
		/// Gets a date as a string value.
		/// </summary>
		public static string DateToString(DateTime date)
		{
			return "D" + XmlConvert.ToString(date, XmlDateTimeSerializationMode.Unspecified);
		}

		private string excludeDatasourceForCertainTypes()
		{
			return TypesWithDatasourceException.Contains(DomainType) ? null : DataSource;
		}

		private string excludeBusinessUnitForCertainTypes()
		{
			var emptyId = IdToString(Guid.Empty);
			return (TypesWithBusinessUnitException.Contains(DomainType) || (TypesWithDatasourceException.Contains(DomainType) && emptyId.Equals(DomainId))) ? emptyId : BusinessUnitId;
		}
	}
}