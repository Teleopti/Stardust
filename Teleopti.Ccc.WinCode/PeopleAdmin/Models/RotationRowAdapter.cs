#region Imports

using System;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

#endregion

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Wrappers
{

    /// <summary>
    /// This class was added with the assumption that start week column of the person rotation needs a 
    /// drop-down box. But eventually it is not required it seems. Lets keeps this adapter just in case we
    /// want something like that in the future.
    /// </summary>
    public class RotationRowAdapter
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RotationRowAdapter"/> class.
        /// </summary>
        /// <remarks>
        /// Created by:Shiran Ginige
        /// Created date: 8/4/2008
        /// </remarks>
        public RotationRowAdapter(int rotationRowIndex, int rotationRowName) 
        {
            _rotationRowIndex = rotationRowIndex;
            //_row = row;
            _rotationRowName = rotationRowName;

        }

        #endregion

        #region Fields - Instance Member

        //The row index for the RotationRowAdapter
        private int _rotationRowIndex;

        //The rotation row
        //private RotationRow _row;
        
        //The rotation row name
        private int _rotationRowName;

        #endregion

        #region Properties - Instance Member



        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>The index.</value>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 8/4/2008
        /// </remarks>
        public int Index
        {
            get { return _rotationRowIndex; }
        }

        /// <summary>
        /// Gets or sets the row.
        /// </summary>
        /// <value>The row.</value>
        /// <remarks>
        /// Created by:Shiran Ginige
        /// Created date: 8/4/2008
        /// </remarks>
        //public RotationRow Row
        //{
        //    get { return _row; }
        //}

        /// <summary>
        /// Gets or sets the name of the rotation row.
        /// </summary>
        /// <value>The name of the rotation row.</value>
        /// <remarks>
        /// Created by:Shrian Ginige
        /// Created date: 8/4/2008
        /// </remarks>
        public  int RotationRowName
        {
            get { return _rotationRowName; }
        }

        #endregion

        

    }

}
