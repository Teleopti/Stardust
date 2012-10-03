﻿using System;
using System.Xml;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker
{
	/// <summary>
	/// A message broker notification
	/// </summary>
	[Serializable]
	public class Notification
	{
		/// <summary>
		/// Creates a new instance of <see cref="Notification"/>.
		/// </summary>
		public Notification()
		{
			DomainId = Guid.Empty.ToString();
			DomainReferenceId = DomainId;
			BusinessUnitId = DomainId;
			ModuleId = DomainId;
			StartDate = Subscription.DateToString(DateTime.MinValue);
			EndDate = Subscription.DateToString(DateTime.MinValue);
		}

		/// <summary>
		/// Gets the route for this subscription.
		/// </summary>
		/// <returns></returns>
		public string[] Routes()
		{
			var stringArray = new[] { DataSource, BusinessUnitId, DomainType };
			var basicRoute = String.Join(Subscription.Separator, stringArray);

			var idRoute = String.Join(Subscription.Separator, new[] { basicRoute, "id", DomainId ?? string.Empty });
			var referenceRoute = String.Join(Subscription.Separator, new[] { basicRoute, "ref", DomainReferenceId ?? string.Empty });

			string[] routes;
			if (StartDateAsDateTime() < DateTime.UtcNow.AddDays(3) && EndDateAsDateTime() > DateTime.UtcNow.AddDays(-3))
			{
				var basicShortTermRoute = String.Join(Subscription.Separator, new[] {basicRoute, Subscription.ShortTerm});
				var idShortTermRoute = String.Join(Subscription.Separator, new[] { basicShortTermRoute, "id", DomainId ?? string.Empty });
				var referenceShortTermRoute = String.Join(Subscription.Separator, new[] { basicShortTermRoute, "ref", DomainReferenceId ?? string.Empty });
				routes = new[] { basicRoute, idRoute, referenceRoute, basicShortTermRoute, idShortTermRoute, referenceShortTermRoute };
			}
			else
			{
				routes = new[] {basicRoute, idRoute, referenceRoute};
			}

			return routes;
		}

		/// <summary>
		/// Gets or sets the domain type.
		/// </summary>
		public string DomainType { get; set; }

		/// <summary>
		/// Gets or sets the domain type.
		/// </summary>
		public string DomainQualifiedType { get; set; }

		/// <summary>
		/// Gets or sets the domain id.
		/// </summary>
		public string DomainId { get; set; }

		/// <summary>
		/// Gets or sets the module id.
		/// </summary>
		public string ModuleId { get; set; }

		/// <summary>
		/// Gets or sets the domain reference id.
		/// </summary>
		public string DomainReferenceId { get; set; }

		/// <summary>
		/// Gets or sets the domain reference type.
		/// </summary>
		public string DomainReferenceType { get; set; }

		/// <summary>
		/// Gets or sets the end date.
		/// </summary>
		public string EndDate { get; set; }

		/// <summary>
		/// Gets or sets the start date.
		/// </summary>
		public string StartDate { get; set; }

		/// <summary>
		/// Gets or sets the domain update type.
		/// </summary>
		public int DomainUpdateType { get; set; }

		/// <summary>
		/// Gets or sets the binary data.
		/// </summary>
		public string BinaryData { get; set; }

		///<summary>
		/// Gets or sets the data source.
		///</summary>
		public string DataSource { get; set; }

		/// <summary>
		/// Gets or sets the business unit id.
		/// </summary>
		public string BusinessUnitId { get; set; }

		/// <summary>
		/// Returns the domain id as guid.
		/// </summary>
		/// <returns></returns>
		public Guid DomainIdAsGuid()
		{
			return XmlConvert.ToGuid(DomainId);
		}

		/// <summary>
		/// Returns the domain reference id as guid.
		/// </summary>
		/// <returns></returns>
		public Guid DomainReferenceIdAsGuid()
		{
			return XmlConvert.ToGuid(DomainReferenceId);
		}

		/// <summary>
		/// Returns the module id as guid.
		/// </summary>
		/// <returns></returns>
		public Guid ModuleIdAsGuid()
		{
			return XmlConvert.ToGuid(ModuleId);
		}

		/// <summary>
		/// Gets the business unit id as guid.
		/// </summary>
		public Guid BusinessUnitIdAsGuid()
		{
			return XmlConvert.ToGuid(BusinessUnitId);
		}

		/// <summary>
		/// Returns the start date as date time.
		/// </summary>
		/// <returns></returns>
		public DateTime StartDateAsDateTime()
		{
			return Subscription.AsDateTime(StartDate);
		}

		/// <summary>
		/// Returns the end date as date time.
		/// </summary>
		/// <returns></returns>
		public DateTime EndDateAsDateTime()
		{
			return Subscription.AsDateTime(EndDate);
		}

		/// <summary>
		/// Returns the domain update type as domain update type.
		/// </summary>
		/// <returns></returns>
		public DomainUpdateType DomainUpdateTypeAsDomainUpdateType()
		{
			return (DomainUpdateType)DomainUpdateType;
		}
	}
}