﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.Services
{
    public class ShiftTradeRequestStatusCheckerForTestDoesNothing : IBatchShiftTradeRequestStatusChecker
    {
        public void Check(IShiftTradeRequest shiftTradeRequest)
        {
        }

    	public bool IsInBatchMode
    	{
			get { return false; }
    	}

    	public void StartBatch(IEnumerable<IPersonRequest> personRequests)
    	{
    	}

    	public void EndBatch()
    	{
    	}
    }
}