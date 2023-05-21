using Orleans.Runtime;

namespace WebApi
{
    public interface IUrlShortenerGrain : IGrainWithStringKey
    {
        Task SetUrl(string fullUrl);

        Task<string> GetUrl();
    }
    public class UrlShortenerGrain : Grain, IUrlShortenerGrain
    {
        //manages reading and writing state values for the URLs to the configured silo storage
        private readonly IPersistentState<UrlDetails> _state;

        public UrlShortenerGrain([PersistentState(stateName: "url", storageName: "urls")] IPersistentState<UrlDetails> state)
        {
            _state = state;
        }
        public Task<string> GetUrl()
        {
            return Task.FromResult(_state.State.FullUrl);
        }

        public async Task SetUrl(string fullUrl)
        {
            _state.State = new UrlDetails() { ShortenedRouteSegment = this.GetPrimaryKeyString(), FullUrl = fullUrl };
            await _state.WriteStateAsync();
        }
    }

    [GenerateSerializer]
    public record UrlDetails
    {
        public string FullUrl { get; set; }
        public string ShortenedRouteSegment { get; set; }
    }
}
