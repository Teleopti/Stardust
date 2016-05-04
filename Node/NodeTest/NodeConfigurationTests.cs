using System;
using System.Reflection;
using NUnit.Framework;
using Stardust.Node;

namespace NodeTest
{
	internal class NodeConfigurationTests
	{
		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowExceptionWhenBaseAddressIsNull()
		{
			new NodeConfiguration(null,
				new Uri("http://localhost:5000"),
				Assembly.Load("NodeTest.JobHandlers"),
				"test",
				 1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenManagerLocationIsNull()
		{
			new NodeConfiguration(new Uri("http://localhost:5000"),
				null,
				Assembly.Load("NodeTest.JobHandlers"),
				"test",
				 1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenHandlerAssemblyIsNull()
		{
			new NodeConfiguration(new Uri("http://localhost:5000"),
				new Uri("http://localhost:5000"),
				null,
				"test",
				 1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenNodeNameIsNull()
		{
			new NodeConfiguration(new Uri("http://localhost:5000"),
				new Uri("http://localhost:5000"),
				Assembly.Load("NodeTest.JobHandlers"),
				null,
				 1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenPingToManagerSecondsIsZero()
		{
			new NodeConfiguration(new Uri("http://localhost:5000"),
				new Uri("http://localhost:5000"),
				Assembly.Load("NodeTest.JobHandlers"),
				"test",
				 0);
		}
	}
}