using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThirdParty;

namespace Adv
{
	public class AdvertisementService
	{
		// Typically these would be shared amongst instances.  However, since this class doesn't control the lifetimes
		// of the cache or the other advert sources, it is up to the creator of the instance to enforce that.
		private readonly AdvertCache adsInMemory;
		private readonly List<AdvertSource> advertSources;
		private readonly static Func<RemoteAddSource> getNoSqlAdvertProvider = () => new NoSqlAdvProviderWrapper();

		private readonly Object lockObj = new Object();

		public AdvertisementService() : this(
			new AdvertCache(),
			new List<AdvertSource>() { new PrimaryAdSource(getNoSqlAdvertProvider), new SecondaryAdSource() })
		{
		}

		public AdvertisementService(AdvertCache cache, IEnumerable<AdvertSource> advertSources)
		{
			if (null == cache)
			{
				throw new ArgumentNullException(nameof(cache), "The cache cannot be null");
			}

			// TODO: Should an empty collection of sources be allowed since the cache is passed into the first instance
			// created?
			if (null == advertSources)
			{
				throw new ArgumentNullException(nameof(advertSources), "Cannot retrieve adverts from null sources");
			}

			adsInMemory = cache;
			this.advertSources = advertSources.ToList();
		}

		/// <summary>
		/// Try to retrieve the advert for the given ID.
		///
		/// First check whether the ad is available in memory.
		/// If not, check each alternative source in turn until the ad is found or all sources are exhausted.
		/// If the ad is found in an alternative source, add it to memory.
		/// </summary>
		/// <param name="id">The ID of the advert to retrieve</param>
		/// <returns>The Advertisement or null</returns>
		public async Task<Advertisement> GetAdvertisement(string id)
		{
			try
			{
				// Ideally the method should allow re-entry for different IDs.
				Monitor.Enter(lockObj);

				Advertisement adv = adsInMemory.Get(id);

				if (null != adv)
				{
					return adv;
				}

				adv = await TryGetAdFromSource(id);

				if (adv != null)
				{
					adsInMemory.Add(id, adv);
				}

				return adv;
			}
			finally
			{
				if (Monitor.IsEntered(lockObj))
				{
					Monitor.Exit(lockObj);
				}
			}
		}

		private async Task<Advertisement> TryGetAdFromSource(string id)
		{
			Advertisement adv = null;
			int advertSource = 0;

			while (null == adv && advertSource < advertSources.Count)
			{
				adv = await advertSources[advertSource++].TryGetAdvertAsync(id);
			}

			return adv;
		}
	}
}