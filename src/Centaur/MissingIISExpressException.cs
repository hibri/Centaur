using System;

namespace Centaur
{
	public class MissingIISExpressException : Exception
	{
		public MissingIISExpressException(string iisExpressPath) : base("IIS Express was not found at " + iisExpressPath)
		{
		}
	}
}