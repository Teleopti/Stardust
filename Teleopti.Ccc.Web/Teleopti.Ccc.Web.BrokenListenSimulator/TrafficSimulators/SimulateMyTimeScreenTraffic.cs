using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.BrokenListenSimulator.SimulationData;

namespace Teleopti.Ccc.Web.BrokenListenSimulator.TrafficSimulators
{
    public class SimulateMyTimeScreenTraffic : TrafficSimulatorBase<MyTimeData>
    {       
        public override void AddFullDayAbsenceForThePersonByNextNDays(MyTimeData data, int days)
        {
			var today = DateTime.Today;
			var allTasks = new List<Task>();
	        for (var i = 0; i < days; i++)
	        {
				var dateTime = today + TimeSpan.FromDays(i);
				var fullDayAbsenseAsync = addFullDayAbsenseAsync(data.User, dateTime, dateTime, data.AbsenseId);
				allTasks.Add(fullDayAbsenseAsync);
				Console.WriteLine("Added full day absense for 1 person for {0} days", days);
	        }
			requestsStatus(allTasks);
        }

	    public override void AddFullDayAbsenceForAllPeopleWithPartTimePercentage100ByNextNDays(MyTimeData data, int days)
	    {
		    var today = DateTime.Today;
		    var people = GetPeopleForPartTimePercentaget100(today);
		    var allTasks = new List<Task>();
			for (var i = 0; i < days; i++)
		    {
			    var li = i;
			    people.ForEach(p =>
				{
					var dateTime = today + TimeSpan.FromDays(li);
					var fullDayAbsenseAsync = addFullDayAbsenseAsync(p, dateTime, dateTime, data.AbsenseId);
					allTasks.Add(fullDayAbsenseAsync);
				});
				Console.WriteLine("Added full day absense for {0} people for {1} days", people.Count(), days);
		    }
		    requestsStatus(allTasks);
	    }

	    private static void requestsStatus(List<Task> allTasks)
	    {
		    var stopwatch = new Stopwatch();
		    stopwatch.Start();

		    while (stopwatch.IsRunning && stopwatch.Elapsed < TimeSpan.FromSeconds(600))
		    {
			    Thread.Sleep(TimeSpan.FromSeconds(5));
			    Console.WriteLine("addFullDayAbsenseAsync {0} completed,{1} not completed, {2} faulted, {3} canceled", allTasks.Count(x => x.IsCompleted),
					allTasks.Count(x => !x.IsCompleted), allTasks.Count(x => x.IsFaulted), allTasks.Count(x => x.IsCanceled));
			    if (allTasks.All(x => x.IsCompleted))
			    {
				    break;
			    }
		    }
	    }

	    public override IEnumerable<Guid> GetPeopleForPartTimePercentaget100(DateTime day)
	    {
		    var message = new HttpRequestMessage(HttpMethod.Get, string.Format("api/GroupSchedule/Get?groupId={0}&date={1}", "b7eda58d-c0a0-4051-b648-9b5e015b240e", day.Date));
			var response = HttpClient.SendAsync(message).GetAwaiter().GetResult();
			Console.WriteLine("GetScheduleForPartTimePercentaget100");
			if (!response.IsSuccessStatusCode)
			{
				throw new Exception("Unable to GetScheduleForPartTimePercentaget100");
			}
			var result = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
			dynamic[] schedules = result.Schedules.ToObject<dynamic[]>();
			return schedules.Select(x => new Guid(x.PersonId.ToObject<string>()));
	    }

	    public override void CallbackAction()
        {
            //var taskList = new List<Task>();
            //Enumerable.Range(1, 1).ForEach(iteration =>
            //{
            //    taskList.Add(FetchSchedule());
            //});
            //Task.WaitAll(taskList.ToArray());
        }

        private Task<HttpResponseMessage> addFullDayAbsenseAsync(Guid user, DateTime start, DateTime end, Guid absenceId)
        {
            var body =
                string.Format(
                    "{{\"StartDate\":\"{2}\",\"EndDate\":\"{4}\",\"AbsenceId\":\"{3}\",\"PersonId\":\"{0}\",\"TrackedCommandInfo\":{{\"TrackId\":\"{1}\"}}}}",
                    user, Guid.NewGuid(), start.Date, absenceId, end.Date);
            var message = new HttpRequestMessage(HttpMethod.Post, "api/PersonScheduleCommand/AddFullDayAbsence")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            return HttpClient.SendAsync(message);
        }

        public Task FetchSchedule()
        {
            var message = new HttpRequestMessage(HttpMethod.Get, string.Format("MyTime/Schedule/FetchData?date=&_={0}", Guid.NewGuid()));
            var response = HttpClient.SendAsync(message);
            return response;
        }
    }
}