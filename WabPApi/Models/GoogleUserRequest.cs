using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WabPApi.Models
{
    public class GoogleUserRequest
    {
        public const string PROVIDER = "google";
        [JsonProperty("idToken")]
        [Required]
        public string IdToken { get; set; }

    }
}
