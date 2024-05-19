using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp;
internal class FileCachedIdMappings([FromKeyedServices("api")] IIdMappings component, IConfiguration configuration) : IIdMappings, IDisposable
{
   private readonly IIdMappings _component = component;
   private IDictionary<string, FileCacheZoneEntry>? _cache;
   private string? _cacheFilePath;

   public async ValueTask<string?> GetZoneId(string zoneName)
   {
      // Try to get the zone ID from the cache
      var cache = GetCache();
      if (cache.TryGetValue(zoneName, out var zoneCache)) return zoneCache.Id;

      // If not found, get it from the component and cache it
      var zoneId = await _component.GetZoneId(zoneName);
      if (zoneId is not null) cache[zoneName] = new FileCacheZoneEntry { Id = zoneId };
      return zoneId;
   }

   public async ValueTask<string?> GetDnsRecordId(string zoneName, string recordName)
   {
      // Try to get the record ID from the cache
      var cache = GetCache();
      if (cache.TryGetValue(zoneName, out var zoneEntry) && zoneEntry.DnsRecords.TryGetValue(recordName, out var recordId)) return recordId;

      // If not found, get it from the component and cache it
      recordId = await _component.GetDnsRecordId(zoneName, recordName);
      if (recordId is not null)
      {
         if (!cache.TryGetValue(zoneName, out zoneEntry))
         {
            var zoneId = await _component.GetZoneId(zoneName);
            if (zoneId is null) return null;

            zoneEntry = new FileCacheZoneEntry { Id = zoneId };
            cache[zoneName] = zoneEntry;
         }

         zoneEntry.DnsRecords[recordName] = recordId;
      }
      return recordId;
   }

   public void Dispose()
   {
      if (_cache is not null)
      {
         var fileContent = JsonSerializer.Serialize(_cache);
         File.WriteAllText(GetCacheFilePath(), fileContent);
      }
   }

   private string GetCacheFilePath()
   {
      if (_cacheFilePath is not null) return _cacheFilePath;
      var homeFolder = Environment.GetEnvironmentVariable("HOME") 
         ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
      var appDataFolder = Path.Combine(homeFolder, ".update-dns");
      if (!Directory.Exists(appDataFolder)) Directory.CreateDirectory(appDataFolder);
      return _cacheFilePath = Path.Combine(appDataFolder, "idMappings.json");
   }

   private IDictionary<string, FileCacheZoneEntry> GetCache()
   {
      if(_cache is not null) return _cache;

      var cacheFilePath = GetCacheFilePath();

      if (!File.Exists(cacheFilePath)) return _cache = new Dictionary<string, FileCacheZoneEntry>();
      var fileContent = File.ReadAllText(cacheFilePath);
      return _cache = JsonSerializer.Deserialize<IDictionary<string, FileCacheZoneEntry>>(fileContent) ?? new Dictionary<string, FileCacheZoneEntry>();      
   }
}
