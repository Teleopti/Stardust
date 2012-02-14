using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    /// <summary>
    /// Holds an agent and a Datetime
    /// Zo?
    /// </summary>
    public struct AgentDate : IEquatable<AgentDate>
    {
        private readonly Person _agent;
        private readonly DateTime _date;

        /// <summary>
        /// Creates an instance
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="date"></param>
        public AgentDate(Person agent, DateTime date)
        {
            InParameter.NotNull("agent",agent);
            InParameter.NotNull("date",date);
            _agent = agent;
            _date = date.Date;
        }

        /// <summary>
        /// Gets the Person
        /// </summary>
        public Person Agent
        {
            get { return _agent; }
        }

        /// <summary>
        /// Gets the Date
        /// </summary>
        public DateTime Date
        {
            get { return _date; }
        }

        /// <summary>
        /// Overrides ToString method and displayes Person.Name + Date
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _agent.Name + " " + _date;
        }

        /// <summary>
        /// Compares two instances od AgentDate
        /// </summary>
        /// <param name="agentDate1"></param>
        /// <param name="agentDate2"></param>
        /// <returns></returns>
        public static bool operator ==(AgentDate agentDate1, AgentDate agentDate2)
        {
            if (agentDate1._date == agentDate2._date && agentDate1._agent.Equals(agentDate2._agent))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Compares to instances od AgentDate
        /// </summary>
        /// <param name="agentDate1"></param>
        /// <param name="agentDate2"></param>
        /// <returns></returns>
        public static bool operator !=(AgentDate agentDate1, AgentDate agentDate2)
        {
            if (agentDate1._date != agentDate2._date || agentDate1._agent != agentDate2._agent)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            return _date.GetHashCode() ^ _agent.GetHashCode();
        }

        ///<summary>
        ///Indicates whether the current object is equal to another object of the same type.
        ///</summary>
        ///
        ///<returns>
        ///true if the current object is equal to the other parameter; otherwise, false.
        ///</returns>
        ///
        ///<param name="other">An object to compare with this object.</param>
        public bool Equals(AgentDate other)
        {
            return (_date == other._date && _agent.Equals(other._agent));
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-01-04
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (obj is AgentDate)
            {
                return Equals((AgentDate)obj);
            } 
            return false;
        }

    }
}