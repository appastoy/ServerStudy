namespace Neti
{
	class SessionIdGenerator
	{
		int currentId;

		public int Generate()
		{
			// TODO : 나중에 ID 발급 변경
			currentId++;

			return currentId;
		}
	}
}
