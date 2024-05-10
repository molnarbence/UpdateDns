using System.Text.Json.Serialization;

namespace ConsoleApp.Cloudflare;
internal abstract class ResponseBase<TResult>
{
   public IEnumerable<object> Errors { get; set; } = [];
   public IEnumerable<object> Messages { get; set; } = [];
   public bool Success { get; set; }
   public required TResult Result { get; set; }
}


internal abstract class ListResponseBase<TResult> : ResponseBase<IEnumerable<TResult>>
{
   public required ResultInfo ResultInfo { get; set; }
}


internal class ResultInfo
{
   public int Count { get; set; }
   public int Page { get; set; }
   [JsonPropertyName("per_page")]
   public int PerPage { get; set; }
   [JsonPropertyName("total_count")]
   public int TotalCount { get; set; }
}