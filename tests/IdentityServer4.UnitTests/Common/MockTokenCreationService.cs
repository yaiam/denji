﻿using IdentityServer4.Models;
using IdentityServer4.Services;

namespace IdentityServer.UnitTests.Common
{
    class MockTokenCreationService : ITokenCreationService
    {
        public string Token { get; set; }

        public Task<string> CreateTokenAsync(Token token)
        {
            return Task.FromResult(Token);
        }
    }
}
