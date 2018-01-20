using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Exceptions
{
    public class InvalidSetRepException : Exception
    {
        public InvalidSetRepException()
        {
        }
        public InvalidSetRepException(string message)
            : base(message)
        {

        }
    }
}