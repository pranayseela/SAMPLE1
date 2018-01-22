using System;
using System.Collections.Generic;
using System.Linq;

namespace TEER.Model
{
    public class LoginUserData
    {
        /// <summary>
        /// Gets the login time in UTC. In production this will be the same as SystemTimeAtLoginUtc
        /// </summary>
        public DateTime LoginTimeUtc { get; set; }

        /// <summary>
        /// Gets the system time in UTC at the time of logon.
        /// </summary>
        public DateTime SystemTimeUtcAtLogin { get; set; }

        /// <summary>
        /// Gets the current system time in UTC
        /// </summary>
        public DateTime SystemTimeUtc
        {
            get
            {
                // calculate the time elapsed since login
                DateTime systemTimeUtc = DateTime.Now.ToUniversalTime();
                TimeSpan elapsedTimeSinceLogin = systemTimeUtc - SystemTimeUtcAtLogin;

                // add the seconds elapsed to the login time
                return LoginTimeUtc.AddSeconds(elapsedTimeSinceLogin.TotalSeconds);
            }
        }

        /// <summary>
        /// Parameterless constructor needed for deserialization
        /// </summary>
        public LoginUserData()
        {

        }
    }
}