using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ConsoleApp1.Models
{
    public class BaseEntity
    {
        [JsonIgnore]
        public long Id { get; set; }
        [JsonIgnore]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
