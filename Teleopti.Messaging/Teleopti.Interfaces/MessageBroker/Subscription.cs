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
		private static readonly StringCollection TypesWithException = new StringCollection {typeof (IExternalAgentState).Name};

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
			var basicRoute = String.Join("/", stringArray);
			
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

		private string excludeDatasourceForCertainTypes()
		{
			return TypesWithException.Contains(DomainType) ? null : DataSource;
		}

		private string excludeBusinessUnitForCertainTypes()
		{
			var emptyId = IdToString(Guid.Empty);
			return TypesWithException.Contains(DomainType) && emptyId.Equals(DomainId) ? emptyId : BusinessUnitId;
		}
	}
}