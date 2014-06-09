using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.PeformanceTool;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers
{
    public class RtaController : Controller
    {
	    private readonly ITestPersonCreator _testPersonCreator;

	    public RtaController(ITestPersonCreator testPersonCreator)
	    {
		    _testPersonCreator = testPersonCreator;
	    }

	    [UnitOfWork, HttpGet]
	    public void CreatePersons(int numberOfPersons)
	    {
			_testPersonCreator.CreatePersons(numberOfPersons);
	    }
    }
}
