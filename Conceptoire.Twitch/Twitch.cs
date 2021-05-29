namespace Conceptoire.Twitch
{
    public static class Twitch
    {
        public static Authentication.IAuthenticationBuilder Authenticate()
        {
            return new Authentication.AuthenticationBuilder();
        }

        public static Authentication.IBotAuthenticationBuilder AuthenticateBot()
        {
            return new Authentication.AuthenticationBuilder();
        }
    }
}
