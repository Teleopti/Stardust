using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Creating test data for Payload domain object
    /// </summary>
    public static class AbsenceFactory
    {
        /// <summary>
        /// Creates an absence aggregate.
        /// </summary>
        /// <returns></returns>
        public static Absence CreateAbsence(string name, string shortName,
                                            Color color)
        {
            InParameter.NotNull("name", name);
            InParameter.NotNull("shortName", shortName);
            InParameter.NotNull("color", color);
            Absence ret = new Absence();
            ret.Description = new Description(name,shortName);
            ret.DisplayColor = color;
            ret.Priority = 145;
            return ret;
        }

        /// <summary>
        /// Creates the requestable absence.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="shortName">The short name.</param>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-10-07
        /// </remarks>
        public static Absence CreateRequestableAbsence(string name, string shortName,
                                         Color color)
        {
            InParameter.NotNull("name", name);
            InParameter.NotNull("shortName", shortName);
            InParameter.NotNull("color", color);
            Absence ret = new Absence();
            ret.Description = new Description(name, shortName);
            ret.DisplayColor = color;
            ret.Priority = 145;
            ret.Requestable = true;
            return ret;
        }

        /// <summary>
        /// Creates an absence aggregate.
        /// </summary>
        /// <returns></returns>
        public static IAbsence CreateAbsence(string name)
        {
            InParameter.NotNull("name", name);
            IAbsence ret = new Absence();
            ret.Description = new Description(name,name.Substring(0, 2));
            ret.DisplayColor = Color.Red;
            return ret;
        }

        public static IList<IAbsence> CreateSomeAbsences()
        {
           return new List<IAbsence>()
                                      {
                                          CreateAbsence("Illness", "IL", Color.ForestGreen),
                                          CreateAbsence("Vacation", "VA", Color.Red),
                                          CreateAbsence("Parental Leave", "PL", Color.YellowGreen),
                                          CreateAbsence("Boating", "BO", Color.Blue)
                                          
                                      };
        }
    }
}