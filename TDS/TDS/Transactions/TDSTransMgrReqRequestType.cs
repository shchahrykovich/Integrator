using System;
using System.Collections.Generic;
using System.Text;

namespace TDS.Transactions
{
    public enum TDSTransMgrReqRequestType
    {
        TM_GET_DTC_ADDRESS = 0,
        TM_PROPAGATE_XACT = 1,
        TM_BEGIN_XACT = 5,
        TM_PROMOTE_XACT = 6,
        TM_COMMIT_XACT = 7,
        TM_ROLLBACK_XACT = 8,
        TM_SAVE_XACT = 9
    }
}
