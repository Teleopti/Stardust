using System.Drawing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Kpi
{
    /// <summary>
    /// Holds values for one Kpi and One Team
    /// </summary>
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-04-07    
    /// </remarks>
    public class KpiTarget : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, IKpiTarget
    {
       private IKeyPerformanceIndicator _keyPerformanceIndicator;
        private ITeam _team;
        private  double _targetValue;
        private double _minValue;
        private double _maxValue;
        private Color _lowerThanMinColor;
        private Color _higherThanMaxColor;
        private Color _betweenColor;
		private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();

		/// <summary>
		/// Gets the team description.
		/// For readonly use in databindings.
		/// </summary>
		/// <value>The team description.</value>
		/// <remarks>
		///  Created by: Ola
		///  Created date: 2008-04-10    
		/// </remarks>
		public virtual string TeamDescription
        {
            get { return Team.Description.ToString(); }
        }

        /// <summary>
        /// Gets or sets the color of the between.
        /// </summary>
        /// <value>The color of the between.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-09    
        /// </remarks>
        public virtual Color BetweenColor
        {
            get { return _betweenColor; }
            set { _betweenColor = value; }
        }

        /// <summary>
        /// Gets or sets the KeyPerformanceIndicator.
        /// </summary>
        /// <value>The KeyPerformanceIndicator.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        public virtual IKeyPerformanceIndicator KeyPerformanceIndicator
        {
            get { return _keyPerformanceIndicator; }
            set { _keyPerformanceIndicator = value; }
        }

        /// <summary>
        /// Gets or sets the team.
        /// </summary>
        /// <value>The team.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        public virtual ITeam Team
        {
            get { return _team; }
            set { _team = value; }
        }

        /// <summary>
        /// Gets or sets the target value.
        /// </summary>
        /// <value>The target value.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        public virtual double TargetValue
        {
            get { return _targetValue; }
            set { 
                _targetValue = value;
                if (_minValue > _targetValue) _minValue = _targetValue;
                if (_maxValue < _targetValue) _maxValue = _targetValue;
                }
        }

        /// <summary>
        /// Gets or sets the min value.
        /// </summary>
        /// <value>The min value.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        public virtual double MinValue
        {
            get { return _minValue; }
            set { 
                _minValue = value;
                if (_minValue > _targetValue) _targetValue =_minValue;
                if (_maxValue < _targetValue) _maxValue = _targetValue;
                }
        }

        /// <summary>
        /// Gets or sets the max value.
        /// </summary>
        /// <value>The max value.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        public virtual double MaxValue
        {
            get { return _maxValue; }
            set { 
                _maxValue = value;
                if (_maxValue < _targetValue) _targetValue = _maxValue;
                if (_minValue > _targetValue) _minValue = _targetValue;
                }
        }

        /// <summary>
        /// Gets or sets the color of the min.
        /// </summary>
        /// <value>The color of the min.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        public virtual Color LowerThanMinColor
        {
            get { return _lowerThanMinColor; }
            set { _lowerThanMinColor = value; }
        }

        /// <summary>
        /// Gets or sets the color of the max.
        /// </summary>
        /// <value>The color of the max.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        public virtual Color HigherThanMaxColor
        {
            get { return _higherThanMaxColor; }
            set { _higherThanMaxColor = value; }
        }

		public virtual string UpdatedTimeInUserPerspective => _localizer.UpdatedTimeInUserPerspective(this);
	}
}
