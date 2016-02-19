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
    public class SimulateMyTimeScreen : SimulateBase<MyTimeData>
    {
        public SimulateMyTimeScreen(IMessageBrokerUrl url, ICurrentDataSource dataSource, ICurrentBusinessUnit businessUnit, ICurrentScenario scenario, IJsonSerializer serializer, IMessageBrokerComposite messageBroker) : base(url, dataSource, businessUnit, scenario, serializer, messageBroker)
        {
        }

        public override void Simulate(MyTimeData data, EventHandler<EventMessageArgs> callback)
        {
            AddSubscription(new Subscription
            {
                DomainType = typeof(IScheduleChangedInDefaultScenario).Name,
                DomainReferenceId = data.User.ToString(),
                DataSource = DataSource.CurrentName(),
                BusinessUnitId = Subscription.IdToString(BusinessUnit.Current().Id.Value),
                LowerBoundary = Subscription.DateToString(Consts.MinDate),
                UpperBoundary = Subscription.DateToString(Consts.MaxDate),
            }, callback);

            AddSubscription(new Subscription
            {
                DomainType = typeof(IScheduleChangedInDefaultScenario).Name,
                DomainReferenceId = data.User.ToString(),
                DataSource = DataSource.CurrentName(),
                BusinessUnitId = Subscription.IdToString(BusinessUnit.Current().Id.Value),
                LowerBoundary = Subscription.DateToString(Consts.MinDate),
                UpperBoundary = Subscription.DateToString(Consts.MaxDate),
            }, callback);

            AddSubscription(new Subscription
            {
                DomainType = typeof(IPushMessageDialogue).Name,
                DomainReferenceId = data.User.ToString(),
                DataSource = DataSource.CurrentName(),
                BusinessUnitId = Subscription.IdToString(BusinessUnit.Current().Id.Value),
                LowerBoundary = Subscription.DateToString(Consts.MinDate),
                UpperBoundary = Subscription.DateToString(Consts.MaxDate),
            }, callback);
        }
    }
}