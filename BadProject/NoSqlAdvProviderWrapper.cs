using ThirdParty;

namespace Adv
{
	/// <summary>
	/// Wrap the third party source to provide a layer of separation between the source and the user of the source.
	/// </summary>
	public class NoSqlAdvProviderWrapper: RemoteAddSource
	{
		private NoSqlAdvProvider adProvider = new NoSqlAdvProvider();
		
		public Advertisement GetAdv(string id)
		{
			return adProvider.GetAdv(id);
		}
	}
}