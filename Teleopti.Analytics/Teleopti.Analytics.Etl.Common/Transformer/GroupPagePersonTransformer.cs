using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class GroupPagePersonTransformer : IGroupPagePersonTransformer
	{
		private readonly Func<ICommonStateHolder> _getGroupPageDataProvider;

		public GroupPagePersonTransformer(Func<ICommonStateHolder> getGroupPageDataProvider)
		{
			_getGroupPageDataProvider = getGroupPageDataProvider;
		}

		public IEnumerable<IGroupPage> UserDefinedGroupings
		{
			get
			{
				IEnumerable<IGroupPage> groupings = _getGroupPageDataProvider().UserDefinedGroupings(null);

				foreach (IGroupPage groupPage in groupings)
				{
					foreach (IRootPersonGroup rootGroup in groupPage.RootGroupCollection)
					{
						MovePersonsFromChildGroupToRoot(rootGroup);
					}
				}

				return groupings;
			}
		}

		public IList<IGroupPage> BuiltInGroupPages
		{
			get
			{
				var groupingsCreator = new GroupingsCreatorOptionalColumn(_getGroupPageDataProvider());
				return groupingsCreator.CreateBuiltInGroupPages(false);
			}
		}

		public void Transform(IList<IGroupPage> builtInGroupPages, IEnumerable<IGroupPage> userDefinedPages, DataTable table)
		{
			buildGroupPagePersonRows(builtInGroupPages, table);
			buildGroupPagePersonRows(userDefinedPages, table);
		}

		private static void buildGroupPagePersonRows(IEnumerable<IGroupPage> groupPageCollection, DataTable dataTable)
		{
			foreach (IGroupPage groupPage in groupPageCollection)
			{
				var gpCasted = groupPage as GroupPage;
				if (gpCasted == null)
					throw new InvalidCastException("Could not cast IGroupPage to GroupPage!");

				TranslateNameToEnglish(groupPage);

				foreach (var rootGroup in groupPage.RootGroupCollection)
				{
					foreach (var person in rootGroup.PersonCollection)
					{
						bool isCustomGroup = false;
						DataRow row = dataTable.NewRow();
						

						if (!groupPage.Id.HasValue)
							groupPage.SetId(Guid.NewGuid());
						if (!rootGroup.Id.HasValue)
							rootGroup.SetId(Guid.Empty);


						if (groupPage.DescriptionKey == null && !rootGroup.IsOptionalColumn)
						{
							isCustomGroup = true;
							row["group_page_name_resource_key"] = DBNull.Value;
						}
						else
						{
							row["group_page_name_resource_key"] = groupPage.DescriptionKey;
						}

						row["group_page_code"] = groupPage.Id.Value;
						row["group_page_name"] = groupPage.Description.Name;
						row["group_code"] = rootGroup.Id.Value;
						row["group_name"] = rootGroup.Name;
						row["group_is_custom"] = isCustomGroup;
						row["person_code"] = person.Id.Value;
						row["business_unit_code"] = gpCasted.GetOrFillWithBusinessUnit_DONTUSE().Id;
						row["business_unit_name"] = gpCasted.GetOrFillWithBusinessUnit_DONTUSE().Description.Name;
						row["datasource_id"] = 1;
						dataTable.Rows.Add(row);
					}
				}
			}
		}

		private static void TranslateNameToEnglish(IGroupPage groupPage)
		{
			if (groupPage.DescriptionKey != null)
			{
				// Translate built in group page name to english for cube
				CultureInfo englishCulture = CultureInfo.GetCultureInfo("en-GB");
				string englishTerm = Resources.ResourceManager.GetString(groupPage.DescriptionKey, englishCulture);
				groupPage.Description = new Description(englishTerm);
			}
		}

		private static void MovePersonsFromChildGroupToRoot(IRootPersonGroup rootGroup)
		{
			if (rootGroup.ChildGroupCollection != null)
			{
				IList<IChildPersonGroup> childGroupsToRemove = new List<IChildPersonGroup>();

				foreach (IChildPersonGroup childGroup in rootGroup.ChildGroupCollection)
				{
					childGroupsToRemove = getChildGroupsToRemoveAndMovePersons(rootGroup, childGroup);
				}
				// Remove obsolete child groups
				foreach (IChildPersonGroup childGroup in childGroupsToRemove)
				{
					rootGroup.RemoveChildGroup(childGroup);
				}
			}
		}

		private static List<IChildPersonGroup> getChildGroupsToRemoveAndMovePersons(IRootPersonGroup rootGroup, IChildPersonGroup childPersonGroup)
		{
			var groupsToRemove = new List<IChildPersonGroup>();

			if (childPersonGroup.PersonCollection != null)
			{
				foreach (IPerson person in childPersonGroup.PersonCollection)
				{
					rootGroup.AddPerson(person);
				}
			}

			groupsToRemove.Add(childPersonGroup);

			if (childPersonGroup.ChildGroupCollection != null)
			{
				foreach (IChildPersonGroup childGroup in childPersonGroup.ChildGroupCollection)
				{
					groupsToRemove.AddRange(getChildGroupsToRemoveAndMovePersons(rootGroup, childGroup));
				}
			}

			return groupsToRemove;
		}
	}
}
