using System;
using System.Runtime.Caching;
using ThirdParty;

namespace BadProject.AdvertisementSources
{
	public sealed class AdvertCache : IDisposable
	{
		private static readonly TimeSpan DefaultAdLifetime = TimeSpan.FromMinutes(5);
		private readonly TimeSpan adLifetime = DefaultAdLifetime;
		private MemoryCache cache = new MemoryCache("Advertisements");
		private bool isDisposed;

		public AdvertCache()
		{
		}

		public AdvertCache(TimeSpan adLifetime)
		{
			if (adLifetime.TotalMilliseconds < 0)
			{
				return;
			}
			
			this.adLifetime = adLifetime;
		}

		public Advertisement Get(string id)
		{
			if (isDisposed)
			{
				throw new ObjectDisposedException($"This {nameof(AdvertCache)} has been disposed");
			}
			
			return (Advertisement) cache.Get(AdvertKey(id));
		}

		public void Add(string id, Advertisement ad)
		{
			if (isDisposed)
			{
				throw new ObjectDisposedException($"This {nameof(AdvertCache)} has been disposed");
			}
			
			cache.Set(AdvertKey(id), ad, DateTimeOffset.Now.Add(adLifetime));
		}

		private static string AdvertKey(string id)
		{
			return $"AdvKey_{id}";
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		private void Dispose(bool isDisposing)
		{
			if (!isDisposing)
			{
				return;
			}

			isDisposed = true;
			cache?.Dispose();
			cache = null;
		}
	}
}