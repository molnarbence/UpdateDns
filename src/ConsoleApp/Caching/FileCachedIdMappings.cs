using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ConsoleApp.Caching;

[RegisterKeyedSingleton("cache")]
internal class FileCachedIdMappings(
   [FromKeyedServices("api")] IIdMappings component, 
   IOptions<AppConfiguration> configuration) : IIdMappings, IDisposable
{
   private IDictionary<string, FileCacheZoneEntry>? _cache;
   private string? _cacheFilePath;

   public async ValueTask<string?> GetZoneId(string zoneName)
   {
      // Try to get the zone ID from the cache
      var cache = GetCache();
      if (cache.TryGetValue(zoneName, out var zoneCache)) return zoneCache.Id;

      // If not found, get it from the component and cache it
      var zoneId = await component.GetZoneId(zoneName);
      if (zoneId is not null) cache[zoneName] = new FileCacheZoneEntry { Id = zoneId };
      return zoneId;
   }

   public async ValueTask<string?> GetDnsRecordId(string zoneName, string recordName)
   {
      // Try to get the record ID from the cache
      var cache = GetCache();
      if (cache.TryGetValue(zoneName, out var zoneEntry) && zoneEntry.DnsRecords.TryGetValue(recordName, out var recordId)) return recordId;

      // If not found, get it from the component and cache it
      recordId = await component.GetDnsRecordId(zoneName, recordName);
      if (recordId is null)
      {
         return recordId;
      }

      if (!cache.TryGetValue(zoneName, out zoneEntry))
      {
         var zoneId = await component.GetZoneId(zoneName);
         if (zoneId is null) return null;

         zoneEntry = new FileCacheZoneEntry { Id = zoneId };
         cache[zoneName] = zoneEntry;
      }

      zoneEntry.DnsRecords[recordName] = recordId;
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
      var appDataFolder = configuration.Value.AppConfigurationFolderPath;
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
