﻿using Newtonsoft.Json;

namespace Lexicala.NET.Response.Languages
{
    public class Resource
    {
        [JsonProperty("source_languages")]
        public string[] SourceLanguages { get; set; }

        [JsonProperty("target_languages")]
        public string[] TargetLanguages { get; set; }
    }
}