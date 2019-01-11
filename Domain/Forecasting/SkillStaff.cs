using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.UndoRedo;

namespace Teleopti.Ccc.Domain.Forecasting
{
	/// <summary>
	/// data needed to calculate staffing needs
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2008-04-09
	/// </remarks>
	public class SkillStaff : ISkillStaff
	{
	    private double _calculatedLoggedOn;   //antal personer - resursberäkning
		private double _calculatedOccupancy;  //erlangsnurrar - erlangsnurran
		private double _calculatedResource;   //antal personer "procent" pga olika - resursberäkning
		private double _forecastedIncomingDemand; //förrut i "layern" - erlang
		private double _bookedAgainstIncomingDemand65;
		private Percent? _multiskillMinOccupancy;    // multi - resursberäkning
		private ServiceAgreement _serviceAgreementData; //80/20 alt 100 inom 4 timmar
		private ITask _taskData;  //uppgifter om samtal alt email, arbetsuppgifter
		private Percent _shrinkage;
		private Percent _efficiency;
		private Percent _efficiencyCalculated;
		private SkillPersonData _skillPersonData;
		private bool _isCalculated;
		private bool _useShrinkage;
		private double? _manualAgents;
		private double _calculatedUsedSeats;
		private int _maxSeats;

	    /// <summary>
		/// Initializes a new instance of the <see cref="SkillStaff"/> struct.
		/// </summary>
		/// <param name="task">The task.</param>
		/// <param name="serviceAgreement">The service agreement.</param>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-04-09
		/// </remarks>
		public SkillStaff(ITask task, ServiceAgreement serviceAgreement)
		{
			_taskData = task;
			_serviceAgreementData = serviceAgreement;
		}

	    public void Reset()
		{
			_isCalculated = false;
			_taskData = new Task();
			_serviceAgreementData = new ServiceAgreement();
			_skillPersonData = new SkillPersonData();
			_calculatedLoggedOn = 0;
			_calculatedOccupancy = 0;
			_calculatedResource = 0;
			_forecastedIncomingDemand = 0;
			_multiskillMinOccupancy = null;
			_shrinkage = new Percent();
			_efficiency = new Percent();
			_efficiencyCalculated = new Percent();
			_manualAgents = null;
            _useShrinkage = false;
		    _calculatedUsedSeats = 0;
		    _maxSeats = 0;
		}

	    /// <summary>
		/// Gets or sets a value indicating whether this instance is calculated.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is calculated; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-09-08
		/// </remarks>
		public bool IsCalculated
		{
			get { return _isCalculated; }
			protected internal set { _isCalculated = value; }
		}

		/// <summary>
		/// Gets or sets the calculated logged on.
		/// </summary>
		/// <value>The calculated logged on.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-04-09
		/// </remarks>
		public double CalculatedLoggedOn
		{
			get { return _calculatedLoggedOn; }
			set { _calculatedLoggedOn = value; }
		}

		/// <summary>
		/// Gets the calculated occupancy.
		/// </summary>
		/// <value>The calculated occupancy.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-04-09
		/// </remarks>
		public double CalculatedOccupancy
		{
			get { return _calculatedOccupancy; }
			internal set { _calculatedOccupancy = value; }
		}

		public Percent CalculatedOccupancyPercent => new Percent(_calculatedOccupancy);

		/// <summary>
		/// Gets or sets the calculated resource.
		/// </summary>
		/// <value>The calculated resource.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-04-09
		/// </remarks>
		public double CalculatedResource
		{
			get { return _calculatedResource; }
			internal set { _calculatedResource = value; }
		}

		public double ForecastedIncomingDemandWithoutShrinkage => _forecastedIncomingDemand;

		/// <summary>
		/// Gets the booked against incoming demand65.
		/// </summary>
		/// <value>The booked against incoming demand65.</value>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2009-02-05
		/// </remarks>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2009-02-05
		/// </remarks>
		public double BookedAgainstIncomingDemand65
		{
			get { return _bookedAgainstIncomingDemand65; }
			internal set { _bookedAgainstIncomingDemand65 = value; }
		}

		/// <summary>
		/// Gets the calculated traffic intensity.
		/// </summary>
		/// <value>The calculated traffic intensity.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-04-09
		/// </remarks>
		public double ForecastedIncomingDemand
		{
			get
			{
                if (_useShrinkage)
                    return CalculatedTrafficIntensityWithShrinkage;
				return _forecastedIncomingDemand;
			}
			internal set
			{
				_forecastedIncomingDemand = value * (1d + _efficiencyCalculated.Value);
			}
		}

		/// <summary>
		/// Gets the multi skill min occupancy.
		/// </summary>
		/// <value>The multi skill min occupancy.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-04-09
		/// </remarks>
		public Percent? MultiskillMinOccupancy
		{
			get { return _multiskillMinOccupancy; }
			set
			{
				if (_multiskillMinOccupancy != value)
				{
					_isCalculated = false;
					_multiskillMinOccupancy = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the service agreement data.
		/// </summary>
		/// <value>The service agreement data.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-09-08
		/// </remarks>
		public ServiceAgreement ServiceAgreementData
		{
			get { return _serviceAgreementData; }
			set
			{
				if (_serviceAgreementData != value)
				{
					_isCalculated = false;
					_serviceAgreementData = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the task data.
		/// </summary>
		/// <value>The task data.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-09-08
		/// </remarks>
		public ITask TaskData
		{
			get { return _taskData; }
			set
			{
				if (_taskData != value)
				{
					_isCalculated = false;
					_taskData = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the shrinkage.
		/// </summary>
		/// <value>The shrinkage.</value>
		/// <remarks>
		/// Created by: zoet
		/// Created date: 2008-05-12
		/// </remarks>
		public Percent Shrinkage
		{
			get { return _shrinkage; }
			set
			{
				if (_shrinkage != value)
				{
					_isCalculated = false;
					_shrinkage = value;
				}
			}
		}


		/// <summary>
		/// Gets or sets the efficiency.
		/// </summary>
		/// <value>The efficiency.</value>
		/// <remarks>
		/// Created by: marias
		/// Created date: 2010-07-01
		/// </remarks>
		public Percent Efficiency
		{
			get { return _efficiency; }
			set
			{
				if (_efficiency != value)
				{
					_isCalculated = false;
					var invertedEfficiency = (1 / value.Value);
					invertedEfficiency -= 1;
					_efficiencyCalculated = new Percent(invertedEfficiency);
					_efficiency = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the skill person data.
		/// </summary>
		/// <value>The skill person data.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-05-13
		/// </remarks>
		public SkillPersonData SkillPersonData
		{
			get { return _skillPersonData; }
			set
			{
				if (_skillPersonData != value)
				{
					_isCalculated = false;
					_skillPersonData = value;
				}
			}
		}

		/// <summary>
		/// Gets the calculated traffic intensity with shrinkage.
		/// </summary>
		/// <value>The calculated traffic intensity with shrinkage.</value>
		/// <remarks>
		/// Created by: zoet
		/// Created date: 2008-05-12
		/// </remarks>
		public double CalculatedTrafficIntensityWithShrinkage
		{
            get
            {
                if (_shrinkage.Value >= 1.0)
                    return 0;
                return _forecastedIncomingDemand / (1 - _shrinkage.Value);
            }
		}

		public bool UseShrinkage
		{
			get { return _useShrinkage; }
			set { _useShrinkage = value; }
		}

		public double? ManualAgents
		{
			get { return _manualAgents; }
			set
			{
				if (_manualAgents != value)
				{
					_isCalculated = false;
					_manualAgents = value;
				}
			}
		}

		public double CalculatedUsedSeats
		{
			get { return _calculatedUsedSeats; }
			set { _calculatedUsedSeats = value; }
		}

		public int MaxSeats
		{
			get { return _maxSeats; }
			set { _maxSeats = value; }
		}

	    public int? NoneBlendDemand { get; set; }
		public void Restore(ISkillStaff previousState)
		{
			_calculatedResource = previousState.CalculatedResource;
			_calculatedLoggedOn = previousState.CalculatedLoggedOn;
		}

		public IMemento CreateMemento()
		{
			var copy = new SkillStaff(_taskData, _serviceAgreementData)
			{
				_calculatedResource = _calculatedResource,
				_calculatedLoggedOn = _calculatedLoggedOn
			};
			return new Memento<ISkillStaff>(this, copy);
		}
	}
}
