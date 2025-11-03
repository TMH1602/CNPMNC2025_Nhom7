namespace WebApplication1.Services
{
    public interface IVnPayService2
    {
        string CreatePaymentUrl2(int orderId, decimal amount, string orderInfo, HttpContext context);
        bool ValidateVnPayHash(IQueryCollection collections);
    }
}
