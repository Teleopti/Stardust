using System;
using System.Linq;
using System.Xml;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker
{
	[Serializable]
	public class Notification
	{
		public Notification()
		{
			DomainId = Guid.Empty.ToString();
			DomainReferenceId = DomainId;
			BusinessUnitId = DomainId;
			ModuleId = DomainId;
			StartDate = Subscription.DateToString(DateTime.MinValue);
			EndDate = Subscription.DateToString(DateTime.MinValue);
		}

		public virtual string[] Routes()
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

		public virtual string DataSource { get; set; }
		public virtual string BusinessUnitId { get; set; }

		public virtual string DomainType { get; set; }
		public virtual string DomainQualifiedType { get; set; }
		public virtual string DomainId { get; set; }
		public virtual string ModuleId { get; set; }
		public virtual string DomainReferenceId { get; set; }
		public virtual string DomainReferenceType { get; set; }
		public virtual string EndDate { get; set; }
		public virtual string StartDate { get; set; }
		public virtual int DomainUpdateType { get; set; }
		public virtual string BinaryData { get; set; }


		public virtual Guid DomainIdAsGuid()
		{
			return XmlConvert.ToGuid(DomainId);
		}

		public virtual Guid DomainReferenceIdAsGuid()
		{
			return XmlConvert.ToGuid(DomainReferenceId);
		}

		public virtual Guid ModuleIdAsGuid()
		{
			return XmlConvert.ToGuid(ModuleId);
		}

		public virtual Guid BusinessUnitIdAsGuid()
		{
			return XmlConvert.ToGuid(BusinessUnitId);
		}

		public virtual DateTime StartDateAsDateTime()
		{
			return Subscription.AsDateTime(StartDate);
		}

		public virtual DateTime EndDateAsDateTime()
		{
			return Subscription.AsDateTime(EndDate);
		}

		public virtual DomainUpdateType DomainUpdateTypeAsDomainUpdateType()
		{
			return (DomainUpdateType) DomainUpdateType;
		}
	}
}