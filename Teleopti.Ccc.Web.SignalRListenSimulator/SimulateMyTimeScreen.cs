using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.SignalRListenSimulator
{
	public class SimulateMyTimeScreen 
    {
	    private readonly ICurrentDataSource _dataSource;
	    private readonly ICurrentBusinessUnit _businessUnit;
	    private readonly IMessageBrokerComposite _messageBroker;
	    private string _number;

	    public SimulateMyTimeScreen(
			ICurrentDataSource dataSource, 
			ICurrentBusinessUnit businessUnit, 
			IMessageBrokerComposite messageBroker)
	    {
		    _dataSource = dataSource;
		    _businessUnit = businessUnit;
		    _messageBroker = messageBroker;
		}

		public void Simulate(SimulationData data, int screen, int client)
		{
			_number = $"#{screen}/{client}";

			AddSubscription(new Subscription
			{
				DomainType = typeof(IScheduleChangedInDefaultScenario).Name,
				DomainReferenceId = data.User.ToString(),
				DataSource = _dataSource.CurrentName(),
				BusinessUnitId = Subscription.IdToString(_businessUnit.Current().Id.Value),
				LowerBoundary = Subscription.DateToString(Consts.MinDate),
				UpperBoundary = Subscription.DateToString(Consts.MaxDate),
			}, callback);

			AddSubscription(new Subscription
			{
				DomainType = typeof(IPushMessageDialogue).Name,
				DomainReferenceId = data.User.ToString(),
				DataSource = _dataSource.CurrentName(),
				BusinessUnitId = Subscription.IdToString(_businessUnit.Current().Id.Value),
				LowerBoundary = Subscription.DateToString(Consts.MinDate),
				UpperBoundary = Subscription.DateToString(Consts.MaxDate),
			}, callback);
		}
		
	    private void AddSubscription(Subscription subscription, EventHandler<EventMessageArgs> callback)
		{
			_messageBroker.RegisterSubscription(subscription, callback);
		}

		private void callback(object sender, EventMessageArgs e)
		{
			Console.WriteLine($"{_number} callbacked");
		}
	}

}