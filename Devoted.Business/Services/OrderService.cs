using Devoted.Business.Error;
using Devoted.Business.Interfaces;
using Devoted.Domain.Sql.Dto;
using Devoted.Domain.Sql.Entity;
using Devoted.Domain.Sql.Request.Order;
using Devoted.Domain.Sql.Response.Base;
using Devoted.Persistence.Sql.Interfaces;
using Microsoft.Extensions.Logging;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repo;
    private readonly IAppUnitOfWork _uow;
    private readonly IProductClient _products;
    private readonly ILogger<OrderService> _log;

    public OrderService(IAppUnitOfWork uow,
                        IOrderRepository repo,
                        IProductClient products,
                        ILogger<OrderService> log)
    {
        _uow = uow;
        _repo = repo;
        _products = products;
        _log = log;
    }

    public async Task<BaseResponse> CreateOrderAsync(CreateOrderRequest req, CancellationToken ct)
    {

        if (req.Quantity <= 0)
            throw new UserError("Quantity must be at least 1");

        var product = await _products.GetByIdAsync(req.ProductId, ct);

        var total = product.Price * req.Quantity;

        var order = new Orders
        {
            ProductId = req.ProductId,
            Quantity = req.Quantity,
            Total = total
        };

        await _repo.CreateAsync(order);
        await _uow.SaveChangesAsync();

        _log.LogInformation(
          "Order {OrderId} created: Product={ProductId}, Qty={Qty}, Total={Total}",
           order.Id, order.ProductId, order.Quantity, order.Total);

        return new BaseResponse
        {
            Message = "Order created",
            Data = new OrderDto(order.Id, order.ProductId, order.Quantity, order.Total)
        };
    }
}
