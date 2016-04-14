using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
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
		public static ICollection<Task<HttpResponseMessage>> AllTasks = new SynchronizedCollection<Task<HttpResponseMessage>>();

        public SimulateMyTimeScreen(IMessageBrokerUrl url, ICurrentDataSource dataSource, ICurrentBusinessUnit businessUnit, ICurrentScenario scenario, IJsonSerializer serializer, IMessageBrokerComposite messageBroker) : base(url, dataSource, businessUnit, scenario, serializer, messageBroker)
        {
			
        }

        public override void Simulate(MyTimeData data, Action callback)
        {
            AddSubscription(new Subscription
            {
                DomainType = typeof(IScheduleChangedInDefaultScenario).Name,
                DomainReferenceId = data.User.ToString(),
                DataSource = DataSource.CurrentName(),
                BusinessUnitId = Subscription.IdToString(BusinessUnit.Current().Id.Value),
                LowerBoundary = Subscription.DateToString(Consts.MinDate),
                UpperBoundary = Subscription.DateToString(Consts.MaxDate),
            }, (sender, args) =>
            {
				callback();
				CallbackAction();
            });

            AddSubscription(new Subscription
            {
                DomainType = typeof(IScheduleChangedInDefaultScenario).Name,
                DomainReferenceId = data.User.ToString(),
                DataSource = DataSource.CurrentName(),
                BusinessUnitId = Subscription.IdToString(BusinessUnit.Current().Id.Value),
                LowerBoundary = Subscription.DateToString(Consts.MinDate),
                UpperBoundary = Subscription.DateToString(Consts.MaxDate),
			}, (sender, args) => callback());

            AddSubscription(new Subscription
            {
                DomainType = typeof(IPushMessageDialogue).Name,
                DomainReferenceId = data.User.ToString(),
                DataSource = DataSource.CurrentName(),
                BusinessUnitId = Subscription.IdToString(BusinessUnit.Current().Id.Value),
                LowerBoundary = Subscription.DateToString(Consts.MinDate),
                UpperBoundary = Subscription.DateToString(Consts.MaxDate),
			}, (sender, args) => callback());
        }

		public override void CallbackAction()
		{
			AllTasks.Add(FetchSchedule());
		}

	    public override void LogOn(MyTimeData data)
	    {
		    LogOn(data.BusinessUnitName, data.Username, data.Password);
	    }

	    public Task<HttpResponseMessage> FetchSchedule()
		{
			Console.WriteLine("FetchData for date {0}", DateTime.Today);
			var message = new HttpRequestMessage(HttpMethod.Get, string.Format("api/Schedule/FetchData?date=&_={0}", Guid.NewGuid()));
			
			var response = HttpClient.SendAsync(message);
			return response;
		}
    }
}