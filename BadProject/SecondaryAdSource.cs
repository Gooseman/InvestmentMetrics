using System.Threading.Tasks;
using ThirdParty;

namespace Adv
{
	public class SecondaryAdSource: AdvertSource
	{
		public Task<Advertisement> TryGetAdvertAsync(string id)
		{
			return Task.FromResult(SQLAdvProvider.GetAdv(id));
		}
	}
}