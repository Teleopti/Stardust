using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
	[TestFixture]
	public class OverrideTaskTest
	{
		private OverrideTask target;
		private OverrideTask targetCopy;
		private const double tasks = 9.11;
		private readonly TimeSpan averageTaskTime = TimeSpan.FromSeconds(112);
		private readonly TimeSpan averageAfterTaskTime = TimeSpan.FromSeconds(118);

		[SetUp]
		public void Setup()
		{
			target = new OverrideTask(tasks, averageTaskTime, averageAfterTaskTime);
			targetCopy = new OverrideTask(tasks, averageTaskTime, averageAfterTaskTime);
		}

		[Test]
		public void ShouldGetProperties()
		{
			Assert.AreEqual(target.OverrideTasks, tasks);
			Assert.AreEqual(target.OverrideAverageTaskTime, averageTaskTime);
			Assert.AreEqual(target.OverrideAverageAfterTaskTime, averageAfterTaskTime);
		}

		[Test]
		public void VerifyEqualsWork()
		{
			var task = new OverrideTask(1, averageTaskTime, averageAfterTaskTime);

			Assert.IsTrue(target.Equals(targetCopy));
			Assert.IsFalse(new ServiceAgreement().Equals(null));
			Assert.AreEqual(target, target);
			Assert.IsFalse(new ServiceAgreement().Equals(3));
			Assert.IsFalse(target.Equals(task));
		}

		[Test]
		public void VerifyEqualsReturnsFalseIfParameterIsNullAndParameterIsTask()
		{
			OverrideTask testObject = null;
			Assert.IsFalse(target.Equals(testObject));
		}

		[Test]
		public void VerifyEqualsReturnsFalseIfParameterIsNull()
		{
			object testObject = null;
			Assert.IsFalse(target.Equals(testObject));
		}

		[Test]
		public void VerifyGetHashCodeWorks()
		{
			IDictionary<OverrideTask, int> dic = new Dictionary<OverrideTask, int>();
			dic[target] = 5;

			Assert.AreEqual(5, dic[target]);
		}

		[Test]
		public void PropertiesInitializesCorrect()
		{
			target = new OverrideTask();
			Assert.AreEqual(target.OverrideTasks, null);
			Assert.AreEqual(target.OverrideAverageTaskTime, null);
			Assert.AreEqual(target.OverrideAverageAfterTaskTime, null);
		}
	}
}