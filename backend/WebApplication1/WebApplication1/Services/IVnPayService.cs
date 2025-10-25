namespace WebApplication1.Services
{
    public interface IVnPayService
    {
        string CreateTokenizationUrl(int userId, HttpContext context);

        string CreatePaymentTokenUrl(int orderId, decimal amount, string token, HttpContext context);

        string CreateRemoveTokenUrl(string token, int userId, HttpContext context);

        bool ValidateVnPayHash(IQueryCollection collections);
    }
}
