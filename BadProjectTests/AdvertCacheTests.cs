using System;
using System.Threading.Tasks;
using Adv;
using NUnit.Framework;
using ThirdParty;

namespace BadProjectTests
{
	public class AdvertCacheTests
	{
		[Test]
		public void TestGet_EmptyCache_NullReturned()
		{
			Assert.IsNull(new AdvertCache().Get("1"));
		}
		
		[Test]
		public void TestGet_AdNotInCache_NullReturned()
		{
			var cache = new AdvertCache();
			string adId = "2";
			
			cache.Add(adId, AdvertCreator.Create(adId));
			Assert.IsNull(cache.Get("1"));
		}
		
		[Test]
		public void TestGet_AdInCache_AdReturned()
		{
			var cache = new AdvertCache();
			string adId = "1";
			
			cache.Add(adId, AdvertCreator.Create(adId));

			Advertisement adFromCache = cache.Get(adId);
			
			Assert.AreEqual(adId, adFromCache.WebId);
		}
		
		[Test]
		public void TestGet_AdExpired_NullReturned()
		{
			var cache = new AdvertCache(TimeSpan.FromMilliseconds(1));
			string adId = "1";
			
			cache.Add(adId, AdvertCreator.Create(adId));
			Task.Delay(2).Wait();

			Assert.IsNull(cache.Get(adId));
		}
		
		[Test]
		public void TestGet_CacheDisposed_ThrowsException()
		{
			var cache = new AdvertCache();
			
			cache.Dispose();
			Assert.Throws<ObjectDisposedException>(() => cache.Get("1"));
		}
		
		[Test]
		public void TestAdd_CacheDisposed_ThrowsException()
		{
			string adId = "1";
			var cache = new AdvertCache();
			
			cache.Dispose();
			Assert.Throws<ObjectDisposedException>(() => cache.Add(adId, AdvertCreator.Create(adId)));
		}
	}
}