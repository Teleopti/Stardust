using System;
using System.Net.Http;
using NUnit.Framework;
using Teleopti.Ccc.Domain;

namespace Teleopti.Ccc.WebTest
{
	public class PreserveStackTest
	{
		[Test]
		public void ShouldPreserveHttpRequestException()
		{
			var httpRequestException = new HttpRequestException("My request exception", new HttpRequestException("Once more..."));
			PreserveStack.For(httpRequestException);
			Assert.IsNull(httpRequestException.StackTrace);
		}

		[Test]
		public void ShouldPreserveSerializableException()
		{
			var exception = new ArgumentNullException("Message", new ArgumentException("Test"));
			Assert.IsNull(exception.StackTrace);
			PreserveStack.For(exception);
			Assert.IsNotNull(exception.StackTrace);
		}
	}
}