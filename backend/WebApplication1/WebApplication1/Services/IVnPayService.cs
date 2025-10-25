namespace WebApplication1.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(int orderId, decimal amount, string orderInfo, HttpContext context);
        bool ValidateVnPayHash(IQueryCollection collections);
    }
}
