using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client.src
{
    /// <summary>
    /// Provides data for explorer navigation exception handling.
    /// </summary>
    class ExplorerErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Gets thrown exception.
        /// </summary>
        public Exception Exception
        {
            get;
            private set;
        }

        public ExplorerErrorEventArgs(Exception ex)
        {
            Exception = ex;
        }
    }
}