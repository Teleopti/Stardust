using System;
using System.Linq;
using System.Xml;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker
{
	[Serializable]
	public class Message
	{
		public Message()
		{
			DomainId = Guid.Empty.ToString();
			DomainReferenceId = DomainId;
			BusinessUnitId = DomainId;
			ModuleId = DomainId;
			StartDate = Subscription.DateToString(DateTime.MinValue);
			EndDate = Subscription.DateToString(DateTime.MinValue);
		}

		public string[] Routes()
		{
			var dataSources = new[] {DataSource};
			var businessUnits = new[] {BusinessUnitId};
			var domainTypes = new[] {DomainType};
			var domainIds = new[] {DomainId, null};
			var domainReferenceIds = new[] {DomainReferenceId, null};
			var shortTermOrNot = new string[] {null};

			if (StartDateAsDateTime() < DateTime.UtcNow.AddDays(3) && EndDateAsDateTime() > DateTime.UtcNow.AddDays(-3))
				shortTermOrNot = new[] {Subscription.ShortTerm, null};

			var routes = from dataSource in dataSources
				from businessUnit in businessUnits
				from domainType in domainTypes
				from domainId in domainIds
				from domainReferenceId in domainReferenceIds
				from shortTerm in shortTermOrNot
				let route = makeRoute(
					dataSource,
					businessUnit,
					domainType,
					domainId,
					domainReferenceId,
					shortTerm)
				where route != null
				select route;
			return routes.ToArray();
		}

		private static string makeRoute(string dataSource, string businessUnit, string domainType, string domainId,
			string domainReferenceId, string shortTerm)
		{
			// exclude this combo for some reason
			if (domainReferenceId != null && domainId != null)
				return null;


			var route = String.Join(Subscription.Separator, dataSource, businessUnit, domainType);
			if (shortTerm != null)
				route = String.Join(Subscription.Separator, route, shortTerm);
			if (domainId != null)
				route = String.Join(Subscription.Separator, route, "id", domainId);
			if (domainReferenceId != null)
				route = String.Join(Subscription.Separator, route, "ref", domainReferenceId);
			return route;
		}

		public string DataSource { get; set; }
		public string BusinessUnitId { get; set; }

		public string DomainType { get; set; }
		public string DomainQualifiedType { get; set; }
		public string DomainId { get; set; }
		public string ModuleId { get; set; }
		public string DomainReferenceId { get; set; }
		public string EndDate { get; set; }
		public string StartDate { get; set; }
		public int DomainUpdateType { get; set; }
		public string BinaryData { get; set; }
		public string TrackId { get; set; }


		public Guid DomainIdAsGuid()
		{
			return XmlConvert.ToGuid(DomainId);
		}

		public Guid DomainReferenceIdAsGuid()
		{
			return XmlConvert.ToGuid(DomainReferenceId);
		}

		public Guid ModuleIdAsGuid()
		{
			return XmlConvert.ToGuid(ModuleId);
		}

		public Guid BusinessUnitIdAsGuid()
		{
			return XmlConvert.ToGuid(BusinessUnitId);
		}

		public DateTime StartDateAsDateTime()
		{
			return Subscription.AsDateTime(StartDate);
		}

		public DateTime EndDateAsDateTime()
		{
			return Subscription.AsDateTime(EndDate);
		}

		public DomainUpdateType DomainUpdateTypeAsDomainUpdateType()
		{
			return (DomainUpdateType) DomainUpdateType;
		}
	}
}