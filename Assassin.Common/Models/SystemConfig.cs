using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assassin.Common.Models
{
    public class SystemConfig : BaseModel
    {
        [JsonProperty(PropertyName = "LastSync")]
        public DateTime LastSync { get => this.GetValue<DateTime>(); set => this.SetValue(value); }
    }
}
