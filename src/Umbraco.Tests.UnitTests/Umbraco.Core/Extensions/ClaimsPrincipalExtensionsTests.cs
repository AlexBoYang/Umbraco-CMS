using System;
using System.Linq;
using System.Security.Claims;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Extensions
{
    [TestFixture]
    public class ClaimsPrincipalExtensionsTests
    {
        [Test]
        public void Get_Remaining_Ticket_Seconds()
        {
            ClaimsIdentity backOfficeIdentity = new ClaimsIdentity().AddRequiredBackofficeClaims(
                Constants.Security.SuperUserIdAsString,
                "test",
                "test",
                Enumerable.Empty<int>(),
                Enumerable.Empty<int>(),
                "en-US",
                Guid.NewGuid().ToString(),
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>());
            var principal = new ClaimsPrincipal(backOfficeIdentity);

            var expireSeconds = 99;
            var elapsedSeconds = 3;
            var remainingSeconds = expireSeconds - elapsedSeconds;
            DateTimeOffset now = DateTimeOffset.Now;
            DateTimeOffset then = now.AddSeconds(elapsedSeconds);
            var expires = now.AddSeconds(expireSeconds).ToString("o");

            backOfficeIdentity.AddClaim(new Claim(
                        Constants.Security.TicketExpiresClaimType,
                        expires,
                        ClaimValueTypes.DateTime,
                        UmbracoBackOfficeIdentity.Issuer,
                        UmbracoBackOfficeIdentity.Issuer,
                        backOfficeIdentity));

            var ticketRemainingSeconds = principal.GetRemainingAuthSeconds(then);

            Assert.AreEqual(remainingSeconds, ticketRemainingSeconds);
        }
    }
}
