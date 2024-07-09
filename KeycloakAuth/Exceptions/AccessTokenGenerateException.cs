namespace KeycloakAuth.Exceptions
{
    
        public class AccessTokenGenerateException : Exception
        {
            public AccessTokenGenerateException()
            {
            }

            public AccessTokenGenerateException(string message)
                : base(message)
            {
            }

            public AccessTokenGenerateException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }
    
}
