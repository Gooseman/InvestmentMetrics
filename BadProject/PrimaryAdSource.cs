using System;
using System.Configuration;
using System.Threading.Tasks;
using ThirdParty;

namespace Adv
{
	public class PrimaryAdSource : AdvertSource
	{
		private Func<RemoteAddSource> getAdvertProvider;
		private static readonly int MaxRetries = int.Parse(ConfigurationManager.AppSettings["RetryCount"]);
		private readonly AdvertErrors errors = new AdvertErrors();
		private const int MaxErrorsInLastHour = 10;
		private static readonly TimeSpan DefaultWaitForRetry = TimeSpan.FromMilliseconds(1000);
		private readonly TimeSpan waitForRetry = DefaultWaitForRetry;

		public PrimaryAdSource(Func<RemoteAddSource> getAdvertProvider)
		{
			if (null == getAdvertProvider)
			{
				throw new ArgumentNullException(
					nameof(getAdvertProvider),
					$"A valid {typeof(Func<RemoteAddSource>).Name} must be provided");
			}

			this.getAdvertProvider = getAdvertProvider;
		}
			
		public PrimaryAdSource(Func<RemoteAddSource> getAdvertProvider, TimeSpan waitForRetry): this(getAdvertProvider)
		{
			if (TimeSpan.Zero > waitForRetry)
			{
				throw new ArgumentException(nameof(waitForRetry), "Cannot wait less than 0 seconds");
			}
			
			this.waitForRetry = waitForRetry;
		}
		
		public async Task<Advertisement> TryGetAdvertAsync(string id)
		{
			int errorsInLastHour = errors.InLastHour();

			if (errorsInLastHour > MaxErrorsInLastHour)
			{
				return null;
			}

			return await TryGetAdvertAsync(id, errorsInLastHour);
		}

		private async Task<Advertisement> TryGetAdvertAsync(string id, int numErrorsInLastHour)
		{
			Advertisement adv = null;
			int retry = 0;
			var advertProvider = getAdvertProvider();

			do
			{
				retry++;

				try
				{
					adv = advertProvider.GetAdv(id);
				}
				catch
				{
					// TODO: Log the error
					errors.Add(DateTime.Now);
					numErrorsInLastHour = errors.InLastHour();

					if (numErrorsInLastHour >= MaxErrorsInLastHour)
					{
						return null;
					}

					await Task.Delay(waitForRetry);
				}
			}
			while ((adv == null) && (retry < MaxRetries));

			return adv;
		}
	}
}