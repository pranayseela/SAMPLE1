using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEER.Helpers
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class StoreValueInCookieAttribute : Attribute
    {
    }
}