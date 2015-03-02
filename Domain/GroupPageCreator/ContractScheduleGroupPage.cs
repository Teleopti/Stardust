using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
	/// <summary>
	/// Populates ContractSchedule's of a GroupPage
	/// </summary>
	/// <remarks>
	/// Created by: Sachintha Weerasekara
	/// Created date: 7/8/2008
	/// </remarks>
	public class ContractScheduleGroupPage : IGroupPageCreator<IContractSchedule>
	{
		public IGroupPage CreateGroupPage(IEnumerable<IContractSchedule> entityCollection, IGroupPageOptions groupPageOptions)
		{
			//Create the GroupPage object
			IGroupPage groupPage = new GroupPage(groupPageOptions.CurrentGroupPageName) { DescriptionKey = groupPageOptions.CurrentGroupPageNameKey };

			var allPersonPeriods = (from p in groupPageOptions.Persons
								   from pp in p.PersonPeriods(groupPageOptions.SelectedPeriod)
								   select
									   new
									   {
										   Person = p,
										   pp.PersonContract.ContractSchedule
									   }).ToList();

			var connectedContractSchedules = new HashSet<IContractSchedule>();
			foreach (var ppp in allPersonPeriods)
			{
				connectedContractSchedules.Add(ppp.ContractSchedule);
			}

			foreach (IContractSchedule contractSchedule in entityCollection.OrderBy(c => c.Description.Name))
			{
				if (((IDeleteTag)contractSchedule).IsDeleted) continue;

				//Create a root Group Object & add into GroupPage
				IRootPersonGroup rootGroup = new RootPersonGroup(contractSchedule.Description.Name);
				if (!groupPage.IsUserDefined())
					rootGroup.SetId(contractSchedule.Id);

				if (connectedContractSchedules.Contains(contractSchedule))
				{
					var schedule = contractSchedule;
					var personPeriodsWithContractSchedule =
						allPersonPeriods.Where(p => schedule.Equals(p.ContractSchedule));
					var personsWithContractSchedule =
						personPeriodsWithContractSchedule.Select(pp => pp.Person).Distinct();
					foreach (var personWithContractSchedule in personsWithContractSchedule)
					{
						rootGroup.AddPerson(personWithContractSchedule);
					}
				}

				//Add into GroupPage
				groupPage.AddRootPersonGroup(rootGroup);
			}
			return groupPage;
		}
	}
}