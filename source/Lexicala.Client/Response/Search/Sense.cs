﻿using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Search
{
    public class Sense
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("definition")]
        public string Definition { get; set; }
    }
}