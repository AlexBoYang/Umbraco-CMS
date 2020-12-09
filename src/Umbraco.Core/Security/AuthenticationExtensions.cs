﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Umbraco.Core.Security
{
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Ensures that the thread culture is set based on the back office user's culture
        /// </summary>
        /// <param name="identity"></param>
        public static void EnsureCulture(this IIdentity identity)
        {
            var culture = GetCulture(identity);
            if (!(culture is null))
            {
                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = culture;
            }
        }

        public static CultureInfo GetCulture(this IIdentity identity)
        {
            if (identity is ClaimsIdentity umbIdentity && umbIdentity.IsAuthenticated)
            {
                return UserCultures.GetOrAdd(umbIdentity.GetCulture(), s => new CultureInfo(s));
            }

            return null;
        }


        /// <summary>
        /// Used so that we aren't creating a new CultureInfo object for every single request
        /// </summary>
        private static readonly ConcurrentDictionary<string, CultureInfo> UserCultures = new ConcurrentDictionary<string, CultureInfo>();
    }
}
