using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
    public class ShiftTradeRequestPersonExtractor
    {
        private readonly IList<IPerson> _persons = new List<IPerson>();

        public void ExtractPersons(IShiftTradeRequest shiftTradeRequest)
        {
            foreach (IShiftTradeSwapDetail shiftTradeSwapDetail in shiftTradeRequest.ShiftTradeSwapDetails)
            {
                if (!_persons.Contains(shiftTradeSwapDetail.PersonFrom)) _persons.Add(shiftTradeSwapDetail.PersonFrom);
                if (!_persons.Contains(shiftTradeSwapDetail.PersonTo)) _persons.Add(shiftTradeSwapDetail.PersonTo);
            }
        }

        public IList<IPerson> Persons
        {
            get { return _persons; }
        }
    }
}
