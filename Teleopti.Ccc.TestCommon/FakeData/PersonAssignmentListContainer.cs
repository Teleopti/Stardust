using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class PersonAssignmentListContainer
	{
		private readonly IList<ISkill> _allSkills;
		private readonly IDictionary<string, ISkill> _containedSkills;
		private readonly IList<IProjectionService> _projectionServices;
		public IList<IPersonAssignment> PersonAssignmentListForActivityDividerTest { get; private set; }
		public IScenario Scenario { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PersonAssignmentListContainer"/> class.
		/// </summary>
		public PersonAssignmentListContainer(IList<IProjectionService> projectionServices)
		{
			_projectionServices = projectionServices;
			_allSkills = new List<ISkill>();
			_containedSkills = new Dictionary<string, ISkill>();
			ContainedActivities = new Dictionary<string, IActivity>();
			ContainedPersons = new Dictionary<string, IPerson>();
			PersonAssignmentListForActivityDividerTest = new List<IPersonAssignment>();
		}
		
		public IDictionary<string, ISkill> ContainedSkills => _containedSkills;
		
		public IList<ISkill> AllSkills => _allSkills;
		
		public IDictionary<string, IActivity> ContainedActivities { get; }
		
		public IDictionary<string, IPerson> ContainedPersons { get; }

		public IList<IVisualLayerCollection> TestVisualLayerCollection()
		{
			IList<IVisualLayerCollection> ret = new List<IVisualLayerCollection>();
			foreach (var projectionService in _projectionServices)
			{
				var projection = projectionService.CreateProjection();
				ret.Add(projection);
			}

			return ret;
		}

		public IList<IFilteredVisualLayerCollection> TestFilteredVisualLayerCollectionWithSamePerson()
		{
			IPerson person = _projectionServices[0].CreateProjection().Person;

			return _projectionServices
				.Select(projectionService => projectionService.CreateProjection())
				.Select(projection => new FilteredVisualLayerCollection(person, projection.ToList(), new ProjectionIntersectingPeriodMerger(), projection))
				.Take(2)
				.Cast<IFilteredVisualLayerCollection>().ToList();
		}
	}
}