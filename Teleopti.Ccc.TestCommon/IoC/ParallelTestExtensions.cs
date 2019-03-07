using System;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public static class ParallelTestExtensions
	{
		public static bool isParallel(this TestContext instance)
		{
			string workerId;
			try
			{
				workerId = instance.WorkerId;
			}
			// VS test runner doesnt support the new nunit features properly.
			// exception here means the runner doesnt support the parallel test feature.
			// possibly this will be fixed with nunit 3.12?
			// https://github.com/nunit/nunit-vs-adapter/issues/166
			// https://github.com/nunit/nunit/issues/2696
			// https://github.com/nunit/nunit/milestone/36
			catch (NullReferenceException) 
			{
				return false;
			}
			return new Regex(@"Worker#[0-9]+")
				.IsMatch(workerId);
		}
	}
}