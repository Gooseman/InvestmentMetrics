using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Adv
{
	public class ReverseDateOrder : IComparer<DateTime>
	{
		public int Compare(DateTime lhs, DateTime rhs)
		{
			return -(lhs.CompareTo(rhs));
		}
	}

	/// <summary>
	/// This class manages a simple collection of times at which errors occurred approximately during the last hour.
	/// It uses locking to prevent changes to the collection from happening simultaneously as well as a count of the
	/// errors in the last hour from occuring at the same time as a change to the collection.
	///
	/// It requires that classes using it are stopped before it is disposed.  Otherwise it would be possible for a user
	/// to be waiting for a lock when Dispose is called, resulting in a use of the class after it has been disposed.
	/// </summary>
	public sealed class AdvertErrors : IDisposable
	{
		private const int MaximumAgeHours = 1;
		private SortedList<DateTime, int> errors = new SortedList<DateTime, int>(new ReverseDateOrder());
		private Timer oldestErrorExpiry;
		private bool isDisposed;
		private object oneAtATime = new object();

		public void Add(DateTime errorTime)
		{
			if (isDisposed)
			{
				throw new ObjectDisposedException($"This instance of {nameof(AdvertErrors)} has been disposed");
			}

			if (errorTime.AddHours(MaximumAgeHours) < DateTime.Now)
			{
				return;
			}

			Monitor.Enter(oneAtATime);

			try
			{
				errors.Add(errorTime, 1);

				if (errors.Count == 1)
				{
					StartOldestErrorExpiryTimer(errorTime);
				}
			}
			catch (ArgumentException invalidArg)
			{
				if (invalidArg.Message.Contains("already exists"))
				{
					return;
				}

				throw;
			}
			finally
			{
				Monitor.Exit(oneAtATime);
			}
		}

		private void StartOldestErrorExpiryTimer(DateTime errorTime)
		{
			oldestErrorExpiry = new Timer((errorTime.AddHours(MaximumAgeHours) - DateTime.Now).TotalMilliseconds);
			oldestErrorExpiry.Elapsed += OnErrorExpired;
			oldestErrorExpiry.Start();
		}

		private void OnErrorExpired(object _, ElapsedEventArgs timeout)
		{
			Monitor.Enter(oneAtATime);

			try
			{
				DateTime now = DateTime.Now;

				do
				{
					errors.RemoveAt(errors.Count - 1);
				}
				while (errors.Count > 0 && errors.Last().Key.AddHours(MaximumAgeHours) < now);

				CleanUpOldestErrorExpiry();

				if (errors.Count > 0)
				{
					StartOldestErrorExpiryTimer(errors.Last().Key);
				}
			}
			finally
			{
				Monitor.Exit(oneAtATime);
			}
		}

		private void CleanUpOldestErrorExpiry()
		{
			if (null == oldestErrorExpiry)
			{
				return;
			}

			oldestErrorExpiry.Elapsed -= OnErrorExpired;
			oldestErrorExpiry.Dispose();
			oldestErrorExpiry = null;
		}

		/// <summary>
		/// The method makes the assumption that the errors can be counted fast enough that none will be more than an
		/// hour ago by the time the counting completes.
		///
		/// The locking is to prevent changes to the collection from occurring while counting is happening.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="ObjectDisposedException"></exception>
		public int InLastHour()
		{
			if (isDisposed)
			{
				throw new ObjectDisposedException($"This instance of {nameof(AdvertErrors)} has been disposed");
			}

			Monitor.Enter(oneAtATime);

			DateTime anHourAgo = DateTime.Now.AddHours(-1);
			int numErrorsInLastHour = errors.Keys.Where(errorAt => errorAt > anHourAgo).Count();

			Monitor.Exit(oneAtATime);
			return numErrorsInLastHour;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);
		}

		/// <summary>
		/// It is assumed that only a single thread at a time is going to call Dispose.  The locking is to prevent
		/// the method from continuing while
		/// </summary>
		/// <param name="isDisposing"></param>
		private void Dispose(bool isDisposing)
		{
			if (!isDisposing)
			{
				return;
			}

			if (isDisposed)
			{
				return;
			}

			isDisposed = true;
			CleanUpOldestErrorExpiry();
		}
	}
}