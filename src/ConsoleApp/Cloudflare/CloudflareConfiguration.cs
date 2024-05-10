using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp.Cloudflare;
internal class CloudflareConfiguration
{
   public const string SectionName = "Cloudflare";

   public required string ApiKey { get; set; }
   public required string ZoneId { get; set; }
   public required string RecordId { get; set; }
}
