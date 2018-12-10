using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Multi;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Restriction;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	/// <summary>
	/// Repository for rotations
	/// </summary>
	public class RotationRepository : Repository<IRotation>, ILoadAggregateById<IRotation>
	{
		public RotationRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
			: base(unitOfWork)
#pragma warning restore 618
		{
		}

		/// <summary>
		/// Loads the name of all person rotations with hierarchy data sort by.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		///  Created by: Ola
		///  Created date: 2008-09-19    
		/// </remarks>
		public IList<IRotation> LoadAllRotationsWithHierarchyData()
		{
            DetachedCriteria rotation = DetachedCriteria.For<Rotation>("rotation")
                .Fetch("RotationDays");

		    DetachedCriteria rotationDay = DetachedCriteria.For<RotationDay>("rotationDay")
		        .Fetch("RestrictionCollection")
		        .Fetch("RestrictionCollection.ShiftCategory")
		        .Fetch("RestrictionCollection.DayOff");

            var result = Session.CreateQueryBatch()
                .Add<Rotation>(rotation)
                .Add<RotationDay>(rotationDay);

            ICollection<IRotation> rotations = CollectionHelper.ToDistinctGenericCollection<IRotation>(result.GetResult<Rotation>(0));
			return rotations.ToList();
		}

        public IList<IRotation> LoadAllRotationsWithDays()
        {
            IList<IRotation> retList = Session.CreateCriteria(typeof(Rotation))
                .Fetch("RotationDays")
                .SetResultTransformer(Transformers.DistinctRootEntity)
                .List<IRotation>();

            return retList;
        }

		public IEnumerable<IRotation> LoadRotationsWithHierarchyData(IEnumerable<IPerson> persons, DateOnly startDate)
		{
			var retList = new HashSet<IRotation>();
			foreach (var personsPart in persons.Batch(400))
			{
				var tempResult = Session.GetNamedQuery("LoadRotationsWithHierarchyData")
						.SetDateOnly("StartDate", startDate)
						.SetParameterList("PersonCollection", personsPart.ToArray())
						.List<IRotation>();
				foreach (var rotation in tempResult)
				{
					retList.Add(rotation);
				}
			}
			return retList;
		}

		public IRotation LoadAggregate(Guid id)
		{
			IRotation ret = Session.CreateCriteria(typeof (Rotation))
				.Fetch("RotationDays")
				.Fetch("RotationDays.RestrictionCollection")
				.Fetch("RotationDays.RestrictionCollection.ShiftCategory")
				.Fetch("RotationDays.RestrictionCollection.DayOff")
				.Add(Restrictions.Eq("Id", id))
				.UniqueResult<IRotation>();
			return ret;
		}
	}
}
