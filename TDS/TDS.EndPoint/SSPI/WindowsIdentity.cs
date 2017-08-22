// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Security.Principal;

namespace Microsoft.SqlServer.TDS.EndPoint.SSPI
{
    internal class WindowsIdentity : IIdentity
    {
        private IntPtr token;

        public WindowsIdentity(IntPtr token)
        {
            this.token = token;
        }

        public string AuthenticationType => throw new NotImplementedException();

        public bool IsAuthenticated => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();
    }
}