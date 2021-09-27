using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BadProject;
using BadProject.AdvertisementSources;
using Moq;
using NUnit.Framework;
using ThirdParty;

namespace BadProjectTests
{
	public class AdvertisementServiceTests
	{
		private static readonly AdvertCache defaultCache = new AdvertCache();

		[Test]
		public void TestGetAdvertisement_AfterDefaultConstructor_GetsAdvert()
		{
			string adId = "1";
			Advertisement advert = new AdvertisementService().GetAdvertisement(adId).Result;
			
			Assert.IsTrue(advert.WebId.Contains(adId));
			Assert.IsTrue(advert.Name.Contains(adId));
		}
		
		[Test]
		public void TestAdvertisementService_NullAdvertSources_ThrowsException()
		{
			Assert.Throws<ArgumentNullException>(() => new AdvertisementService(defaultCache, null));
		}

		[Test]
		public void TestAdvertisementService_NullCache_ThrowsException()
		{
			Assert.Throws<ArgumentNullException>(() => new AdvertisementService(null, new List<AdvertSource>()));
		}
		
		[Test]
		public void TestGetAdvertisement_InCache_RetrievedFromCache()
		{
			string advertId = "1";
			Mock<AdvertSource> primaryAdSource = AdSource();
			AdvertCache cacheWithEntry = new AdvertCache();

			cacheWithEntry.Add(advertId, AdvertCreator.Create(advertId));

			Advertisement advert =
				new AdvertisementService(cacheWithEntry, new List<AdvertSource>() { primaryAdSource.Object })
					.GetAdvertisement(advertId)
					.Result;

			Assert.AreEqual(advertId, advert.WebId);
			Assert.AreEqual(AdvertCreator.NameFromId(advertId), advert.Name);
			Assert.IsNull(advert.Description);
		}

		private static Mock<AdvertSource> AdSource(params string[] ids)
		{
			Mock<AdvertSource> primaryAdSource = new Mock<AdvertSource>();
			HashSet<string> adIds = new HashSet<string>(ids);

			primaryAdSource.Setup(ads => ads.TryGetAdvertAsync(It.Is<string>(id => adIds.Contains(id))))
				.Returns<string>((id) => Task.FromResult(AdvertCreator.Create(id)));
			return primaryAdSource;
		}

		[Test]
		public void TestGetAdvertisement_NotInCache_RetrievedFromPrimarySource()
		{
			string advertId = "1";
			Mock<AdvertSource> primaryAdSource = AdSource(advertId);

			Advertisement advert =
				new AdvertisementService(defaultCache, new List<AdvertSource>() { primaryAdSource.Object })
					.GetAdvertisement(advertId)
					.Result;

			Assert.AreEqual(advertId, advert.WebId);
			Assert.AreEqual(AdvertCreator.NameFromId(advertId), advert.Name);
			Assert.IsNull(advert.Description);
		}

		[Test]
		public void TestGetAdvertisement_NotInCacheOrPrimary_RetrievedFromSecondarySource()
		{
			string advertId = "1";
			Mock<AdvertSource> primaryAdSource = AdSource();
			Mock<AdvertSource> secondaryAdSource = AdSource(advertId);

			Advertisement advert = new AdvertisementService(
					defaultCache,
					new List<AdvertSource>() { primaryAdSource.Object, secondaryAdSource.Object })
				.GetAdvertisement(advertId)
				.Result;

			Assert.AreEqual(advertId, advert.WebId);
			Assert.AreEqual(AdvertCreator.NameFromId(advertId), advert.Name);
			Assert.IsNull(advert.Description);
		}

		[Test]
		public void TestGetAdvertisement_InCacheAfterRetrievalFromPrimary_RetrievedFromCache()
		{
			string advertId = "1";
			Mock<AdvertSource> primaryAdSource = AdSource(advertId);
			AdvertisementService adverts = new AdvertisementService(
				defaultCache,
				new List<AdvertSource>() { primaryAdSource.Object });

			adverts.GetAdvertisement(advertId);
			primaryAdSource.Reset();

			Advertisement advert = adverts.GetAdvertisement(advertId).Result;

			Assert.AreEqual(advertId, advert.WebId);
			Assert.AreEqual(AdvertCreator.NameFromId(advertId), advert.Name);
			Assert.IsNull(advert.Description);
		}

		[Test]
		public void TestGetAdvertisement_InCacheAfterRetrievalFromSecondary_RetrievedFromCache()
		{
			string advertId = "1";
			Mock<AdvertSource> primaryAdSource = AdSource();
			Mock<AdvertSource> secondaryAdSource = AdSource(advertId);
			AdvertisementService adverts = new AdvertisementService(
				defaultCache,
				new List<AdvertSource>() { primaryAdSource.Object, secondaryAdSource.Object });

			adverts.GetAdvertisement(advertId);
			secondaryAdSource.Reset();

			Advertisement advert = adverts.GetAdvertisement(advertId).Result;

			Assert.AreEqual(advertId, advert.WebId);
			Assert.AreEqual(AdvertCreator.NameFromId(advertId), advert.Name);
			Assert.IsNull(advert.Description);
		}
	}
}