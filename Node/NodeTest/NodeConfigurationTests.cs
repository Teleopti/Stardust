using System;
using NUnit.Framework;
using Stardust.Node.Workers;

namespace NodeTest
{
	internal class NodeConfigurationTests
	{
		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowExceptionWhenConstructorArgumentIsNull()
		{
			new NodeConfiguration(null,
				null,
				null,
				null,
				pingToManagerSeconds: 10);
		}
	}
}