using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Common
{
    public class GroupingAbsence : AggregateRoot, IGroupingAbsence, IDeleteTag
    {
        #region Fields

        private Description _description;
        private IList<IAbsence> _absenceCollection;
        private bool _isDeleted;

        #endregion

        #region Properties

        /// <summary>
        /// Set/Get for Description
        /// </summary>     
        public virtual Description Description
        {
            get { return _description; }
            set { _description=value; }
        }

        /// <summary>
        /// Name Property
        /// </summary>
        public virtual string Name
        {
            get
            {
                return _description.Name;
            }
            set
            {
                _description = new Description(value, _description.ShortName);
            }
        }

        /// <summary>
        /// ShortName Property
        /// </summary>
        public virtual string ShortName
        {
            get
            {
                return _description.ShortName;
            }
            set
            {
                _description = new Description(_description.Name, value);
            }
        }

        #endregion

        /// <summary>
        /// CreateProjection new GroupingAbsence
        /// </summary>
        public GroupingAbsence(string nameToSet) : this()
        {
            _description = new Description(nameToSet);
        }

        /// <summary>
        /// Empty contructor for NHibernate
        /// </summary>
        protected GroupingAbsence()
        {
            _absenceCollection = new List<IAbsence>();
        }

        /// <summary>
        /// Gets the absence types.
        /// Read only wrapper around the actual absence list.
        /// </summary>
        /// <value>The absence types list.</value>
        public virtual ReadOnlyCollection<IAbsence> AbsenceCollection
        {
            get { return new ReadOnlyCollection<IAbsence>(_absenceCollection); }
        }

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

        /// <summary>
        /// Adds an Absence.
        /// </summary>
        /// <param name="absence">The absence.</param>
        public virtual void AddAbsence(IAbsence absence)
        {
            InParameter.NotNull("absence", absence);
            if (_absenceCollection.Contains(absence))
            {
                _absenceCollection.Remove(absence);
            }
            _absenceCollection.Add(absence);
            //Add property at absence level to reference grouping absence
            absence.GroupingAbsence = this;
        }

        /// <summary>
        /// Remove an Absence.
        /// </summary>
        /// <param name="absence">The absence.</param>
        public virtual void RemoveAbsence(IAbsence absence)
        {
            InParameter.NotNull("absence", absence);
            if (_absenceCollection.Contains(absence))
            {
                _absenceCollection.Remove(absence);
            }
        }

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}