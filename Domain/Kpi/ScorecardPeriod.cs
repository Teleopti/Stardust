using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Kpi
{

    /// <summary>
    /// Class holding what Period the scorecard shall show
    /// </summary>
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-04-17    
    /// </remarks>
    public class ScorecardPeriod : IScorecardPeriod
    {
        private readonly int _id;
        private readonly string _name;
        private const string PERIODDAY = "Day";
        private const string PERIODWEEK = "Week";
        private const string PERIODMONTH = "Month";
        private const string PERIODQUARTER = "Quarter";
        private const string PERIODYEAR = "Year";


        /// <summary>
        /// Initializes a new instance of the <see cref="ScorecardPeriod"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-17
        /// </remarks>
        public ScorecardPeriod(int id)
        {
            if (id < 0 || id > 4)
            {
                throw new ArgumentOutOfRangeException("id", "The parameter id must be between 0 and 4");
            }
            _id = id;

            switch (_id)
            {
                case 0:
                    _name = UserTexts.Resources.ResourceManager.GetString(PERIODDAY);
                    break;
                case 1:
                    _name = UserTexts.Resources.ResourceManager.GetString(PERIODWEEK);
                    break;
                case 2:
                    _name = UserTexts.Resources.ResourceManager.GetString(PERIODMONTH);
                    break;
                case 3:
                    _name = UserTexts.Resources.ResourceManager.GetString(PERIODQUARTER);
                    break;
                case 4:
                    _name = UserTexts.Resources.ResourceManager.GetString(PERIODYEAR);
                    break;
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-18    
        /// </remarks>
        public string Name
        {
            get {
                return _name; 
            }
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-18    
        /// </remarks>
        public int Id
        {
            get { return _id; }
        }
    }
}
