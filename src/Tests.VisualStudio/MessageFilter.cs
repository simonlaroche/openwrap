using System;
using System.Runtime.InteropServices;

namespace Tests.VisualStudio
{
    public class MessageFilter : IOleMessageFilter
    {
        //
        // Class containing the IOleMessageFilter
        // thread error-handling functions.

        // Start the filter.
        public static void Register()
        {
            IOleMessageFilter newFilter = new MessageFilter();
            IOleMessageFilter oldFilter = null;
            CoRegisterMessageFilter(newFilter, out oldFilter);
        }

        // Done with the filter, close it.
        public static void Revoke()
        {
            IOleMessageFilter oldFilter = null;
            CoRegisterMessageFilter(null, out oldFilter);
        }

        //
        // IOleMessageFilter functions.
        // Handle incoming thread requests.
        int IOleMessageFilter.HandleInComingCall(int dwCallType,
                                                 IntPtr hTaskCaller,
                                                 int dwTickCount,
                                                 IntPtr
                                                     lpInterfaceInfo)
        {
            //Return the flag SERVERCALL_ISHANDLED.
            return 0;
        }

        // Thread call was rejected, so try again.

        int IOleMessageFilter.MessagePending(IntPtr hTaskCallee,
                                             int dwTickCount,
                                             int dwPendingType)
        {
            //Return the flag PENDINGMSG_WAITDEFPROCESS.
            return 2;
        }

        int IOleMessageFilter.RetryRejectedCall(IntPtr
                                                    hTaskCallee,
                                                int dwTickCount,
                                                int dwRejectType)
        {
            if (dwRejectType == 2)
                // flag = SERVERCALL_RETRYLATER.
            {
                // Retry the thread call immediately if return >=0 & 
                // <100.
                return 99;
            }
            // Too busy; cancel call.
            return -1;
        }

        // Implement the IOleMessageFilter interface.
        [DllImport("Ole32.dll")]
        static extern int
            CoRegisterMessageFilter(IOleMessageFilter newFilter,
                                    out
                                        IOleMessageFilter oldFilter);
    }
}