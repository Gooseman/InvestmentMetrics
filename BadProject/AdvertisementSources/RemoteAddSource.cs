using ThirdParty;

namespace BadProject.AdvertisementSources
{
	/// <summary>
	/// This interface matches that of third party advert data sources.  It allows wrapping of a third party source in
	/// a local class to separate local use of third party sources from those sources.
	/// </summary>
	public interface RemoteAddSource
	{
		Advertisement GetAdv(string id);
	}
}