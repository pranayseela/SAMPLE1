using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEER.Data
{
    public class DalException : Exception
    {
        public DalException()
        {

        }
        public DalException(string message)
            : base(message)
        {

        }
        public DalException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
        protected DalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {

        }
    }
}
