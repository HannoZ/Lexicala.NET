﻿using Newtonsoft.Json;

namespace Lexicala.NET.Client.Response.Search
{
    public class Headword
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("pos")]
        public string Pos { get; set; }
    }
}