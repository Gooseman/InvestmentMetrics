using System;
using BadProject.AdvertisementSources;
using Moq;
using NUnit.Framework;
using ThirdParty;

namespace BadProjectTests.AdvertisementSources
{
	public class PrimaryAdSourceTests
	{
		[Test]
		public void TestPrimaryAdSource_NullSourceFunction_ThrowsException()
		{
			Assert.Throws<ArgumentNullException>(() => new PrimaryAdSource(null));
		}
		
		[Test]
		public void TestPrimaryAdSource_NullSourceFunctionWithWaitTime_ThrowsException()
		{
			Assert.Throws<ArgumentNullException>(() => new PrimaryAdSource(null, TimeSpan.FromMilliseconds(100)));
		}
		
		[Test]
		public void TestPrimaryAdSource_NegativeWaitTime_ThrowsException()
		{
			Assert.Throws<ArgumentException>(
				() => new PrimaryAdSource(() => new NoSqlAdvProviderWrapper(), TimeSpan.FromMilliseconds(-100)));
		}

		[Test]
		public void TestTryGetAdvertAsync_AdvertNoFound_Retries()
		{
			string adId = "1";
			var externalAds = new Mock<RemoteAddSource>();
			var adSource = new PrimaryAdSource(() => externalAds.Object, TimeSpan.FromMilliseconds(5));

			adSource.TryGetAdvertAsync(adId);
			externalAds.Verify(ads => ads.GetAdv(It.Is<string>(id => adId == id)), Times.Exactly(3));
		}

		[Test]
		public void TestTryGetAdvertAsync_AdvertFoundImmediately_NoRetries()
		{
			string adId = "1";
			var externalAds = new Mock<RemoteAddSource>();
			var adSource = new PrimaryAdSource(() => externalAds.Object, TimeSpan.FromMilliseconds(5));

			externalAds.Setup(ads => ads.GetAdv(It.IsAny<string>()))
				.Returns<string>(id => new Advertisement() { WebId = id });
			adSource.TryGetAdvertAsync(adId);
			externalAds.Verify(ads => ads.GetAdv(It.Is<string>(id => adId == id)), Times.Once());
		}
	}
}