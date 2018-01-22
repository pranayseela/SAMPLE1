using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEER.Validation
{
    public class ValidationErrorCollection : List<ValidationError>
    {
        public void Add(string name, object attemptedValue, string message)
        {
            Add(new ValidationError(name, attemptedValue, message));
        }

        public void Add(string name, object attemptedValue, Exception exception)
        {
            Add(new ValidationError(name, attemptedValue, exception));
        }
    }
}