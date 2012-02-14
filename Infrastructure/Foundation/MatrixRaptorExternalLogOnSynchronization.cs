using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    /// <summary>
    /// Synchronizes Matrix ACD Logins and Raptor External Log ons
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-05-07
    /// </remarks>
    public class MatrixRaptorExternalLogOnSynchronization
    {
        private readonly IExternalLogOnRepository _externalLogOnRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixRaptorExternalLogOnSynchronization"/> class.
        /// </summary>
        /// <param name="externalLogOnRepository">The external log on repository.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        public MatrixRaptorExternalLogOnSynchronization(IExternalLogOnRepository externalLogOnRepository)
        {
            _externalLogOnRepository = externalLogOnRepository;
        }

        /// <summary>
        /// Synchronizes the external log ons.
        /// </summary>
        /// <param name="matrixExternalLogOns">The matrix external log ons.</param>
        /// <returns>The number of logins affected by synchronization.</returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-18
        /// </remarks>
        public int SynchronizeExternalLogOns(IList<IExternalLogOn> matrixExternalLogOns)
        {
            IList<IExternalLogOn> raptorExternalLogOns = _externalLogOnRepository.LoadAllExternalLogOns();
            IList<IExternalLogOn> externalLogOnsToAdd = new List<IExternalLogOn>(matrixExternalLogOns);
            int updatedCount = 0;

        	ClearInvalidMartDataOnRaptorLogOns(raptorExternalLogOns, matrixExternalLogOns);

            foreach (IExternalLogOn matrixLogin in matrixExternalLogOns)
            {
                foreach (IExternalLogOn raptorLogin in raptorExternalLogOns)
                {
                    if ((raptorLogin.AcdLogOnMartId == -1 && 
                        raptorLogin.DataSourceId == -1 &&
                        raptorLogin.AcdLogOnAggId > -1 &&
                        raptorLogin.AcdLogOnOriginalId == raptorLogin.AcdLogOnAggId.ToString(CultureInfo.InvariantCulture) &&
                        raptorLogin.AcdLogOnName == raptorLogin.AcdLogOnAggId.ToString(CultureInfo.InvariantCulture) &&
                        raptorLogin.AcdLogOnAggId == matrixLogin.AcdLogOnAggId) 
                        ||
                        (raptorLogin.AcdLogOnMartId == matrixLogin.AcdLogOnMartId))
                    {
                        // Newly upgraded/converted database with unmapped agent logins with matching AcdLogOnAggId´s
                        // Or
                        // agent logins with matching AcdLogOnMartId´s. 
                        raptorLogin.Active = matrixLogin.Active;
                        raptorLogin.AcdLogOnMartId = matrixLogin.AcdLogOnMartId;
                        raptorLogin.AcdLogOnAggId = matrixLogin.AcdLogOnAggId;
                        raptorLogin.AcdLogOnOriginalId = matrixLogin.AcdLogOnOriginalId;
                        raptorLogin.AcdLogOnName = matrixLogin.AcdLogOnName;
                        raptorLogin.DataSourceId = matrixLogin.DataSourceId;
                        externalLogOnsToAdd.Remove(matrixLogin);
                        updatedCount += 1;
                    }
                }
            }
            _externalLogOnRepository.AddRange(externalLogOnsToAdd);

            return externalLogOnsToAdd.Count + updatedCount;
        }

		private static void ClearInvalidMartDataOnRaptorLogOns(IList<IExternalLogOn> raptorLogOns, IList<IExternalLogOn> matrixLogOns)
		{
			foreach (var raptorLogOn in raptorLogOns)
			{
				var hasInvalidData = matrixLogOns.All(matrixQueue => raptorLogOn.AcdLogOnMartId != matrixQueue.AcdLogOnMartId);

				if (hasInvalidData)
				{
					raptorLogOn.AcdLogOnMartId = -1;
					raptorLogOn.DataSourceId = -1;
					raptorLogOn.AcdLogOnOriginalId = raptorLogOn.AcdLogOnAggId.ToString(CultureInfo.InvariantCulture);
					raptorLogOn.AcdLogOnName = raptorLogOn.AcdLogOnOriginalId;
				}
			}
		}
    }
}
