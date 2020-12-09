using System;
using System.Linq;
using System.Security.Claims;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Security;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.BackOffice
{
    [TestFixture]
    public class UmbracoBackOfficeIdentityTests
    {

        public const string TestIssuer = "TestIssuer";

        [Test]
        public void Create_From_Claims_Identity()
        {
            var securityStamp = Guid.NewGuid().ToString();
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                //This is the id that 'identity' uses to check for the user id
                new Claim(ClaimTypes.NameIdentifier, "1234", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                //This is the id that 'identity' uses to check for the username
                new Claim(ClaimTypes.Name, "testing", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(ClaimTypes.GivenName, "hello world", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(Constants.Security.StartContentNodeIdClaimType, "-1", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                new Claim(Constants.Security.StartMediaNodeIdClaimType, "5543", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                new Claim(Constants.Security.StartMediaNodeIdClaimType, "5555", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                new Claim(Constants.Security.AllowedApplicationsClaimType, "content", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(Constants.Security.AllowedApplicationsClaimType, "media", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(ClaimTypes.Locality, "en-us", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, "admin", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(Constants.Security.SecurityStampClaimType, securityStamp, ClaimValueTypes.String, TestIssuer, TestIssuer),
            });

            if (!UmbracoBackOfficeIdentity.FromClaimsIdentity(claimsIdentity, out var backofficeIdentity))
                Assert.Fail();

            Assert.IsNull(backofficeIdentity.Actor);
            Assert.AreEqual(1234, backofficeIdentity.GetId());
            //Assert.AreEqual(sessionId, backofficeIdentity.SessionId);
            Assert.AreEqual(securityStamp, backofficeIdentity.GetSecurityStamp());
            Assert.AreEqual("testing", backofficeIdentity.GetUsername());
            Assert.AreEqual("hello world", backofficeIdentity.GetRealName());
            Assert.AreEqual(1, backofficeIdentity.GetStartContentNodes().Length);
            Assert.IsTrue(backofficeIdentity.GetStartMediaNodes().UnsortedSequenceEqual(new[] { 5543, 5555 }));
            Assert.IsTrue(new[] {"content", "media"}.SequenceEqual(backofficeIdentity.GetAllowedApplications()));
            Assert.AreEqual("en-us", backofficeIdentity.GetCulture());
            Assert.IsTrue(new[] { "admin" }.SequenceEqual(backofficeIdentity.GetRoles()));

            Assert.AreEqual(11, backofficeIdentity.Claims.Count());
        }

        [Test]
        public void Create_From_Claims_Identity_Missing_Required_Claim()
        {
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1234", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                new Claim(ClaimTypes.Name, "testing", ClaimValueTypes.String, TestIssuer, TestIssuer),
            });

            if (UmbracoBackOfficeIdentity.FromClaimsIdentity(claimsIdentity, out _))
                Assert.Fail();

            Assert.Pass();
        }

        [Test]
        public void Create_From_Claims_Identity_Required_Claim_Null()
        {
            var claimsIdentity = new ClaimsIdentity(new[]
            {
                //null or empty
                new Claim(ClaimTypes.NameIdentifier, "", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                new Claim(ClaimTypes.Name, "testing", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(ClaimTypes.GivenName, "hello world", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(Constants.Security.StartContentNodeIdClaimType, "-1", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                new Claim(Constants.Security.StartMediaNodeIdClaimType, "5543", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                new Claim(Constants.Security.AllowedApplicationsClaimType, "content", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(Constants.Security.AllowedApplicationsClaimType, "media", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(ClaimTypes.Locality, "en-us", ClaimValueTypes.String, TestIssuer, TestIssuer),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, "admin", ClaimValueTypes.String, TestIssuer, TestIssuer),
            });

            if (UmbracoBackOfficeIdentity.FromClaimsIdentity(claimsIdentity, out _))
                Assert.Fail();

            Assert.Pass();
        }


        [Test]
        public void Create_With_Claims_And_User_Data()
        {
            var securityStamp = Guid.NewGuid().ToString();

            var identity = new ClaimsIdentity(new[]
            {
                new Claim("TestClaim1", "test", ClaimValueTypes.Integer32, TestIssuer, TestIssuer),
                new Claim("TestClaim1", "test", ClaimValueTypes.Integer32, TestIssuer, TestIssuer)
            });

            identity.AddRequiredBackofficeClaims(
                "1234",
                "testing",
                "hello world",
                new[] { 654 },
                new[] { 654 },
                "en-us",
                securityStamp,
                new[] { "content", "media" },
                new[] { "admin" });

            Assert.AreEqual(12, identity.Claims.Count());
            Assert.IsNull(identity.Actor);
        }


        [Test]
        public void Clone()
        {
            var securityStamp = Guid.NewGuid().ToString();

            var identity = new ClaimsIdentity().AddRequiredBackofficeClaims(
                "1234", "testing", "hello world", new[] { 654 }, new[] { 654 }, "en-us", securityStamp, new[] { "content", "media" }, new[] { "admin" });

            // this will be filtered out during cloning
            identity.AddClaim(new Claim(Constants.Security.TicketExpiresClaimType, "test"));

            var cloned = identity.Clone();
            Assert.IsNull(cloned.Actor);

            Assert.AreEqual(10, cloned.Claims.Count());
        }

    }
}
