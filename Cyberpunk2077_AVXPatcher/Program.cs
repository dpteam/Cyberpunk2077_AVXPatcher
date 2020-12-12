using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Cyberpunk2077_AVXPatcher
{
	public static class Program
	{
		public static int Main(string[] args)
		{
			string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string cp77exe = "Cyberpunk2077.exe";
			File.Copy(assemblyPath + Path.DirectorySeparatorChar + cp77exe, assemblyPath + Path.DirectorySeparatorChar + cp77exe + ".bak");
			Console.WriteLine("Backup created");
			string findStr = "55 48 81 EC A0 00 00 00 0F 29 70 E8";
			string replaceStr = "C3 48 81 EC A0 00 00 00 0F 29 70 E8";
			byte[] find = ConvertHexStringToByteArray(Regex.Replace(findStr, "[ ,]", string.Empty).Normalize().Trim());
			byte[] replace = ConvertHexStringToByteArray(Regex.Replace(replaceStr, "[ ,]", string.Empty).Normalize().Trim());
			byte[] bytes = File.ReadAllBytes(cp77exe);
			Console.WriteLine("All bytes readed");
			using (IEnumerator<int> enumerator = PatternAt(bytes, find).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					int index = enumerator.Current;
					int i = index;
					int replaceIndex = 0;
					while (i < bytes.Length && replaceIndex < replace.Length)
					{
						bytes[i] = replace[replaceIndex];
						i++;
						replaceIndex++;
					}
					File.WriteAllBytes(cp77exe, bytes);
					Console.WriteLine("AVX Pattern found at offset {0} and replaced. Cyberpunk 2077 patched successful.", index);
					return 0;
				}
			}
			Console.WriteLine("Pattern not found");
			return -1;
		}

		private static IEnumerable<int> PatternAt(byte[] source, byte[] pattern)
		{
			for (int i = 0; i < source.Length; i++)
			{
				if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
				{
					yield return i;
				}
			}
		}

		private static string ConvertByteToHex(byte[] byteData)
		{
			string hexValues = BitConverter.ToString(byteData).Replace("-", "");
			return hexValues;
		}

		private static byte[] ConvertHexStringToByteArray(string hexString)
		{
			if (hexString.Length % 2 != 0)
			{
				throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
			}

			byte[] data = new byte[hexString.Length / 2];

			for (int index = 0; index < data.Length; index++)
			{
				string byteValue = hexString.Substring(index * 2, 2);
				data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			}

			return data;
		}
	}
}
