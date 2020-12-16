﻿using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Umbraco.Core.Security;
using Umbraco.Extensions;

namespace Umbraco.Web.BackOffice.Security
{

    /// <summary>
    /// Custom secure format that ensures the Identity in the ticket is a valid ClaimsIdentity for backoffice.
    /// </summary>
    internal class BackOfficeSecureDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly int _loginTimeoutMinutes;
        private readonly ISecureDataFormat<AuthenticationTicket> _ticketDataFormat;

        public BackOfficeSecureDataFormat(int loginTimeoutMinutes, ISecureDataFormat<AuthenticationTicket> ticketDataFormat)
        {
            _loginTimeoutMinutes = loginTimeoutMinutes;
            _ticketDataFormat = ticketDataFormat ?? throw new ArgumentNullException(nameof(ticketDataFormat));
        }

        public string Protect(AuthenticationTicket data, string purpose)
        {
            // create a new ticket based on the passed in tickets details, however, we'll adjust the expires utc based on the specified timeout mins
            var ticket = new AuthenticationTicket(data.Principal,
                new AuthenticationProperties(data.Properties.Items)
                {
                    IssuedUtc = data.Properties.IssuedUtc,
                    ExpiresUtc = data.Properties.ExpiresUtc ?? DateTimeOffset.UtcNow.AddMinutes(_loginTimeoutMinutes),
                    AllowRefresh = data.Properties.AllowRefresh,
                    IsPersistent = data.Properties.IsPersistent,
                    RedirectUri = data.Properties.RedirectUri
                }, data.AuthenticationScheme);

            return _ticketDataFormat.Protect(ticket);
        }

        public string Protect(AuthenticationTicket data) => Protect(data, string.Empty);


        public AuthenticationTicket Unprotect(string protectedText) => Unprotect(protectedText, string.Empty);

        /// <summary>
        /// Un-protects the cookie
        /// </summary>
        /// <param name="protectedText"></param>
        /// <param name="purpose"></param>
        /// <returns></returns>
        public AuthenticationTicket Unprotect(string protectedText, string purpose)
        {
            AuthenticationTicket decrypt;
            try
            {
                decrypt = _ticketDataFormat.Unprotect(protectedText);
                if (decrypt == null)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }

            ClaimsIdentity identity = decrypt.Principal.VerifyBackOfficeIdentity();
            if (identity is null)
            {
                return null;
            }

            // return the ticket with a UmbracoBackOfficeIdentity
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), decrypt.Properties, decrypt.AuthenticationScheme);

            return ticket;
        }
    }
}
