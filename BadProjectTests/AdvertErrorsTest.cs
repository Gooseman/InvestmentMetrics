using System;
using System.Threading;
using System.Threading.Tasks;
using Adv;
using NUnit.Framework;

namespace BadProjectTests
{
	public class AdvertErrorsTest
	{
		[Test]
		public void TestInLastHour_NoErrors_ReturnsZero()
		{
			Assert.AreEqual(0, new AdvertErrors().InLastHour());
		}

		[Test]
		public void TestInLastHour_OneErrorInLastHour_ReturnsOne()
		{
			var errors = new AdvertErrors();

			errors.Add(DateTime.Now.AddHours(-1).AddMinutes(1));
			Assert.AreEqual(1, errors.InLastHour());
		}

		[Test]
		public void TestInLastHour_AllErrorsBeforeLastHour_ReturnsZero()
		{
			var errors = new AdvertErrors();

			errors.Add(DateTime.Now.AddHours(-1).AddMinutes(-1));
			Assert.AreEqual(0, errors.InLastHour());
		}

		[Test]
		public void TestInLastHour_OneErrorOnEitherSideOfBoundary_ReturnsOne()
		{
			var errors = new AdvertErrors();

			errors.Add(DateTime.Now.AddHours(-1).AddMinutes(-1));
			errors.Add(DateTime.Now.AddHours(-1).AddMinutes(1));
			Assert.AreEqual(1, errors.InLastHour());
		}

		[Test]
		public void TestInLastHour_OlderErrorAddedAfterNewer_ReturnsOne()
		{
			var errors = new AdvertErrors();

			errors.Add(DateTime.Now.AddHours(-1).AddMinutes(1));
			errors.Add(DateTime.Now.AddHours(-1).AddMinutes(-1));
			Assert.AreEqual(1, errors.InLastHour());
		}

		[Test]
		public async Task TestInLastHour_ErrorGetsTooOld_ReturnsZero()
		{
			var errors = new AdvertErrors();

			errors.Add(DateTime.Now.AddHours(-1).AddMilliseconds(100));
			Assert.AreEqual(1, errors.InLastHour());
			await Task.Delay(150);
			Assert.AreEqual(0, errors.InLastHour());
		}

		[Test]
		public async Task TestInLastHour_OnlyOneHasNotExpired_ReturnsOne()
		{
			var errors = new AdvertErrors();

			errors.Add(DateTime.Now.AddHours(-1).AddMilliseconds(50));
			errors.Add(DateTime.Now.AddHours(-1).AddMilliseconds(100));
			errors.Add(DateTime.Now.AddHours(-1).AddMilliseconds(300));
			Assert.AreEqual(3, errors.InLastHour());
			await Task.Delay(150);
			Assert.AreEqual(1, errors.InLastHour());
		}

		[Test]
		public void TestAdd_AfterDispose_ThrowsException()
		{
			var errors = new AdvertErrors();

			errors.Dispose();
			Assert.Throws<ObjectDisposedException>(() => errors.Add(DateTime.Now));
		}

		[Test]
		public void TestInLastHour_AfterDispose_ThrowsException()
		{
			var errors = new AdvertErrors();

			errors.Dispose();
			Assert.Throws<ObjectDisposedException>(() => errors.InLastHour());
		}

		[Test]
		public void TestLocking_RandomAddAndCount_NoExceptions()
		{
			var generator = new Random();
			var errors = new AdvertErrors();
			int completedThreads = 0;

			for (int i = 0; i < 100; ++i)
			{
				ThreadPool.QueueUserWorkItem(
					(_) =>
					{
						Thread.Sleep(generator.Next(10));

						if (generator.Next(100) < 50)
						{
							int offsetSeconds = -generator.Next(1000);

							errors.Add(DateTime.Now.AddSeconds(offsetSeconds));
							Interlocked.Increment(ref completedThreads);
							return;
						}

						errors.InLastHour();
						Interlocked.Increment(ref completedThreads);
					});
			}

			while (completedThreads < 100)
			{
				Thread.Sleep(50);
			}
		}
	}
}