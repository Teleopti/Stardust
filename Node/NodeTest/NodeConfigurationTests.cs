﻿using System;
using NUnit.Framework;
using Stardust.Node.API;
using Stardust.Node.Workers;

namespace NodeTest
{
	internal class NodeConfigurationTests
	{
		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void ShouldThrowExceptionWhenConstructorArgumentIsNull()
		{
			var nodeConfiguration = new NodeConfiguration(null,
			                                              null,
			                                              null,
			                                              null,
			                                              pingToManagerSeconds: 10);
		}
	}
}