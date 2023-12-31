﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Security.Claims;
using FluentAssertions;
using IdentityServer.UnitTests.Common;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Endpoints;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.UnitTests.Common;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace IdentityServer.UnitTests.Endpoints.Authorize
{
    public class AuthorizeCallbackEndpointTests
    {
        private const string Category = "Authorize Endpoint";

        private HttpContext _context;

        private TestEventService _fakeEventService =
            new TestEventService();

        private ILogger<AuthorizeCallbackEndpoint> _fakeLogger =
            TestLogger.Create<AuthorizeCallbackEndpoint>();

        private IdentityServerOptions _options =
            new IdentityServerOptions();

        private MockConsentResponseMessageStore _mockUserConsentResponseResponseMessageStore =
            new MockConsentResponseMessageStore();

        private MockUserSession _mockUserSession =
            new MockUserSession();

        private NameValueCollection _params =
            new NameValueCollection();

        private StubAuthorizeRequestValidator _stubAuthorizeRequestValidator =
            new StubAuthorizeRequestValidator();

        private StubAuthorizeResponseGenerator _stubAuthorizeResponseGenerator =
            new StubAuthorizeResponseGenerator();

        private StubAuthorizeInteractionResponseGenerator _stubInteractionGenerator =
            new StubAuthorizeInteractionResponseGenerator();

        private MockStore _mockStore =
            new MockStore();

        private MockLoginRequestStore _mockLoginRequestStore =
            new MockLoginRequestStore();

        private MockLoginResponseStore _mockLoginResponseStore =
            new MockLoginResponseStore();

        private MockConsentRequestStore _mockConsentRequestStore =
            new MockConsentRequestStore();

        private MockConsentResponseStore _mockConsentResponseStore =
            new MockConsentResponseStore();

        private AuthorizeCallbackEndpoint _subject;

        private ClaimsPrincipal _user =
            new IdentityServerUser("bob").CreatePrincipal();

        private ValidatedAuthorizeRequest _validatedAuthorizeRequest;

        public AuthorizeCallbackEndpointTests()
        {
            Init();
        }

        [Fact(Skip = "needs to be fixed")]
        [Trait("Category", Category)]
        public async Task ProcessAsync_authorize_after_consent_path_should_return_authorization_result()
        {
            var parameters = new NameValueCollection()
            {
                { "client_id", "client" },
                { "nonce", "some_nonce" },
                { "scope", "api1 api2" }
            };
            var request = new ConsentRequest(parameters, _user.GetSubjectId());
            _mockUserConsentResponseResponseMessageStore.Messages.Add(request.Id,
                new Message<ConsentResponse>(new ConsentResponse()));

            _mockUserSession.User = _user;

            _context.Request.Method = "GET";
            _context.Request.Path = new PathString("/connect/authorize/callback");
            _context.Request.QueryString = new QueryString("?" + parameters.ToQueryString());

            var result = await _subject.ProcessAsync(_context);

            result.Should().BeOfType<AuthorizeResult>();
        }

        [Fact(Skip = "needs to be fixed")]
        [Trait("Category", Category)]
        public async Task ProcessAsync_authorize_after_login_path_should_return_authorization_result()
        {
            _context.Request.Method = "GET";
            _context.Request.Path = new PathString("/connect/authorize/callback");
            _mockUserSession.User = _user;

            var result = await _subject.ProcessAsync(_context);

            result.Should().BeOfType<AuthorizeResult>();
        }

        [Fact(Skip = "needs to be fixed")]
        [Trait("Category", Category)]
        public async Task ProcessAsync_consent_missing_consent_data_should_return_error_page()
        {
            var parameters = new NameValueCollection()
            {
                { "client_id", "client" },
                { "nonce", "some_nonce" },
                { "scope", "api1 api2" }
            };
            var request = new ConsentRequest(parameters, _user.GetSubjectId());
            _mockUserConsentResponseResponseMessageStore.Messages.Add(request.Id, new Message<ConsentResponse>(null));

            _mockUserSession.User = _user;

            _context.Request.Method = "GET";
            _context.Request.Path = new PathString("/connect/authorize/callback");
            _context.Request.QueryString = new QueryString("?" + parameters.ToQueryString());

            var result = await _subject.ProcessAsync(_context);

            result.Should().BeOfType<AuthorizeResult>();
            ((AuthorizeResult)result).Response.IsError.Should().BeTrue();
        }

        [Fact(Skip = "needs to be fixed")]
        [Trait("Category", Category)]
        public async Task ProcessAsync_no_consent_message_should_return_redirect_for_consent()
        {
            _stubInteractionGenerator.Response.IsConsent = true;

            var parameters = new NameValueCollection()
            {
                { "client_id", "client" },
                { "nonce", "some_nonce" },
                { "scope", "api1 api2" }
            };
            var request = new ConsentRequest(parameters, _user.GetSubjectId());
            _mockUserConsentResponseResponseMessageStore.Messages.Add(request.Id, null);

            _mockUserSession.User = _user;

            _context.Request.Method = "GET";
            _context.Request.Path = new PathString("/connect/authorize/callback");
            _context.Request.QueryString = new QueryString("?" + parameters.ToQueryString());

            var result = await _subject.ProcessAsync(_context);

            result.Should().BeOfType<ConsentPageResult>();
        }

        [Fact(Skip = "needs to be fixed")]
        [Trait("Category", Category)]
        public async Task ProcessAsync_post_to_entry_point_should_return_405()
        {
            _context.Request.Method = "POST";

            var result = await _subject.ProcessAsync(_context);

            var statusCode = result as StatusCodeResult;
            statusCode.Should().NotBeNull();
            statusCode.StatusCode.Should().Be(405);
        }

        [Fact(Skip = "needs to be fixed")]
        [Trait("Category", Category)]
        public async Task ProcessAsync_valid_consent_message_should_cleanup_consent_cookie()
        {
            var parameters = new NameValueCollection()
            {
                { "client_id", "client" },
                { "nonce", "some_nonce" },
                { "scope", "api1 api2" }
            };
            var request = new ConsentRequest(parameters, _user.GetSubjectId());
            _mockUserConsentResponseResponseMessageStore.Messages.Add(request.Id,
                new Message<ConsentResponse>(new ConsentResponse()
                    { ScopesValuesConsented = new string[] { "api1", "api2" } }));

            _mockUserSession.User = _user;

            _context.Request.Method = "GET";
            _context.Request.Path = new PathString("/connect/authorize/callback");
            _context.Request.QueryString = new QueryString("?" + parameters.ToQueryString());

            var result = await _subject.ProcessAsync(_context);

            _mockUserConsentResponseResponseMessageStore.Messages.Count.Should().Be(0);
        }

        [Fact(Skip = "needs to be fixed")]
        [Trait("Category", Category)]
        public async Task ProcessAsync_valid_consent_message_should_return_authorize_result()
        {
            var parameters = new NameValueCollection()
            {
                { "client_id", "client" },
                { "nonce", "some_nonce" },
                { "scope", "api1 api2" }
            };
            var request = new ConsentRequest(parameters, _user.GetSubjectId());
            _mockUserConsentResponseResponseMessageStore.Messages.Add(request.Id,
                new Message<ConsentResponse>(new ConsentResponse()
                    { ScopesValuesConsented = new string[] { "api1", "api2" } }));

            _mockUserSession.User = _user;

            _context.Request.Method = "GET";
            _context.Request.Path = new PathString("/connect/authorize/callback");
            _context.Request.QueryString = new QueryString("?" + parameters.ToQueryString());

            var result = await _subject.ProcessAsync(_context);

            result.Should().BeOfType<AuthorizeResult>();
        }

        internal void Init()
        {
            _context = new MockHttpContextAccessor().HttpContext;

            _validatedAuthorizeRequest = new ValidatedAuthorizeRequest()
            {
                RedirectUri = "http://client/callback",
                State = "123",
                ResponseMode = "fragment",
                ClientId = "client",
                Client = new Client
                {
                    ClientId = "client",
                    ClientName = "Test Client"
                },
                Raw = _params,
                Subject = _user
            };
            _stubAuthorizeResponseGenerator.Response.Request = _validatedAuthorizeRequest;

            _stubAuthorizeRequestValidator.Result = new AuthorizeRequestValidationResult(_validatedAuthorizeRequest);

            _subject = new AuthorizeCallbackEndpoint(
                events: _fakeEventService,
                logger: _fakeLogger,
                options: _options,
                validator: _stubAuthorizeRequestValidator,
                interactionGenerator: _stubInteractionGenerator,
                authorizeResponseGenerator: _stubAuthorizeResponseGenerator,
                userSession: _mockUserSession,
                consentResponseResponseStore: _mockUserConsentResponseResponseMessageStore,
                loginRequestStore: _mockLoginRequestStore,
                loginResponseStore: _mockLoginResponseStore,
                consentRequestStore: _mockConsentRequestStore,
                consentResponseStore: _mockConsentResponseStore,
                authorizeRequest2Store: null
            );
        }
    }
}