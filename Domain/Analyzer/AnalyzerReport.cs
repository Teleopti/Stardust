using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Domain.Analyzer
{
    /// <summary>
    /// Holds temporary information about reports the user can access from Analyzer
    /// </summary>
    /// /// 
    /// <remarks>
    ///  Created by: Ola
    ///  Created date: 2008-05-15    
    /// /// </remarks>
    public class AnalyzerReport
    {
        private int _depth;
        private string _folderName;
        private string _reportName;
        private int? _reportId;

        /// <summary>
        /// Gets or sets the report id.
        /// </summary>
        /// <value>The report id.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-05-15    
        /// /// </remarks>
        public int? ReportId
        {
            get { return _reportId; }
            set { _reportId = value; }
        }

        /// <summary>
        /// Gets or sets the name of the report.
        /// </summary>
        /// <value>The name of the report.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-05-15    
        /// /// </remarks>
        public string ReportName
        {
            get { return _reportName; }
            set { _reportName = value; }
        }


        /// <summary>
        /// Gets or sets the name of the folder.
        /// </summary>
        /// <value>The name of the folder.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-05-15    
        /// /// </remarks>
        public string FolderName
        {
            get { return _folderName; }
            set { _folderName = value; }
        }


        /// <summary>
        /// Gets or sets the depth.
        /// </summary>
        /// <value>The depth.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-05-15    
        /// /// </remarks>
        public int Depth
        {
            get { return _depth; }
            set { _depth = value; }
        } 
    }
}
