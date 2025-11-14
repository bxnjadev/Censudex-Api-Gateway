using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using order_service;
using censudex_api_gateway.src.Extensions;
using Microsoft.AspNetCore.Authorization;
using censudex_api_gateway.src.Dtos.Orders;

namespace censudex_api_gateway.src.Controller
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly OrderGrpcService.OrderGrpcServiceClient _ordersClient;

        public OrderController(OrderGrpcService.OrderGrpcServiceClient ordersClient)
        {
            _ordersClient = ordersClient;
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto body)
        {
            try
            {
                var grpcRequest = new CreateOrderRequest
                {
                    CustomerId = body.CustomerId,
                    CustomerName = body.CustomerName,
                    CustomerEmail = body.CustomerEmail
                };

                grpcRequest.Items.AddRange(
                    body.Items.Select(i => new OrderItemDto
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    })
                );

                var response = await _ordersClient.CreateOrderAsync(grpcRequest);

                return Created($"/orders/{response.Id}", response);
            }
            catch (RpcException ex)
            {
                return GrpcErrors.GrpcErrortoHttp(ex);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllOrders([FromQuery] string? customerId, [FromQuery] string? from, [FromQuery] string? to)
        {
            try
            {
                var response = await _ordersClient.GetAllOrdersAsync(new order_service.Empty());

                var orders = response.Orders.AsQueryable();

                // FILTRO: por cliente
                if (!string.IsNullOrWhiteSpace(customerId))
                {
                    orders = orders.Where(o => o.CustomerId == customerId);
                }

                // FILTRO: desde fecha (from)
                if (!string.IsNullOrWhiteSpace(from))
                {
                    if (DateTime.TryParse(from, out var fromDate))
                    {
                        orders = orders.Where(o => DateTime.Parse(o.OrderDate) >= fromDate);
                    }
                }

                // FILTRO: hasta fecha (to)
                if (!string.IsNullOrWhiteSpace(to))
                {
                    if (DateTime.TryParse(to, out var toDate))
                    {
                        orders = orders.Where(o => DateTime.Parse(o.OrderDate) <= toDate);
                    }
                }

                var filtered = new OrdersListResponse();
                filtered.Orders.AddRange(orders);

                return Ok(filtered);
            }
            catch (RpcException ex)
            {
                return GrpcErrors.GrpcErrortoHttp(ex);
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrderById(string id)
        {
            try
            {
                var response = await _ordersClient.GetOrderByIdAsync(new OrderByIdRequest { Id = id });

                if (response == null || string.IsNullOrWhiteSpace(response.Id))
                    return NotFound($"La orden con ID '{id}' no existe.");

                return Ok(response);
            }
            catch (RpcException ex)
            {
                return GrpcErrors.GrpcErrortoHttp(ex);
            }
        }

        [HttpPut("{id}/status")]
        [Authorize (Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateStatusRequest dto)
        {
            try
            {
                dto.Id = id;
                var result = await _ordersClient.UpdateOrderStatusAsync(dto);

                if (!result.Success)
                    return BadRequest(result.Message);

                return Ok(result);
            }
            catch (RpcException ex)
            {
                return GrpcErrors.GrpcErrortoHttp(ex);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> CancelOrder(string id)
        {
            try
            {
                var result = await _ordersClient.CancelOrderAsync(new OrderByIdRequest { Id = id });

                if (!result.Success)
                    return BadRequest(result.Message);

                return Ok(result);
            }
            catch (RpcException ex)
            {
                return GrpcErrors.GrpcErrortoHttp(ex);
            }
        }
    }
}
