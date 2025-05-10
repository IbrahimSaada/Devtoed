using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Devoted.Domain.Sql.Response.Base
{
    public class BaseResponse
    {
        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("dateTime")]
        public DateTime DateTime => DateTime.UtcNow;

        [JsonProperty("data")]
        public object Data { get; set; }
    }
}
