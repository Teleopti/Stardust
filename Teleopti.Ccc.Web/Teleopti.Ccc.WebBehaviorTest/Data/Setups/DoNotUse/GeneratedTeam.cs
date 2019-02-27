using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public abstract class GeneratedTeam : IDataSetup
	{
		public ISite Site;
		public ITeam TheTeam;

		protected GeneratedTeam(ISite site)
		{
			Site = site;
		}

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			if (Site == null)
			{
				var site = new SiteConfigurable { BusinessUnit = DefaultBusinessUnit.BusinessUnit.Name };
				DataMaker.Data().Apply(site);
				Site = site.Site;
			}

			TheTeam = TeamFactory.CreateSimpleTeam(TeamNameGenerator.Generate());
			TheTeam.Site = Site;

			var teamRepository = TeamRepository.DONT_USE_CTOR(currentUnitOfWork);
			teamRepository.Add(TheTeam);
		}
	}

	public static class TeamNameGenerator
	{

		public static string Generate()
		{
			return NameQueue().Dequeue();
		}

		private static Queue<string> _nameQueue;

		private static Queue<string> NameQueue()
		{
			if (_nameQueue == null || _nameQueue.Count == 0)
			{
				var random = new Random(DateTime.Now.Millisecond);
				var uniqueNames = Names.Distinct();
				var randomlyOrderedNames = uniqueNames.OrderBy(n => random.Next(0, 1000000));
				_nameQueue = new Queue<string>(randomlyOrderedNames.ToArray());
			}
			return _nameQueue;
		}

		// generated from http://www.namegenerator.biz/team-name-generator.php just for fun
		// removed names starting with V or W because they were sorted differently sometimes and I dont find it worth finding out why =)
		private static readonly IEnumerable<string> Names = new[]
		                                                    	{
		                                                    		"The Bewildered Badgers",
		                                                    		"The Moaning Squids",
		                                                    		"The Scrawny Lemurs",
		                                                    		"The Talented Pelicans",
		                                                    		"The Large Foxes",
		                                                    		"The True Ferrets",
		                                                    		"The Illustrious Donkies",
		                                                    		"The Creepy Wasps",
		                                                    		"The Deafening Eagles",
		                                                    		"The Subdued Anteaters",
		                                                    		"The Adventurous Zebras",
		                                                    		"The Aspiring Echidnas",
		                                                    		"The Lazy Bats",
		                                                    		"The Envious Magpies",
		                                                    		"The Greasy Bats",
		                                                    		"The Panicky Dogfishes",
		                                                    		"The Omniscient Mallards",
		                                                    		"The Even Crabs",
		                                                    		"The Imperfect Herons",
		                                                    		"The Solid Foxes",
		                                                    		"The Dynamic Snails",
		                                                    		"The Longing Herons",
		                                                    		"The Nauseating Peafowls",
		                                                    		"The Dapper Wasps",
		                                                    		"The Succinct Rams",
		                                                    		"The Goofy Badgers",
		                                                    		"The Boundless Gooses",
		                                                    		"The Freezing Pelicans",
		                                                    		"The Innocent Anteaters",
		                                                    		"The Tangible Bison",
		                                                    		"The Living Beavers",
		                                                    		"The Four Rams",
		                                                    		"The Concerned Donkies",
		                                                    		"The Naive Wolves",
		                                                    		"The Stereotyped Bears",
		                                                    		"The Gusty Cockroaches",
		                                                    		"The Caring Owls",
		                                                    		"The Dirty Porcupines",
		                                                    		"The Silent Gooses",
		                                                    		"The Psychotic Partridges",
		                                                    		"The Aberrant Skunks",
		                                                    		"The Gorgeous Hornets",
		                                                    		"The Better Deers",
		                                                    		"The Fragile Eagles",
		                                                    		"The Graceful Pandas",
		                                                    		"The Insidious Eels",
		                                                    		"The Rich Kangaroos",
		                                                    		"The Half Zebras",
		                                                    		"The Seemly Mosquitos",
		                                                    		"The Thoughtless Herons",
		                                                    		"The Fluttering Foxes",
		                                                    		"The Certain Flys",
		                                                    		"The Remarkable Deers",
		                                                    		"The Protective Hamsters",
		                                                    		"The Doubtful Porcupines",
		                                                    		"The Actually Wasps",
		                                                    		"The Brown Hippopotamuss",
		                                                    		"The Aware Skunks",
		                                                    		"The Maniacal Armadillos",
		                                                    		"The Immense Dogfishes",
		                                                    		"The Bizarre Apes",
		                                                    		"The Quixotic Goats",
		                                                    		"The Bizarre Hippopotamuss",
		                                                    		"The Savory Ponies",
		                                                    		"The Unhealthy Whales",
		                                                    		"The Like Peafowls",
		                                                    		"The Lively Donkeys",
		                                                    		"The Flat Cranes",
		                                                    		"The Repulsive Seahorses",
		                                                    		"The Historical Llamas",
		                                                    		"The Sable Rats",
		                                                    		"The Lacking Ponies",
		                                                    		"The Mute Kangaroos",
		                                                    		"The Infamous Armadillos",
		                                                    		"The Historical Porcupines",
		                                                    		"The Disastrous Cranes",
		                                                    		"The Faulty Crows",
		                                                    		"The Sticky Donkeys",
		                                                    		"The Shut Gooses",
		                                                    		"The Zealous Panthers",
		                                                    		"The Lopsided Hippopotamuss",
		                                                    		"The Gaudy Yaks",
		                                                    		"The Standing Swans",
		                                                    		"The Neighborly Cats",
		                                                    		"The Eminent Camels",
		                                                    		"The Lowly Louses",
		                                                    		"The Eight Crows",
		                                                    		"The Calm Jackals",
		                                                    		"The Flagrant Jaguars",
		                                                    		"The Nimble Boars",
		                                                    		"The Unusual Moles",
		                                                    		"The Erect Kangaroos",
		                                                    		"The Abnormal Barracudas",
		                                                    		"The Rare Cougars",
		                                                    		"The Cute Humans",
		                                                    		"The Longing Ravens",
		                                                    		"The Dear Hawks",
		                                                    		"The Lazy Pandas",
		                                                    		"The Determined Monkeys",
		                                                    		"The Nutritious Aardvarks",
		                                                    		"The Diligent Humans",
		                                                    		"The Quaint Gazelles",
		                                                    		"The Comfortable Kangaroos",
		                                                    		"The Pointless Larks",
		                                                    		"The Goofy Mooses"
		                                                    	};

	}
}