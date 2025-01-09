using System.Diagnostics.Metrics;

namespace ConsoleApp;

[RegisterSingleton]
public class UpdateDnsMetrics
{
   private readonly Counter<int> _dnsUpdates;
   private readonly Counter<int> _dnsUpdateErrors;
   private readonly Counter<int> _dnsRemainsUnchanged;
   private readonly Histogram<long> _dnsUpdateDuration;

   public UpdateDnsMetrics(IMeterFactory meterFactory)
   {
      var meter = meterFactory.Create("UpdateDns");
      _dnsUpdates = meter.CreateCounter<int>("dns.update.updated");
      _dnsUpdateErrors = meter.CreateCounter<int>("dns.update.errors");
      _dnsRemainsUnchanged = meter.CreateCounter<int>("dns.update.unchanged");
        _dnsUpdateDuration = meter.CreateHistogram<long>("dns.update.duration", "ms");
   }

   public void DnsUpdated() => _dnsUpdates.Add(1);
   public void DnsUpdateFailed() => _dnsUpdateErrors.Add(1);
   
   public void DnsRemainsUnchanged() => _dnsRemainsUnchanged.Add(1);
    public void DnsUpdateDuration(long duration) => _dnsUpdateDuration.Record(duration);
}