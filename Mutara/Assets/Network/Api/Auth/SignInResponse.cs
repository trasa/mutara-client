using System;

namespace Mutara.Network.Api.Auth
{
    [Serializable]
    public class SignInResponse
    {
        public string SessionId { get; set; }
        public string AccessToken { get; set; }
        public string IdToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }

        public override string ToString()
        {
            return $"[SignInResponse SessionId:{SessionId} AccessToken:{AccessToken} IdToken:{IdToken}]";
        }
    }
    
}