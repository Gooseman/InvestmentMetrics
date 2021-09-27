using ThirdParty;

namespace BadProjectTests
{
	public static class AdvertCreator
	{
		public static Advertisement Create(string id)
		{
			return new Advertisement { WebId = id, Name = NameFromId(id) };
		}

		public static string NameFromId(string id)
		{
			return $"Advertisement #{id}";
		}
	}
}