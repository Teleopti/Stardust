namespace Teleopti.Ccc.Win.SmartParts
{
    /// <summary>
    /// Represents a Property object that hold s the information about Smart
    /// Part which going to load
    /// </summary>
    public class SmartPartInformation
    {
        /// <summary>
        /// store the name of the assembly which contain the Smart Part
        /// </summary>
        public string ContainingAssembly { get; set; }

        /// <summary>
        /// Name of the smartPart to be load
        /// </summary>
        public string SmartPartName { get; set; }

        /// <summary>
        /// Get or set the Header Title of the Smart which
        /// displace when title bar of Smart part
        /// </summary>
        public string SmartPartHeaderTitle { get; set; }

        /// <summary>
        /// user defined Id to identify SmartPart within the Grid workspace
        /// </summary>
        public string SmartPartId { get; set; }

        /// <summary>
        /// Column no of the SmartPart 
        /// </summary>
        public int? GridColumn { get; set; }

        /// <summary>
        /// Row No of of the SmartPart
        /// </summary>
        public int? GridRow { get; set; }
    }
}
