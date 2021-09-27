using System.Threading.Tasks;
using ThirdParty;

namespace Adv
{
	public interface AdvertSource
	{
		Task<Advertisement> TryGetAdvertAsync(string id);
	}
}