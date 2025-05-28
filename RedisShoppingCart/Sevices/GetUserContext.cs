namespace Server.API.Sevices
{
    public class GetUserContext
    {
        private readonly IHttpContextAccessor _context;
        public GetUserContext(IHttpContextAccessor context)
        {
            _context = context;
        }
        public string getSession()
        {       
            _context.HttpContext!.Items.TryGetValue("uid", out var sessionId);
            var sessionValue = sessionId?.ToString();
            return sessionValue??default!;         
        }
        public string getCartKey()
        {        
           return $"cart:user_{getSession()}";
        }
       
       
    }
}
