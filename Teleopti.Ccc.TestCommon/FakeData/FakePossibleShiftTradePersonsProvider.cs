using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakePossibleShiftTradePersonsProvider : IPossibleShiftTradePersonsProvider
	{
		public void AddPerson(IPerson person)
		{
			personList.Add(person);
		}

		private IList<IPerson> personList=new List<IPerson>();

		public DatePersons RetrievePersons(ShiftTradeScheduleViewModelData shiftTradeArguments)
		{
			 
			var datePersons = new DatePersons
			{
				Date = shiftTradeArguments.ShiftTradeDate,
				Persons = personList
			};

			return datePersons;
		}

		public DatePersons RetrievePersons(ShiftTradeScheduleViewModelData shiftTradeArguments, Guid[] personIds)
		{
			var datePersons = new DatePersons
			{
				Date = shiftTradeArguments.ShiftTradeDate,
				Persons = personList
			};

			return datePersons;
		}
	}
}
