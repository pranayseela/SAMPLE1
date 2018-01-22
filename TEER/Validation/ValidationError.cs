﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEER.Validation
{
    public class ValidationError
    {

        public string Name { get; private set; }

        public object AttemptedValue { get; private set; }

        public string Message { get; private set; }

        public Exception Exception { get; private set; }

        public ValidationError(string name, object attemptedValue, string message)
        {
            //if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException("message");

            Name = name;
            AttemptedValue = attemptedValue;
            Message = message;
        }

        public ValidationError(string name, object attemptedValue, Exception exception)
        {
            //if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (exception == null) throw new ArgumentNullException("exception");

            Name = name;
            AttemptedValue = attemptedValue;
            Exception = exception;
            Message = exception.Message;
        }
    }
}