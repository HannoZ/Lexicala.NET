﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lexicala.NET.Response.Me
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class Usage
    {
        [JsonProperty("today")]
        public Today Today { get; set; }

        [JsonProperty("lifetime")]
        public long Lifetime { get; set; }

        [JsonProperty("history")]
        public Dictionary<string, long> History { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}