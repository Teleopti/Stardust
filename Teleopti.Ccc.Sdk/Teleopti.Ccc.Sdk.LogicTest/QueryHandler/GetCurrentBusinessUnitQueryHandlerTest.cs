using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetCurrentBusinessUnitQueryHandlerTest
	{
		private GetCurrentBusinessUnitQueryHandler target;

		[SetUp]
		public void Setup()
		{
			target = new GetCurrentBusinessUnitQueryHandler(new CurrentTeleoptiPrincipal(new ThreadPrincipalContext()));
		}

		[Test]
		public void ShouldGetTheCurrentBusinessUnit()
		{
			var result = target.Handle(new GetCurrentBusinessUnitQueryDto());
			result.First().Name.Should().Be.EqualTo(((TeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity).BusinessUnitName);
		}
	}
}
