using System.Drawing;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Kpi
{
    /// <summary>
    /// A KPI Key Performance Indicator
    /// </summary>
    /// <remarks>
    /// Created by: Ola
    /// Created date: 2008-04-07
    /// </remarks>
    public class KeyPerformanceIndicator : AggregateRoot_Events_ChangeInfo_Versioned, IKeyPerformanceIndicator
    {
        private readonly IList<IKpiTarget> _kpiTargetCollection = new List<IKpiTarget>();

        private readonly EnumTargetValueType _targetValueType = EnumTargetValueType.TargetValueTypeNumber;
        private readonly string _name = " ";
        private readonly double _defaultTargetValue = 1;
        private readonly double _defaultMinValue = 1;
        private readonly double _defaultMaxValue = 1;
        private readonly Color _defaultLowerThanMinColor;
        private readonly Color _defaultHigherThanMaxColor;
        private readonly Color _defaultBetweenColor;
        private readonly string _resourceKey = " ";

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyPerformanceIndicator"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="resourceKey">The resource key.</param>
        /// <param name="targetValueType">Type of the target value.</param>
        /// <param name="defaultTargetValue">The default target value.</param>
        /// <param name="defaultMinValue">The default min value.</param>
        /// <param name="defaultMaxValue">The default max value.</param>
        /// <param name="defaultLowerThanMinColor">Default color of the lower than min.</param>
        /// <param name="defaultHigherThanMaxColor">Default color of the higher than max.</param>
        /// <param name="defaultBetweenColor">Default color of the between.</param>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-23    
        /// </remarks> 
        public KeyPerformanceIndicator(string name, string resourceKey, EnumTargetValueType targetValueType,
            double defaultTargetValue, double defaultMinValue, double defaultMaxValue,
             Color defaultBetweenColor, Color defaultLowerThanMinColor, Color defaultHigherThanMaxColor)
        {
            _name = name;
            _resourceKey = resourceKey;
            _targetValueType = targetValueType;

            _defaultTargetValue = defaultTargetValue;
            _defaultMinValue = defaultMinValue;
            _defaultMaxValue = defaultMaxValue;

            _defaultLowerThanMinColor = defaultLowerThanMinColor;
            _defaultHigherThanMaxColor = defaultHigherThanMaxColor;
            _defaultBetweenColor = defaultBetweenColor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyPerformanceIndicator"/> class.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-23    
        /// </remarks>
        public KeyPerformanceIndicator()
        {
            _defaultLowerThanMinColor = Color.White;
            _defaultHigherThanMaxColor = Color.White;
            _defaultBetweenColor = Color.White;
        }

        /// <summary>
        /// Gets the resource key.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-17    
        /// </remarks>
        public virtual string ResourceKey
        {
            get { return _resourceKey; }
        } 

        /// <summary>
        /// Gets the default color of the target.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-09    
        /// </remarks>
        public virtual Color DefaultBetweenColor
        {
            get { return _defaultBetweenColor; }
        }

        /// <summary>
        /// Gets the default color of the max.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-09    
        /// </remarks>
        public virtual Color DefaultHigherThanMaxColor
        {
            get { return _defaultHigherThanMaxColor; }
        }

        /// <summary>
        /// Gets the default color of the min.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-09    
        /// </remarks>
        public virtual Color DefaultLowerThanMinColor
        {
            get { return _defaultLowerThanMinColor; }
        }

        /// <summary>
        /// Gets the default max value.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-09    
        /// </remarks>
        public virtual double DefaultMaxValue
        {
            get { return _defaultMaxValue; }
        }

        /// <summary>
        /// Gets the default min value.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-09    
        /// </remarks>
        public virtual double DefaultMinValue
        {
            get { return _defaultMinValue; }
        }

        /// <summary>
        /// Gets or sets the default target value.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-09    
        /// </remarks>
        public virtual double DefaultTargetValue
        {
            get { return _defaultTargetValue; }
        }

        /// <summary>
        /// Gets the type of the target value.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-09    
        /// </remarks>
        public virtual EnumTargetValueType TargetValueType
        {
            get { return _targetValueType; }
        }

        /// <summary>
        /// Gets the kpi target collection.
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        public virtual IList<IKpiTarget> KpiTargetCollection
        {
            get { return new ReadOnlyCollection<IKpiTarget>(_kpiTargetCollection); }
        }

        /// <summary>
        /// Gets the Name
        /// </summary>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        public virtual string Name
        {
            get {
                string tmp = UserTexts.Resources.ResourceManager.GetString(_resourceKey);
                if (!string.IsNullOrEmpty(tmp)) return tmp;
                return _name;
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-16    
        /// </remarks>
        public override string ToString()
        {
            return Name;
        }
    }
}
