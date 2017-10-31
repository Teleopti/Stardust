using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Common
{
	public class OrganizationSelectionProvider
	{
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly ISiteRepository _siteRepository;
		private const string SiteType = "Site";
		private const string TeamType = "Team";
		private const string BusinessUnitType = "BusinessUnit";

		public OrganizationSelectionProvider(ICurrentBusinessUnit currentBusinessUnit, ISiteRepository siteRepository)
		{
			_currentBusinessUnit = currentBusinessUnit;
			_siteRepository = siteRepository;
		}
		
		public OrganizationModel Provide(bool provideDynamicOptions)
		{
			IBusinessUnit businessUnit = _currentBusinessUnit.Current();
			var ret = 
				new OrganizationModel
				{
					BusinessUnit =
					new OrganizationChildModel
					{
						Name = businessUnit.Name,
						Id = businessUnit.Id.GetValueOrDefault(),
						Type = BusinessUnitType,
						ChildNodes =
						_siteRepository.LoadAll()
							.Select(
								s =>
									new OrganizationChildModel
									{
										Name = s.Description.Name,
										Id = s.Id.GetValueOrDefault(),
										Type = SiteType,
										ChildNodes =
										s.TeamCollection.Where(t => t.IsChoosable)
											.Select(t => new OrganizationChildModel { Name = t.Description.Name, Type = TeamType, Id = t.Id.GetValueOrDefault(), ChildNodes = new OrganizationChildModel[0]}).ToArray()
									})
							.ToArray()
					},
					DynamicOptions = provideDynamicOptions? GetDynamicOptions() : new DynamicOptions[0]

				};
			return ret;
		}

		private DynamicOptions[] GetDynamicOptions()
		{
			return Enum.GetValues(typeof(AvailableDataRangeOption)).OfType<AvailableDataRangeOption>()
				.Select(o => new DynamicOptions { RangeOption = o, Name = Enum.GetName(typeof(AvailableDataRangeOption), o)}).ToArray();
		}
	}

	public class OrganizationModel
	{
		public OrganizationChildModel BusinessUnit { get; set; }
		public DynamicOptions[] DynamicOptions { get; set; }
	}

	public class DynamicOptions
	{
		public AvailableDataRangeOption RangeOption { get; set; }
		public string Name { get; set; }
	}

	public class OrganizationChildModel
	{
		public string Name { get; set; }
		public Guid Id { get; set; }
		public string Type { get; set; }
		public OrganizationChildModel[] ChildNodes { get; set; }
	}
}