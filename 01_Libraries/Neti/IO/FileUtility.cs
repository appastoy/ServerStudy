using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Neti.IO
{
	public static class FileUtility
	{
		public static async Task WriteAllTextAsync(string path, string content, Encoding encoding = null)
		{
			var validEncoding = encoding ?? Encoding.Default;
			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096, true))
			{
				var bytes = validEncoding.GetBytes(content);
				await stream.WriteAsync(bytes, 0, bytes.Length);
			}
		}

		public static async Task<string> ReadAllTextAsync(string path, Encoding encoding = null)
		{
			var validEncoding = encoding ?? Encoding.Default;
			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, true))
			{
				var bytes = new byte[stream.Length];
				await stream.ReadAsync(bytes, 0, bytes.Length);

				return validEncoding.GetString(bytes);
			}
		}
	}
}
