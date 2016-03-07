using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core.Stardust;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TestFixture]
	class StardustControllerTest
	{
		public StardustController Target;
		public StardustHelper StardustHelper;
		public StardustRepository StardustRepository;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			StardustRepository = new StardustRepository();
			StardustHelper = new StardustHelper(StardustRepository);
			Target = new StardustController(StardustHelper);
		}

		[Test]
		public void JobHistoryListShouldNotCrash()
		{
			Target.JobHistoryList();
		}
	}
}
