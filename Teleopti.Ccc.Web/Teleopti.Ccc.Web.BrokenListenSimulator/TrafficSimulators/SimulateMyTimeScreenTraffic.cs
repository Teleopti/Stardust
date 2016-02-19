using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Web.BrokenListenSimulator.SimulationData;

namespace Teleopti.Ccc.Web.BrokenListenSimulator.TrafficSimulators
{
    public class SimulateMyTimeScreenTraffic : TrafficSimulatorBase<MyTimeData>
    {       
        public override void Simulate(MyTimeData data)
        {
            AddFullDayAbsense(data.User, DateTime.Today, data.AbsenseId);

            AddFullDayAbsense(data.User, DateTime.Today + TimeSpan.FromDays(1), data.AbsenseId);

            AddFullDayAbsense(data.User, DateTime.Today + TimeSpan.FromDays(2), data.AbsenseId);
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

        public async void AddFullDayAbsense(Guid user, DateTime day, Guid absenceId)
        {
            var body =
                string.Format(
                    "{{\"StartDate\":\"{2}\",\"EndDate\":\"{2}\",\"AbsenceId\":\"{3}\",\"PersonId\":\"{0}\",\"TrackedCommandInfo\":{{\"TrackId\":\"{1}\"}}}}",
                    user, Guid.NewGuid(), day.Date, absenceId);
            var message = new HttpRequestMessage(HttpMethod.Post, "api/PersonScheduleCommand/AddFullDayAbsence")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            var response = await HttpClient.SendAsync(message);

            Console.WriteLine("Added full day absense");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Unable to generate schedule change");
            }
        }

        public Task FetchSchedule()
        {
            var message = new HttpRequestMessage(HttpMethod.Get, string.Format("MyTime/Schedule/FetchData?date=&_={0}", Guid.NewGuid()));
            var response = HttpClient.SendAsync(message);
            return response;
            //if (!response.IsSuccessStatusCode) throw new Exception("Unable to fetch schedule");
        }
    }
}