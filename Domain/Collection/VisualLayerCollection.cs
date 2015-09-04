using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
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
		private IFilterOnPeriodOptimizer _periodOptimizer;
		private readonly IProjectionMerger _merger;
		private readonly Lazy<DateTimePeriod?> _period;
		private readonly Lazy<TimeSpan> _contractTime;

		public VisualLayerCollection(IPerson assignedPerson, IList<IVisualLayer> layerCollection, IProjectionMerger merger)
		{
			UnMergedCollection = layerCollection;
			Person = assignedPerson;
			_merger = merger;
			HasLayers = UnMergedCollection.Count > 0;
			_period = new Lazy<DateTimePeriod?>(extractPeriod);
			_contractTime = new Lazy<TimeSpan>(() =>
			{
				var ret = TimeSpan.Zero;
				foreach (VisualLayer layer in UnMergedCollection)
				{
					ret = ret.Add(layer.ThisLayerContractTime());
				}
				return ret;
			});
		}

		public bool HasLayers { get; private set; }

		public IPerson Person { get; private set; }

		public IFilterOnPeriodOptimizer PeriodOptimizer
		{
			get
			{
				if (_periodOptimizer == null)
					_periodOptimizer = new NextPeriodOptimizer();
				return _periodOptimizer;
			}
			set { _periodOptimizer = value; }
		}

		internal IList<IVisualLayer> UnMergedCollection { get; private set; }


		public bool IsSatisfiedBy(ISpecification<IVisualLayerCollection> specification)
		{
			return specification.IsSatisfiedBy(this);
		}

		public TimeSpan ReadyTime()
		{
			var ret = TimeSpan.Zero;
			foreach (VisualLayer layer in UnMergedCollection)
			{
				ret = ret.Add(layer.ReadyTime());
			}
			return ret;
		}

		public TimeSpan WorkTime()
		{
			var ret = TimeSpan.Zero;
			foreach (VisualLayer layer in UnMergedCollection)
			{
				ret = ret.Add(layer.WorkTime());
			}
			return ret;
		}

		public TimeSpan PaidTime()
		{
			var ret = TimeSpan.Zero;
			foreach (VisualLayer layer in UnMergedCollection)
			{
				ret = ret.Add(layer.PaidTime());
			}
			return ret;
		}

		public TimeSpan Overtime()
		{
			var ret = TimeSpan.Zero;
			foreach (VisualLayer layer in UnMergedCollection)
			{
				if (layer.DefinitionSet != null)
				{
					if (layer.HighestPriorityAbsence == null && 
						layer.DefinitionSet.MultiplicatorType == MultiplicatorType.Overtime)
						ret = ret.Add(layer.WorkTime());
				}
			}
			return ret;
		}

		public IDictionary<string, TimeSpan> TimePerDefinitionSet()
		{
			IDictionary<string, TimeSpan> result = new Dictionary<string, TimeSpan>();
			foreach (VisualLayer layer in UnMergedCollection)
			{
				if (layer.DefinitionSet != null)
				{
					string definitionSetName = layer.DefinitionSet.Name;
					if (!result.ContainsKey(definitionSetName))
						result.Add(definitionSetName, TimeSpan.Zero);

					result[definitionSetName] = result[definitionSetName].Add(layer.Period.ElapsedTime());
				}
			}
			return result;
		}

		public TimeSpan ContractTime()
		{
			return _contractTime.Value;
		}

		//borde göras om till en IProjectionMerger
		public IFilteredVisualLayerCollection FilterLayers<TPayload>() where TPayload : IPayload
		{
			return new FilteredVisualLayerCollection(Person, UnMergedCollection.Where(l => l.Payload is TPayload).ToList(), (IProjectionMerger)_merger.Clone(),this);
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

		//borde göras om till en IProjectionMerger
		public IFilteredVisualLayerCollection FilterLayers(IPayload payloadToSearch)
		{
			IList<IVisualLayer> retColl = new List<IVisualLayer>();
			foreach (IVisualLayer layer in UnMergedCollection)
			{
				if (layer.Payload.OptimizedEquals(payloadToSearch))
					retColl.Add(layer);
			}
			return new FilteredVisualLayerCollection(Person, retColl, (IProjectionMerger)_merger.Clone(),this);
		}

		//borde göras om till en IProjectionMerger
		public IFilteredVisualLayerCollection FilterLayers(DateTimePeriod periodToSearch)
		{
			IVisualLayerFactory visualLayerFactory = new VisualLayerFactory();
			IList<IVisualLayer> retColl = new List<IVisualLayer>();
			int collCount = UnMergedCollection.Count;
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
						IVisualLayer newLayer = visualLayerFactory.CreateResultLayer(layer.Payload, layer, intersectionPeriod.Value);
						retColl.Add(newLayer);
						foundIndex = index;
					}
				}
				opt.FoundEndIndex(foundIndex);
			}
			return new FilteredVisualLayerCollection(Person, retColl, (IProjectionMerger)_merger.Clone(), this);
		}

		public int Count()
		{
			return _merger.MergedCollection(UnMergedCollection, Person).Count;
		}

		public IEnumerator GetEnumerator()
		{
			return ((IEnumerable<IVisualLayer>)this).GetEnumerator();
		}

		public DateTimePeriod? Period()
		{
			return _period.Value;
		}

		private DateTimePeriod? extractPeriod()
		{
			switch (UnMergedCollection.Count)
			{
				case 0:
					return null;
				case 1:
					return UnMergedCollection[0].Period;
				default:
					return
						new DateTimePeriod(UnMergedCollection[0].Period.StartDateTime,
										   UnMergedCollection[UnMergedCollection.Count - 1].Period.EndDateTime);
			}
		}

		IEnumerator<IVisualLayer> IEnumerable<IVisualLayer>.GetEnumerator()
		{
			return _merger.MergedCollection(UnMergedCollection, Person).GetEnumerator();
		}
	}

	public class FilteredVisualLayerCollection : VisualLayerCollection,IFilteredVisualLayerCollection
	{
		public FilteredVisualLayerCollection(IPerson assignedPerson, IList<IVisualLayer> layerCollection, IProjectionMerger merger, IVisualLayerCollection original) : base(assignedPerson, layerCollection, merger)
		{
			if (original!=null)
			{
				OriginalProjectionPeriod = original.Period();
			}
		}

		public DateTimePeriod? OriginalProjectionPeriod { get; private set; }
	}
}
