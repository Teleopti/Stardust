using System.Web.Mvc;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.WebTest.Core.Startup
{
	public class RegisterAreasTest
	{
		[Test]
		public void ShouldRegisterAreas()
		{
			var area1 = MockRepository.GenerateMock<AreaRegistration>();
			var area2 = MockRepository.GenerateMock<AreaRegistration>();
			var registerArea = MockRepository.GenerateMock<IRegisterArea>();
			var findAreaRegistrations = MockRepository.GenerateMock<IFindAreaRegistrations>();
			var target = new RegisterAreas(registerArea, findAreaRegistrations);
			findAreaRegistrations.Expect(x => x.AreaRegistrations()).Return(new[] {area1, area2});

			target.Execute();

			registerArea.AssertWasCalled(x => x.Register(area1));
			registerArea.AssertWasCalled(x => x.Register(area2));
		}
	}
}