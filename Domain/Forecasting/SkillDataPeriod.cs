using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	/// <summary>
	/// SkillDataPeriod
	/// </summary>
	/// <remarks>
	/// Created by: peterwe
	/// Created date: 2007-11-01
	/// </remarks>
	public class SkillDataPeriod : AggregateEntity, ISkillDataPeriod
	{
		private ServiceAgreement _serviceAgreement;
		private SkillPersonData _skillPersonData;
		private readonly DateTimePeriod _period;
		private double? _manualAgents;
		private Percent _shrinkage = new Percent(0);
		private Percent _efficiency = new Percent(1);

		/// <summary>
		/// Initializes a new instance of the <see cref="SkillDataPeriod"/> class.
		/// </summary>
		/// <param name="serviceAgreement">The skill data.</param>
		/// <param name="skillPersonData">The skill person data.</param>
		/// <param name="period">The period.</param>
		/// <remarks>
		/// Created by: peterwe
		/// Created date: 2007-11-01
		/// </remarks>
		public SkillDataPeriod(ServiceAgreement serviceAgreement, SkillPersonData skillPersonData, DateTimePeriod period) : this()
		{
			_skillPersonData = skillPersonData;
			_serviceAgreement = serviceAgreement;
			_period = period;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SkillDataPeriod"/> class.
		/// </summary>
		/// <remarks>
		/// Created by: peterwe
		/// Created date: 2007-11-01
		/// </remarks>
		protected SkillDataPeriod() { }

		/// <summary>
		/// Gets or sets the service agreement.
		/// </summary>
		/// <value>The service agreement.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-02-14
		/// </remarks>
		public virtual ServiceAgreement ServiceAgreement
		{
			get { return _serviceAgreement; }
			set
			{
				if (value != _serviceAgreement)
				{
					_serviceAgreement = value;
					OnChangeSkillData();
				}
			}
		}

		/// <summary>
		/// Gets or sets the min occupancy.
		/// </summary>
		/// <value>The min occupancy.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-04-08
		/// </remarks>
		public virtual Percent MinOccupancy
		{
			get { return _serviceAgreement.MinOccupancy; }
			set
			{
				if (value != _serviceAgreement.MinOccupancy)
				{
					_serviceAgreement = _serviceAgreement.WithMinOccupancy(value);
					OnChangeSkillData();
				}
			}
		}

		/// <summary>
		/// Gets or sets the max occupancy.
		/// </summary>
		/// <value>The max occupancy.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-04-08
		/// </remarks>
		public virtual Percent MaxOccupancy
		{
			get { return _serviceAgreement.MaxOccupancy; }
			set
			{
				if (value != _serviceAgreement.MaxOccupancy)
				{
					_serviceAgreement = _serviceAgreement.WithMaxOccupancy(value);
					OnChangeSkillData();
				}
			}
		}

		/// <summary>
		/// Gets or sets the service level percent.
		/// </summary>
		/// <value>The service level percent.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-04-08
		/// </remarks>
		public virtual Percent ServiceLevelPercent
		{
			get { return _serviceAgreement.ServiceLevel.Percent; }
			set
			{
				if (value != _serviceAgreement.ServiceLevel.Percent)
				{
					_serviceAgreement = _serviceAgreement.WithServiceLevel(new ServiceLevel(value,_serviceAgreement.ServiceLevel.Seconds));
					OnChangeSkillData();
				}
			}
		}

		/// <summary>
		/// Gets or sets the service level seconds.
		/// </summary>
		/// <value>The service level seconds.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-04-08
		/// </remarks>
		public virtual double ServiceLevelSeconds
		{
			get { return _serviceAgreement.ServiceLevel.Seconds; }
			set
			{
				if (value != _serviceAgreement.ServiceLevel.Seconds)
				{
					_serviceAgreement = _serviceAgreement.WithServiceLevel(new ServiceLevel(_serviceAgreement.ServiceLevel.Percent, value));
					OnChangeSkillData();
				}
			}
		}
		/// <summary>
		/// Gets or sets the service level time span/ServiceLevelSeconds.
		/// </summary>
		/// <value>The service level time span.</value>
		/// <remarks>
		/// Created by: zoet
		/// Created date: 2008-08-27
		/// </remarks>
		public virtual TimeSpan ServiceLevelTimeSpan
		{
			get { return TimeSpan.FromSeconds(_serviceAgreement.ServiceLevel.Seconds); }
			set { ServiceLevelSeconds = value.TotalSeconds; }
		}

		/// <summary>
		/// Gets or sets the minimum persons.
		/// </summary>
		/// <value>The minimum persons.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-04-08
		/// </remarks>
		public virtual int MinimumPersons
		{
			get { return _skillPersonData.MinimumPersons; }
			set
			{
				if (value != _skillPersonData.MinimumPersons)
				{
					_skillPersonData = new SkillPersonData(value,_skillPersonData.MaximumPersons);
					OnChangeSkillData();
				}
			}
		}

		/// <summary>
		/// Gets or sets the maximum persons.
		/// </summary>
		/// <value>The maximum persons.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-04-08
		/// </remarks>
		public virtual int MaximumPersons
		{
			get { return _skillPersonData.MaximumPersons; }
			set
			{
				if (value != _skillPersonData.MaximumPersons)
				{
					_skillPersonData = new SkillPersonData(_skillPersonData.MinimumPersons, value);
					OnChangeSkillData();
				}
			}
		}

		/// <summary>
		/// Gets the period.
		/// </summary>
		/// <value>The period.</value>
		/// <remarks>
		/// Created by: peterwe
		/// Created date: 2007-11-01
		/// </remarks>
		public virtual DateTimePeriod Period
		{
			get { return _period; }
		}

		/// <summary>
		/// Gets or sets the agent skill data.
		/// </summary>
		/// <value>The agent skill data.</value>
		/// <remarks>
		/// Created by: peterwe
		/// Created date: 2007-11-02
		/// </remarks>
		public virtual SkillPersonData SkillPersonData
		{
			get { return _skillPersonData; }
			set
			{
				if (value != _skillPersonData)
				{
					_skillPersonData = value;
					OnChangeSkillData();
				}
			}
		}

		/// <summary>
		/// Gets or sets the shrinkage, the percentage that will decrease
		/// the staff. If 10% = 90 % of the staff, so this must be handled
		/// on calculation.
		/// </summary>
		/// <value>The shrinkage.</value>
		/// <remarks>
		/// Created by: zoet
		/// Created date: 2008-05-09
		/// </remarks>
		public virtual Percent Shrinkage
		{
			get { return _shrinkage; }
			set
			{
				if (value != _shrinkage)
				{
					_shrinkage = value;
					OnChangeSkillData();
				}
			}
		}

		public virtual Percent Efficiency
		{
			get { return _efficiency; }
			set
			{
				if (value != _efficiency)
				{
					_efficiency = value;
					OnChangeSkillData();
				}
			}
		}

		public virtual double? ManualAgents
		{
			get { return _manualAgents; }
			set
			{
				if (value != _manualAgents)
				{
					_manualAgents = value;
					OnChangeSkillData();
				}
			}
		}

		private void OnChangeSkillData()
		{
			ISkillDay parent = SkillDataParent();
			if (parent != null)
			{
				parent.ResetSkillStaffPeriods(this);
				parent.RecalculateStaff();
				parent.ClearTemplateName();
			}
		}

		private ISkillDay SkillDataParent()
		{
			return Parent as ISkillDay;
		}

		#region ICloneable Members

		///<summary>
		///Creates a new object that is a copy of the current instance.
		///</summary>
		///
		///<returns>
		///A new object that is a copy of this instance.
		///</returns>
		///<filterpriority>2</filterpriority>
		public virtual object Clone()
		{
			SkillDataPeriod retobj = (SkillDataPeriod)MemberwiseClone();
			retobj.SetId(null);
			return retobj;
		}

		#endregion

		public virtual ISkillDataPeriod NoneEntityClone()
		{
			var newSkillDataPeriod = (ISkillDataPeriod)MemberwiseClone();
			newSkillDataPeriod.SetId(null);
			return newSkillDataPeriod;
		}

		public virtual ISkillDataPeriod EntityClone()
		{
			var newSkillDataPeriod = (ISkillDataPeriod)MemberwiseClone();
			return newSkillDataPeriod;
		}

		/// <summary>
		/// Merges the specified list.
		/// </summary>
		/// <param name="list">The list.</param>
		/// <param name="parent">The parent.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: HenryG
		/// Created date: 2009-01-22
		/// </remarks>
		public static ISkillDataPeriod Merge(IList<ISkillDataPeriod> list, IEntity parent)
		{
			list = list.OrderBy(s => s.Period.StartDateTime).ToList();
			ISkillDataPeriod newSkillDataPeriod = new SkillDataPeriod(
				list[0].ServiceAgreement,
				list[0].SkillPersonData,
				new DateTimePeriod(list[0].Period.StartDateTime, list.Last().Period.EndDateTime));
			newSkillDataPeriod.Shrinkage = list[0].Shrinkage;
			newSkillDataPeriod.Efficiency = list[0].Efficiency;
			newSkillDataPeriod.ManualAgents = list[0].ManualAgents;
			newSkillDataPeriod.SetParent(parent);
			return newSkillDataPeriod;
		}
	}
}