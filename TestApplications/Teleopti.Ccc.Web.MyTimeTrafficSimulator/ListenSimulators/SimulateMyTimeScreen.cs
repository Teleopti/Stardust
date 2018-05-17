using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Web.MyTimeTrafficSimulator.SimulationData;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.MyTimeTrafficSimulator.ListenSimulators
{
    public class SimulateMyTimeScreen : SimulateBase<MyTimeData>
    {
		public static readonly object Lock = new object();
		public static ICollection<Task> AllTasks = new SynchronizedCollection<Task>();

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
			lock (Lock)
				AllTasks.Add(Task.Factory.StartNew(FetchSchedule, TaskCreationOptions.LongRunning));
		}

	    public override void LogOn(MyTimeData data)
	    {
		    LogOn(data.BusinessUnitName, data.Username, data.Password);
	    }

	    public void FetchSchedule()
		{
			//Console.WriteLine("FetchData for date {0}", DateTime.Today);
			var message = new HttpRequestMessage(HttpMethod.Get, string.Format("api/Schedule/FetchData?date=&_={0}", Guid.NewGuid()));
			
			var response = HttpClient.SendAsync(message).Result;
			if (!response.IsSuccessStatusCode)
				throw new Exception("Asd");

		}
    }
}