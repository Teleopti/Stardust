using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Teleopti.Ccc.Web.BrokenListenSimulator.SimulationData;
using Teleopti.Ccc.Web.TestApplicationsCommon;

namespace Teleopti.Ccc.Web.BrokenListenSimulator.TrafficSimulators
{
    public class SimulateMyTimeScreenTraffic : TrafficSimulatorBase
    {       
        public void AddFullDayAbsenceForThePersonByNextNDays(MyTimeData data, int days)
        {
			var today = DateTime.Today;
			var allTasks = new List<Task>();
	        allTasks.AddRange(Enumerable.Range(0, days).Select(day => Task.Factory.StartNew(() =>
			{
				var dateTime = today + TimeSpan.FromDays(day);
				addFullDayAbsence(data.User, dateTime, dateTime, data.AbsenseId);
				Console.WriteLine("Added full day absense for 1 person for {0} days", days);
			}, TaskCreationOptions.LongRunning)));
	        Task.WaitAll(allTasks.ToArray());
			requestsStatus(allTasks);
        }

	    public void AddFullDayAbsenceForAllPeopleWithPartTimePercentage100ByNextNDays(MyTimeData data, int days)
	    {
		    var today = DateTime.Today;
		    var people = GetPeopleForPartTimePercentaget100(today);
		    var allTasks = new List<Task>();
			for (var i = 0; i < days; i++)
		    {
			    var li = i;
				allTasks.AddRange(people.Select(p => Task.Factory.StartNew(() =>
				{
					var dateTime = today + TimeSpan.FromDays(li);
					addFullDayAbsence(p, dateTime, dateTime, data.AbsenseId);
				}, TaskCreationOptions.LongRunning)));
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

	    public IEnumerable<Guid> GetPeopleForPartTimePercentaget100(DateTime day)
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

        private void addFullDayAbsence(Guid user, DateTime start, DateTime end, Guid absenceId)
        {
            var body =
                string.Format(
                    "{{\"StartDate\":\"{2}\",\"EndDate\":\"{4}\",\"AbsenceId\":\"{3}\",\"PersonId\":\"{0}\",\"TrackedCommandInfo\":{{\"TrackId\":\"{1}\"}}}}",
                    user, Guid.NewGuid(), start.Date, absenceId, end.Date);
            var message = new HttpRequestMessage(HttpMethod.Post, "api/PersonScheduleCommand/AddFullDayAbsence")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            var response = HttpClient.SendAsync(message).Result;
			if (!response.IsSuccessStatusCode)
				throw new Exception("Unable to add full days absence");
        }
    }
}