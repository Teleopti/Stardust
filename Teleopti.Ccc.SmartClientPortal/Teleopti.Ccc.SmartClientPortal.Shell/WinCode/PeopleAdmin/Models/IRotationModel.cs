using Syncfusion.Windows.Forms.Grid;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
{
    public interface IRotationModel<TRotationType, TRotationBaseType>
    {
        string PersonFullName { get; set; }

        IPerson Person { get; set;}

        TRotationType PersonRotation
        {
            get;
            set;
        }

        GridControl GridControl
        {
            get;
            set;
        }

        bool ExpandState
        {
            get;
            set;
        }

        TRotationBaseType CurrentRotation
        {
            get;
            set;
        }

        DateOnly? FromDate { get; set; }

        bool CanBold { get; set; }

        /// <summary>
        /// The rotation start week for the given person
        /// </summary>
        /// <remarks>
        /// Created by: Shiran Ginige
        /// Created date: 2008-07-23
        /// </remarks>
        int StartWeek { get; set; }
    }
}
