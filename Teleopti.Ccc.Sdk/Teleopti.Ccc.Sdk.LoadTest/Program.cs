using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Services.Protocols;

namespace Teleopti.Ccc.Sdk.LoadTest
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var serviceUrl = args.ElementAt(0);

			var count = int.Parse(args.ElementAt(1)); // 100;
			var step = int.Parse(args.ElementAt(2)); //3;
			var stepDelay = TimeSpan.FromSeconds(int.Parse(args.ElementAt(3))); // 60;
			var intervalTime = TimeSpan.FromSeconds(int.Parse(args.ElementAt(4))); // 1;

			Console.Clear();
			Console.WindowWidth = 100;
			Console.WindowHeight = 50;

			var userSource = new FileUserSource("users.txt");
			var runner = new TestRunner(count, step, stepDelay, intervalTime, serviceUrl, userSource);
			runner.Start();

			while (true)
			{
				Thread.Sleep(TimeSpan.FromSeconds(1));
				Console.CursorLeft = 0;
				Console.CursorTop = 0;
				var stringBuilder = new StringBuilder();
				runner.Tests.ForEach(t => t.WriteStatus(s => stringBuilder.Append((s + new string(' ', 5)).Substring(0, 5))));
				Console.Write(stringBuilder);
			}

		}
	}
}