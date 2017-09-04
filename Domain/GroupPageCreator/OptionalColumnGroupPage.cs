using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
	public class OptionalColumnGroupPage : IGroupPageCreator<IOptionalColumn>
	{
		public IGroupPage CreateGroupPage(IEnumerable<IOptionalColumn> entityCollection, IGroupPageOptions groupPageOptions)
		{
			if(!entityCollection.Any())
				return null;
			var optionalColumn = entityCollection.First();

			var groupPage = new GroupPage(optionalColumn.Name);
			groupPage.SetId(optionalColumn.Id.Value);
			var groups = new Dictionary<string, IList<IPerson>>();


			foreach (var person in groupPageOptions.Persons)
			{
				var columnValue = person.OptionalColumnValueCollection
					.FirstOrDefault(y => y.Parent.Id.Value == optionalColumn.Id.Value && y.Description != string.Empty);
				if(columnValue == null)
					continue;
				if (groups.ContainsKey(columnValue.Description))
					groups[columnValue.Description].Add(person);
				else
					groups.Add(columnValue.Description, new List<IPerson> {person});
			}

			foreach (var group in groups)
			{
				IRootPersonGroup rootGroup = new RootPersonGroup(group.Key)
				{
					Entity = optionalColumn
				};
				foreach (var person in group.Value)
				{
					rootGroup.AddPerson(person);
				}
				rootGroup.SetId(Guid.NewGuid());
				groupPage.AddRootPersonGroup(rootGroup);
			}

			return groupPage.RootGroupCollection.Any()? groupPage:null;
		}
	}
}