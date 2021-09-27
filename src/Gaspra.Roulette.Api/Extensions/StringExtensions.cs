using System.Linq;

namespace Gaspra.Roulette.Api.Extensions
{
    public static class StringExtensions
    {
        public static string Sanitise(this string dirty)
        {
            var dirtyCharacters = "?&^$#@!()+-,:;<>’\'-_*\"£";

            var sanitisedName = dirtyCharacters.Aggregate(dirty, (current, c) => current.Replace(c.ToString(), ""));

            if (sanitisedName.Length > 50)
            {
                sanitisedName = sanitisedName.Substring(0, 50);
            }

            return sanitisedName;
        }
    }
}
