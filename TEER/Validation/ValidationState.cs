using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEER.Validation
{
    public class ValidationState
    {
        private readonly List<ValidationError> errors;

        public List<ValidationError> Errors
        {
            get
            {
                return errors;
            }
        }

        public bool IsValid
        {
            get
            {
                return Errors.Count == 0;
            }
        }


        public ValidationState()
        {
            errors = new ValidationErrorCollection();
        }

        public void AddError(string name, string message, object attemptedValue = null)
        {
            var error = new ValidationError(name, attemptedValue, message);
            Errors.Add(error);
        }
    }
}