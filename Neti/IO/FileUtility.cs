using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Neti.IO
{
	public static class FileUtility
	{
		public static async Task WriteAllTextAsync(string path, string content)
		{
			await WriteAllTextAsync(path, content, Encoding.Default);
		}

		public static async Task WriteAllTextAsync(string path, string content, Encoding encoding)
		{
			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096, true))
			{
				var bytes = encoding.GetBytes(content);
				await stream.WriteAsync(bytes, 0, bytes.Length);
			}
		}

		public static async Task<string> ReadAllTextAsync(string path)
		{
			return await ReadAllTextAsync(path, Encoding.Default);
		}

		public static async Task<string> ReadAllTextAsync(string path, Encoding encoding)
		{
			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, true))
			{
				var bytes = new byte[stream.Length];
				await stream.ReadAsync(bytes, 0, bytes.Length);

				return encoding.GetString(bytes);
			}
		}
	}
}
