using SimplySDK.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sage50API
{
	public class Utility
	{
		public static short AccountNumberLength { get; private set; }

		static Utility()
		{
			AccountNumberLength = 4;
		}

		public static void InitializeAfterDatabaseOpen()
		{
			try
			{
				AccountNumberLength = (short)(new SDKDatabaseUtility()).RunScalerQuery("SELECT nActNumLen FROM tCompOth");
			}
			catch { }
		}

		/// <summary>
		/// Generates Account Number according to <see cref="AccountNumberLength"/>.
		/// </summary>
		public static int MakeAccountNumber(int number)
		{
			return MakeAccountNumber(number, AccountNumberLength);
		}

		/// <summary>
		/// Generates Account Number according to <see cref="AccountNumberLength"/>.
		/// </summary>
		public static int MakeAccountNumber(int number, short maxLen)
		{
			string zeros = string.Empty;
			string num_str = (number <= 0) ? "1" : num_str = Convert.ToString(number);

			for (int i = num_str.Length; i < Math.Min((short)8, maxLen); i++)
				zeros = zeros + "0";

			return Convert.ToInt32(num_str + zeros);
		}

	}
}