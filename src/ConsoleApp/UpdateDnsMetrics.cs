using System.Diagnostics.Metrics;

namespace ConsoleApp;

[RegisterSingleton]
public class UpdateDnsMetrics
{
   private readonly Counter<int> _dnsUpdates;
   private readonly Counter<int> _dnsUpdateErrors;
   private readonly Counter<int> _dnsRemainsUnchanged;

   public UpdateDnsMetrics(IMeterFactory meterFactory)
   {
      var meter = meterFactory.Create("Dns");
      _dnsUpdates = meter.CreateCounter<int>("dns.updates");
      _dnsUpdateErrors = meter.CreateCounter<int>("dns.update.errors");
      _dnsRemainsUnchanged = meter.CreateCounter<int>("dns.unchanged");
   }

   public void DnsUpdated() => _dnsUpdates.Add(1);
   public void DnsUpdateFailed() => _dnsUpdateErrors.Add(1);
   
   public void DnsRemainsUnchanged() => _dnsRemainsUnchanged.Add(1);
}