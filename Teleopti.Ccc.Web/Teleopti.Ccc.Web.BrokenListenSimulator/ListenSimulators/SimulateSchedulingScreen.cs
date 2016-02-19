using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Web.BrokenListenSimulator.SimulationData;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.BrokenListenSimulator.ListenSimulators
{
    public class SimulateSchedulingScreen : SimulateBase<SchedulingData>
    {
        public SimulateSchedulingScreen(IMessageBrokerUrl url, ICurrentDataSource dataSource, ICurrentBusinessUnit businessUnit, ICurrentScenario scenario, IJsonSerializer serializer, IMessageBrokerComposite messageBroker) : base(url, dataSource, businessUnit, scenario, serializer, messageBroker)
        {
        }

        public override void Simulate(SchedulingData data, EventHandler<EventMessageArgs> callback)
        {
            AddSubscription(new Subscription
            {
                DomainType = typeof(IScheduleChangedEvent).Name,
                DomainReferenceId = Subscription.IdToString(Scenario.Current().Id.Value),
                DomainReferenceType = typeof(Scenario).AssemblyQualifiedName,
                LowerBoundary = Subscription.DateToString(data.StartDate),
                UpperBoundary = Subscription.DateToString(data.EndDate),
                DataSource = DataSource.CurrentName(),
                BusinessUnitId = Subscription.IdToString(BusinessUnit.Current().Id.Value),
            }, callback);

            AddSubscription(new Subscription
            {
                DomainType = typeof(IPersistableScheduleData).Name,
                DomainReferenceId = null,
                DomainReferenceType = null,
                LowerBoundary = Subscription.DateToString(data.StartDate),
                UpperBoundary = Subscription.DateToString(data.EndDate),
                DataSource = DataSource.CurrentName(),
                BusinessUnitId = Subscription.IdToString(BusinessUnit.Current().Id.Value),
            }, callback);

            AddSubscription(new Subscription
            {
                DomainType = typeof(IMeeting).Name,
                DomainReferenceId = null,
                DomainReferenceType = null,
                LowerBoundary = Subscription.DateToString(Consts.MinDate),
                UpperBoundary = Subscription.DateToString(Consts.MaxDate),
                DataSource = DataSource.CurrentName(),
                BusinessUnitId = Subscription.IdToString(BusinessUnit.Current().Id.Value),
            }, callback);

            AddSubscription(new Subscription
            {
                DomainType = typeof(IPersonRequest).Name,
                DomainReferenceId = null,
                DomainReferenceType = null,
                LowerBoundary = Subscription.DateToString(Consts.MinDate),
                UpperBoundary = Subscription.DateToString(Consts.MaxDate),
                DataSource = DataSource.CurrentName(),
                BusinessUnitId = Subscription.IdToString(BusinessUnit.Current().Id.Value),
            }, callback);
        }
    }
}