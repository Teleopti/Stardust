using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_LessResourcesXXL_74915)]
	public class CreateMergedCollection
	{
		public virtual Lazy<IEnumerable<IVisualLayer>> Execute(IProjectionMerger projectionMerger, IVisualLayer[] unmergedCollection)
		{
			return new Lazy<IEnumerable<IVisualLayer>>(() => projectionMerger.MergedCollection(unmergedCollection).ToList());
		}
	}
	
	[RemoveMeWithToggle(Toggles.ResourcePlanner_LessResourcesXXL_74915)]
	public class CreateMergedCollectionNoState : CreateMergedCollection
	{
		public override Lazy<IEnumerable<IVisualLayer>> Execute(IProjectionMerger projectionMerger, IVisualLayer[] unmergedCollection)
		{
			return null;
		}
	}
	
	/// <summary>
	/// Collection for visual layers.
	/// Note! The layercollection sent to this instance must be sorted correctly,
	/// old one first, new latest!
	/// No checks for this right now because of performance. Add later if necessary.
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2008-02-26
	/// </remarks>
	public class VisualLayerCollection : IVisualLayerCollection
	{
		private readonly IProjectionMerger _merger;
		[RemoveMeWithToggle(Toggles.ResourcePlanner_LessResourcesXXL_74915)]
		private readonly Lazy<IEnumerable<IVisualLayer>> _mergedCollection;
		
		private readonly Lazy<DateTimePeriod?> _period;
		private readonly Lazy<LayerCollectionNumbers> _timeNumbers;

		private Lazy<IFilterOnPeriodOptimizer> _periodOptimizer = new Lazy<IFilterOnPeriodOptimizer>(()=>new NextPeriodOptimizer());
		
		public VisualLayerCollection(IEnumerable<IVisualLayer> layerCollection, IProjectionMerger merger)
		{
			UnMergedCollection = layerCollection.ToArray();
			_merger = merger;
			HasLayers = UnMergedCollection.Length > 0;
			_period = new Lazy<DateTimePeriod?>(extractPeriod);
			_timeNumbers = new Lazy<LayerCollectionNumbers>(() =>
			{
				var ret = new LayerCollectionNumbers();
				foreach (VisualLayer layer in UnMergedCollection)
				{
					ret = new LayerCollectionNumbers(
						ret.WorkTime.Add(layer.WorkTime()),
						ret.ContractTime.Add(layer.ThisLayerContractTime()),
						ret.PaidTime.Add(layer.PaidTime()),
						ret.OverTime.Add(layer.DefinitionSet != null &&
										 layer.HighestPriorityAbsence == null &&
										 layer.DefinitionSet.MultiplicatorType == MultiplicatorType.Overtime ? layer.WorkTime() : TimeSpan.Zero));
				}
				return ret;
			});
			_mergedCollection = ServiceLocatorForLegacy.CreateMergedCollection.Execute(merger, UnMergedCollection);
		}

		[RemoveMeWithToggle("Remove condition", Toggles.ResourcePlanner_LessResourcesXXL_74915)]
		public IEnumerable<IVisualLayer> MergedCollection()
		{
			return _mergedCollection == null ? ((IProjectionMerger)_merger.Clone()).MergedCollection(UnMergedCollection).ToList() : _mergedCollection.Value;
		} 

		public static IVisualLayerCollection CreateEmptyProjection(IPerson assignedPerson)
		{
			return new VisualLayerCollection(Enumerable.Empty<IVisualLayer>(), new NoProjectionMerger());
		}

		public bool HasLayers { get; }
		
		public IFilterOnPeriodOptimizer PeriodOptimizer
		{
			get { return _periodOptimizer.Value; }
			set
			{
				_periodOptimizer = new Lazy<IFilterOnPeriodOptimizer>(()=>value);
			}
		}

		internal IVisualLayer[] UnMergedCollection { get; private set; }


		public bool IsSatisfiedBy(ISpecification<IVisualLayerCollection> specification)
		{
			return specification.IsSatisfiedBy(this);
		}

		public TimeSpan WorkTime()
		{
			return _timeNumbers.Value.WorkTime;
		}

		public TimeSpan PaidTime()
		{
			return _timeNumbers.Value.PaidTime;
		}

		public TimeSpan Overtime()
		{
			return _timeNumbers.Value.OverTime;
		}

		public TimeSpan ContractTime()
		{
			return _timeNumbers.Value.ContractTime;
		}

		//borde göras om till en IProjectionMerger
		public IFilteredVisualLayerCollection FilterLayers<TPayload>() where TPayload : IPayload
		{
			return new FilteredVisualLayerCollection(UnMergedCollection.Where(l => l.Payload is TPayload).ToList(), (IProjectionMerger)_merger.Clone(),this);
		}

		public TimeSpan ContractTime(DateTimePeriod filterPeriod)
		{
			var ret = TimeSpan.Zero;
			foreach (VisualLayer layer in UnMergedCollection)
			{
				var sharedPeriod = layer.Period.Intersection(filterPeriod);
				if (!sharedPeriod.HasValue) continue;

				var layerContractTime = layer.ThisLayerContractTime();
				if (layerContractTime>TimeSpan.Zero)
				{
					ret = ret.Add(sharedPeriod.Value.ElapsedTime());
				}
			}
			return ret;
		}

		public TimeSpan Overtime(DateTimePeriod filterPeriod)
		{
			var ret = TimeSpan.Zero;
			foreach (VisualLayer layer in UnMergedCollection)
			{
				if (layer.DefinitionSet == null) continue;
				if (layer.HighestPriorityAbsence == null &&
				    layer.DefinitionSet.MultiplicatorType == MultiplicatorType.Overtime)
				{
					var sharedPeriod = layer.Period.Intersection(filterPeriod);
					if (!sharedPeriod.HasValue) continue;

					var overtime = layer.WorkTime() > TimeSpan.Zero ? sharedPeriod.Value.ElapsedTime() : TimeSpan.Zero;
					ret = ret.Add(overtime);
				}
			}
			return ret;
		}

		public TimeSpan PaidTime(DateTimePeriod filterPeriod)
		{
			var ret = TimeSpan.Zero;
			foreach (VisualLayer layer in UnMergedCollection)
			{
				var sharedPeriod = layer.Period.Intersection(filterPeriod);
				if (!sharedPeriod.HasValue) continue;

				var layerPaidTime = layer.ThisLayerPaidTime();
				if (layerPaidTime > TimeSpan.Zero)
				{
					ret = ret.Add(sharedPeriod.Value.ElapsedTime());
				}
			}
			return ret;
		}

		public TimeSpan WorkTime(DateTimePeriod filterPeriod)
		{
			var ret = TimeSpan.Zero;
			foreach (VisualLayer layer in UnMergedCollection)
			{
				var sharedPeriod = layer.Period.Intersection(filterPeriod);
				if (!sharedPeriod.HasValue) continue;

				var layerWorkTime = layer.ThisLayerWorkTime();
				if (layerWorkTime > TimeSpan.Zero)
				{
					ret = ret.Add(sharedPeriod.Value.ElapsedTime());
				}
			}
			return ret;
		}

		public TimeSpan PlannedOvertime(DateTimePeriod filterPeriod)
		{
			var ret = TimeSpan.Zero;
			foreach (VisualLayer layer in UnMergedCollection)
			{
				var sharedPeriod = layer.Period.Intersection(filterPeriod);
				if (!sharedPeriod.HasValue) continue;

				if (layer.DefinitionSet != null && layer.HighestPriorityActivity.InWorkTime)
				{
					ret = ret.Add(sharedPeriod.Value.ElapsedTime());
				}
			}
			return ret;
		}

		//borde göras om till en IProjectionMerger
		public IFilteredVisualLayerCollection FilterLayers(IPayload payloadToSearch)
		{
			IList<IVisualLayer> retColl = new List<IVisualLayer>();
			foreach (IVisualLayer layer in UnMergedCollection)
			{
				if (layer.Payload.OptimizedEquals(payloadToSearch))
					retColl.Add(layer);
			}
			return new FilteredVisualLayerCollection(retColl, (IProjectionMerger)_merger.Clone(),this);
		}

		//borde göras om till en IProjectionMerger
		public IFilteredVisualLayerCollection FilterLayers(DateTimePeriod periodToSearch)
		{
			IVisualLayerFactory visualLayerFactory = new VisualLayerFactory();
			IList<IVisualLayer> retColl = new List<IVisualLayer>();
			int collCount = UnMergedCollection.Length;
			if (collCount > 0)
			{
				DateTime endDateTimeSearch = periodToSearch.EndDateTime;
				DateTime startDateTimeSearch = periodToSearch.StartDateTime;
				IFilterOnPeriodOptimizer opt = PeriodOptimizer;

				int foundIndex = collCount - 1; //if no hits
				int startIndex = 0;
				if (collCount > 1)
					startIndex = opt.FindStartIndex(UnMergedCollection, startDateTimeSearch);

				for (int index = startIndex; index < collCount; index++)
				{
					IVisualLayer layer = UnMergedCollection[index];
					DateTimePeriod layerPeriod = layer.Period;
					DateTime layerPeriodStartDateTime = layerPeriod.StartDateTime;

					if (endDateTimeSearch <= layerPeriodStartDateTime)
					{
						foundIndex = index == 0 ? 0 : index - 1;
						break;
					}
					DateTimePeriod? intersectionPeriod = layerPeriod.Intersection(periodToSearch);
					if (intersectionPeriod.HasValue)
					{
						var newLayer = visualLayerFactory.CreateResultLayer(layer.Payload, layer, intersectionPeriod.Value);
						retColl.Add(newLayer);
						foundIndex = index;
					}
				}
				opt.FoundEndIndex(foundIndex);
			}
			return new FilteredVisualLayerCollection(retColl, (IProjectionMerger)_merger.Clone(), this);
		}

		public int Count()
		{
			return MergedCollection().Count();
		}

		public IEnumerator GetEnumerator()
		{
			return MergedCollection().GetEnumerator();
		}

		public DateTimePeriod? Period()
		{
			return _period.Value;
		}

		private DateTimePeriod? extractPeriod()
		{
			switch (UnMergedCollection.Length)
			{
				case 0:
					return null;
				case 1:
					return UnMergedCollection[0].Period;
				default:
					var startDateTime = UnMergedCollection[0].Period.StartDateTime;
					var endDateTime = UnMergedCollection[UnMergedCollection.Length - 1].Period.EndDateTime;
					if (endDateTime < startDateTime)
					{
						throw new ArgumentOutOfRangeException(string.Format("The start datetime cannot be greater than the layer end datetime. ({0} - {1})", startDateTime, endDateTime));
					}
					return
						new DateTimePeriod(startDateTime,
										   endDateTime);
			}
		}

		IEnumerator<IVisualLayer> IEnumerable<IVisualLayer>.GetEnumerator()
		{
			return MergedCollection().GetEnumerator();
		}

		private struct LayerCollectionNumbers
		{
			public LayerCollectionNumbers(TimeSpan workTime, TimeSpan contractTime, TimeSpan paidTime, TimeSpan overTime) : this()
			{
				WorkTime = workTime;
				ContractTime = contractTime;
				PaidTime = paidTime;
				OverTime = overTime;
			}

			public TimeSpan WorkTime { get; private set; }
			public TimeSpan ContractTime { get; private set; }
			public TimeSpan OverTime { get; private set; }
			public TimeSpan PaidTime { get; private set; }
		}
	}

	public class FilteredVisualLayerCollection : VisualLayerCollection,IFilteredVisualLayerCollection
	{
		public FilteredVisualLayerCollection(IEnumerable<IVisualLayer> layerCollection, IProjectionMerger merger, IVisualLayerCollection original) : base(layerCollection, merger)
		{
			if (original!=null)
			{
				OriginalProjectionPeriod = original.Period();
			}
		}

		public DateTimePeriod? OriginalProjectionPeriod { get; private set; }
	}
}
