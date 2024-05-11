using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp;
internal class FileCacheZoneEntry
{
   public required string Id { get; set; }
   public IDictionary<string, string> DnsRecords { get; set; } = new Dictionary<string, string>();
}
