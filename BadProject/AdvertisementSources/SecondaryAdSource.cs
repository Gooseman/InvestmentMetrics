using System.Threading.Tasks;
using ThirdParty;

namespace BadProject.AdvertisementSources
{
	public class SecondaryAdSource: AdvertSource
	{
		public Task<Advertisement> TryGetAdvertAsync(string id)
		{
			return Task.FromResult(SQLAdvProvider.GetAdv(id));
		}
	}
}