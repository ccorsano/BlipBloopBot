namespace Conceptoire.Twitch
{
    public static class Twitch
    {
        public static Authentication.IAuthenticationBuilder Authenticate()
        {
            return new Authentication.AuthenticationBuilder();
        }
    }
}
