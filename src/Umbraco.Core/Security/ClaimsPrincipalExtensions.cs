using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Umbraco.Core;
using Umbraco.Core.Security;

namespace Umbraco.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// This will return the current back office identity if the IPrincipal is the correct type
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static UmbracoBackOfficeIdentity GetUmbracoIdentity(this IPrincipal user)
        {
            // TODO: It would be nice to get rid of this and only rely on Claims, not a strongly typed identity instance

            // If it's already a UmbracoBackOfficeIdentity
            if (user.Identity is UmbracoBackOfficeIdentity backOfficeIdentity) return backOfficeIdentity;

            // Check if there's more than one identity assigned and see if it's a UmbracoBackOfficeIdentity and use that
            if (user is ClaimsPrincipal claimsPrincipal)
            {
                backOfficeIdentity = claimsPrincipal.Identities.OfType<UmbracoBackOfficeIdentity>().FirstOrDefault();
                if (backOfficeIdentity != null) return backOfficeIdentity;
            }

            // Otherwise convert to a UmbracoBackOfficeIdentity if it's auth'd
            if (user.Identity is ClaimsIdentity claimsIdentity
                && claimsIdentity.IsAuthenticated
                && UmbracoBackOfficeIdentity.FromClaimsIdentity(claimsIdentity, out var umbracoIdentity))
            {
                return umbracoIdentity;
            }

            return null;
        }

        /// <summary>
        /// Returns the remaining seconds on an auth ticket for the user based on the claim applied to the user durnig authentication
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static double GetRemainingAuthSeconds(this IPrincipal user) => user.GetRemainingAuthSeconds(DateTimeOffset.UtcNow);

        /// <summary>
        /// Returns the remaining seconds on an auth ticket for the user based on the claim applied to the user durnig authentication
        /// </summary>
        /// <param name="user"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public static double GetRemainingAuthSeconds(this IPrincipal user, DateTimeOffset now)
        {
            var claimsPrincipal = user as ClaimsPrincipal;
            if (claimsPrincipal == null)
            {
                return 0;
            }

            var ticketExpires = claimsPrincipal.FindFirst(Constants.Security.TicketExpiresClaimType)?.Value;
            if (ticketExpires.IsNullOrWhiteSpace())
            {
                return 0;
            }

            var utcExpired = DateTimeOffset.Parse(ticketExpires, null, DateTimeStyles.RoundtripKind);

            var secondsRemaining = utcExpired.Subtract(now).TotalSeconds;
            return secondsRemaining;
        }
    }
}
