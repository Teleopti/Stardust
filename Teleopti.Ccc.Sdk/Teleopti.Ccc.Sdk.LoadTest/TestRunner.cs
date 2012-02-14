using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Teleopti.Ccc.Sdk.LoadTest
{
	public class TestRunner
	{
		private readonly int _count;
		private readonly int _step;
		private readonly TimeSpan _stepTime;
		private readonly TimeSpan _intervalTime;
		private readonly string _serviceUrl;
		private readonly IUserSource _userSource;

		public TestRunner(int count, int step, TimeSpan stepTime, TimeSpan intervalTime, string serviceUrl, IUserSource userSource)
		{
			_count = count;
			_serviceUrl = serviceUrl;
			_userSource = userSource;
			_step = step;
			_stepTime = stepTime;
			_intervalTime = intervalTime;
		}

		public void Start()
		{
			var tests = from n in Enumerable.Range(1, _count) select MakeTask(n);
			Tests = tests.ToArray();
			/*var thread = new Thread(() => Tests.ForEach(x => x.StartAsync()));
			thread.Start();*/
			// 4.0 only
			var startTask = Tests.Select(x => new Task(x.ThreadMethod));
			startTask.ForEach(t => t.Start());
		}

		private TestTask MakeTask(int testNumber)
		{
			var user = _userSource.GetUser(testNumber);
			var scenario = new LoginScenario(_serviceUrl, user.Name, user.Password);
			var delayTime = GetDelayTime(testNumber);
			return new TestTask(delayTime, _intervalTime, scenario);
		}

		public IEnumerable<TestTask> Tests { get; private set; }

		private TimeSpan GetDelayTime(int testNumber)
		{
			var testStep = (testNumber - 1) / _step;
			return TimeSpan.FromTicks(_stepTime.Ticks*testStep);
		}
	}

}