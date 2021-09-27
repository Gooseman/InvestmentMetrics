using System.Threading.Tasks;
using ThirdParty;

namespace BadProject.AdvertisementSources
{
	public interface AdvertSource
	{
		Task<Advertisement> TryGetAdvertAsync(string id);
	}
}