using System;
using System.Xml;

namespace Teleopti.Interfaces.MessageBroker
{
	/// <summary>
	/// Subscription details for message broker
	/// </summary>
	[Serializable]
	public class Subscription
	{
		/// <summary>
		/// Creates a new instance of <see cref="Subscription"/>
		/// </summary>
		public Subscription()
		{
			DomainId = Guid.Empty.ToString();
			DomainReferenceId = DomainId;
			SubscriptionId = DomainId;
			BusinessUnitId = DomainId;
			LowerBoundary = DateToString(DateTime.MinValue);
			UpperBoundary = DateToString(DateTime.MinValue);
		}

		/// <summary>
		/// Gets the route for this subscription.
		/// </summary>
		/// <returns></returns>
		public string Route()
		{
			var emptyId = Guid.Empty.ToString();
			var stringArray = new[] { DataSource, BusinessUnitId, DomainType };
			var basicRoute = String.Join("/", stringArray);
			
			if (!DomainId.Equals(emptyId))
			{
				return String.Join("/", new[] {basicRoute, "id", DomainId});
			}

			if (!DomainReferenceId.Equals(emptyId))
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

		/// <summary>
		/// Gets or sets the subscription id.
		/// </summary>
		public string SubscriptionId { get; set; }

		///<summary>
		/// Gets or sets the data source.
		///</summary>
		public string DataSource { get; set; }

		/// <summary>
		/// Gets or sets the business unit id.
		/// </summary>
		public string BusinessUnitId { get; set; }

		/// <summary>
		/// Gets the domain id as guid.
		/// </summary>
		public Guid DomainIdAsGuid()
		{
			return XmlConvert.ToGuid(DomainId);
		}

		/// <summary>
		/// Gets the domain reference id as guid.
		/// </summary>
		public Guid DomainReferenceIdAsGuid()
		{
			return XmlConvert.ToGuid(DomainReferenceId);
		}

		/// <summary>
		/// Gets the subscription id as guid.
		/// </summary>
		public Guid SubscriptionIdAsGuid()
		{
			return XmlConvert.ToGuid(SubscriptionId);
		}

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
			return XmlConvert.ToDateTime(LowerBoundary, XmlDateTimeSerializationMode.Unspecified);
		}

		/// <summary>
		/// Gets the upper boundary as date time.
		/// </summary>
		public DateTime UpperBoundaryAsDateTime()
		{
			return XmlConvert.ToDateTime(UpperBoundary, XmlDateTimeSerializationMode.Unspecified);
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
			return XmlConvert.ToString(date, XmlDateTimeSerializationMode.Unspecified);
		}
	}
}