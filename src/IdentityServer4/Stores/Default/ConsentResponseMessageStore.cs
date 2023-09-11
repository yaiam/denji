﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;

namespace IdentityServer4.Stores
{
    internal class ConsentResponseMessageStore : IConsentResponseMessageStore
    {
        protected readonly MessageCookie<ConsentResponse> Cookie;

        public ConsentResponseMessageStore(MessageCookie<ConsentResponse> cookie)
        {
            Cookie = cookie;
        }

        public virtual Task DeleteAsync(string id)
        {
            Cookie.Clear(id);
            return Task.CompletedTask;
        }

        public virtual Task<Message<ConsentResponse>> ReadAsync(string id)
        {
            return Task.FromResult(Cookie.Read(id));
        }

        public virtual Task WriteAsync(string id, Message<ConsentResponse> message)
        {
            Cookie.Write(id, message);
            return Task.CompletedTask;
        }
    }
}
