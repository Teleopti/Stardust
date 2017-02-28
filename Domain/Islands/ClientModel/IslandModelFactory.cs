﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands.ClientModel
{
	public class IslandModelFactory
	{
		private readonly CreateIslands _createIslands;
		private readonly ReduceSkillGroups _reduceSkillGroups;
		private readonly IPeopleInOrganization _peopleInOrganization;

		public IslandModelFactory(CreateIslands createIslands, ReduceSkillGroups reduceSkillGroups, IPeopleInOrganization peopleInOrganization)
		{
			_createIslands = createIslands;
			_reduceSkillGroups = reduceSkillGroups;
			_peopleInOrganization = peopleInOrganization;
		}

		public IslandTopModel Create()
		{
			var period = DateOnly.Today.ToDateOnlyPeriod();
			var agents = _peopleInOrganization.Agents(period);

			var islandsModelBefore = createIslandsModel(new ReduceNoSkillGroups(), agents, period);
			var islandsModelAfter = createIslandsModel(_reduceSkillGroups, agents, period);

			var islandTopModel = new IslandTopModel
			{
				AfterReducing = islandsModelAfter,
				BeforeReducing = islandsModelBefore
			};

			return islandTopModel;
		}

		private IslandsModel createIslandsModel(IReduceSkillGroups reduceSkillGroups, IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			IEnumerable<Island> islands=null;
			var timeToGenerate = MeasureTime.Do(() => islands = _createIslands.Create(reduceSkillGroups, agents, period));
			var islandModels = (from Island island in islands select island.CreateClientModel()).ToList();
			islandModels.Sort();
			var islandsModel = new IslandsModel
			{
				Islands = islandModels,
				TimeToGenerateInMs = (int) Math.Ceiling(timeToGenerate.TotalMilliseconds)
			};
			islandsModel.NumberOfAgentsOnAllIsland = islandsModel.Islands.Sum(x => x.NumberOfAgentsOnIsland);
			return islandsModel;
		}
	}
}