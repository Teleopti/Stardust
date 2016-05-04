using System;
using NUnit.Framework;
using Stardust.Node;

namespace NodeTest
{
	internal class NodeConfigurationTests
	{
		[Test, Ignore]
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