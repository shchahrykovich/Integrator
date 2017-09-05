using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Runner
{
    public class ProcessWaitHandle : WaitHandle
    {
        public ProcessWaitHandle(Process p)
        {
            SafeWaitHandle = new SafeWaitHandle(p.Handle, true);
        }
    }
}
