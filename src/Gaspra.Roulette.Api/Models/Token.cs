namespace Gaspra.Roulette.Api.Models
{
    public class Token
    {
        public int Reference { get; }

        public Token(int reference)
        {
            Reference = reference;
        }
    }
}