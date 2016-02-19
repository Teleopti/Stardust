using System;
using System.Net.Http;
using System.Text;
using Teleopti.Ccc.Web.BrokenListenSimulator.SimulationData;

namespace Teleopti.Ccc.Web.BrokenListenSimulator.TrafficSimulators
{
    public class SimulateMyTimeScreenTraffic : TrafficSimulatorBase<MyTimeData>
    {       
        public override void Simulate(MyTimeData data)
        {
            AddFullDayAbsense(data.User, DateTime.Today);

            AddFullDayAbsense(data.User, DateTime.Today+TimeSpan.FromDays(1));
        }

        public void AddFullDayAbsense(Guid user, DateTime day)
        {
            var body =
                string.Format(
                    "{{\"StartDate\":\"{2}\",\"EndDate\":\"{2}\",\"AbsenceId\":\"4b4c15f0-5c3c-479e-8f9f-9bb900b80624\",\"PersonId\":\"{0}\",\"TrackedCommandInfo\":{{\"TrackId\":\"{1}\"}}}}",
                    user, Guid.NewGuid(), day.Date);
            var message = new HttpRequestMessage(HttpMethod.Post, "api/PersonScheduleCommand/AddFullDayAbsence")
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            var response = HttpClient.SendAsync(message).GetAwaiter().GetResult();
            Console.WriteLine("Added full day absense");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Unable to generate schedule change");
            }
        }
    }
}