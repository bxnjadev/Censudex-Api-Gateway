using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using order_service;
using censudex_api_gateway.src.Extensions;
using Microsoft.AspNetCore.Authorization;

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
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest dto)
        {
            try
            {
                var response = await _ordersClient.CreateOrderAsync(dto);
                return Created($"/orders/{response.Id}", response);
            }
            catch (RpcException ex)
            {
                return GrpcErrors.GrpcErrortoHttp(ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var response = await _ordersClient.GetAllOrdersAsync(new order_service.Empty());
                return Ok(response);
            }
            catch (RpcException ex)
            {
                return GrpcErrors.GrpcErrortoHttp(ex);
            }
        }

        [HttpGet("{id}")]
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
