﻿using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.PeformanceTool;
using Teleopti.Ccc.Web.Core.Aop.Aspects;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers
{
    public class CreateTemporaryPersonsController : Controller
    {
	    private readonly ITestPersonCreator _testPersonCreator;

	    public CreateTemporaryPersonsController(ITestPersonCreator testPersonCreator)
	    {
		    _testPersonCreator = testPersonCreator;
	    }

	    [UnitOfWork, HttpGet]
	    public void CreatePersons(int numberOfPersons)
	    {
			_testPersonCreator.CreatePersons(numberOfPersons);
	    }

		[UnitOfWork, HttpGet]
	    public void RemoveCreatedData()
	    {
			_testPersonCreator.RemoveCreatedPersons();
	    }
    }
}
