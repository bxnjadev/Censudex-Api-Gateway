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
    /// <summary>
    /// Controlador responsable de exponer endpoints REST para la gestión de órdenes.
    /// Actúa como API Gateway y comunica estas operaciones con el microservicio OrderService vía gRPC.
    /// </summary>
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly OrderGrpcService.OrderGrpcServiceClient _ordersClient;

        /// <summary>
        /// Constructor del controlador de órdenes.
        /// </summary>
        /// <param name="ordersClient">Cliente gRPC del microservicio de órdenes.</param>
        public OrderController(OrderGrpcService.OrderGrpcServiceClient ordersClient)
        {
            _ordersClient = ordersClient;
        }

        /// <summary>
        /// Crea una nueva orden en el sistema.
        /// </summary>
        /// <remarks>
        /// Este endpoint recibe un cuerpo JSON (REST), lo convierte en un mensaje gRPC
        /// y lo envía al microservicio OrderService para procesar la orden.
        /// Requiere autenticación.
        /// </remarks>
        /// <param name="body">Datos necesarios para crear la orden.</param>
        /// <returns>La orden creada con su ID, fecha, estado e ítems.</returns>
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

        /// <summary>
        /// Obtiene todas las órdenes, con la posibilidad de aplicar filtros opcionales.
        /// </summary>
        /// <remarks>
        /// Filtros disponibles:
        /// <br/><br/>
        /// • <b>customerId</b>: Filtra órdenes por ID de cliente.<br/>
        /// • <b>from</b>: Fecha mínima (incluye).<br/>
        /// • <b>to</b>: Fecha máxima (incluye).<br/><br/>
        /// Requiere autenticación.
        /// </remarks>
        /// <param name="customerId">ID del cliente para filtrar.</param>
        /// <param name="from">Fecha mínima en formato YYYY-MM-DD.</param>
        /// <param name="to">Fecha máxima en formato YYYY-MM-DD.</param>
        /// <returns>Lista filtrada de órdenes.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllOrders([FromQuery] string? customerId, [FromQuery] string? from, [FromQuery] string? to)
        {
            try
            {
                var response = await _ordersClient.GetAllOrdersAsync(new order_service.Empty());

                var orders = response.Orders.AsQueryable();

                
                if (!string.IsNullOrWhiteSpace(customerId))
                {
                    orders = orders.Where(o => o.CustomerId == customerId);
                }

                
                if (!string.IsNullOrWhiteSpace(from))
                {
                    if (DateTime.TryParse(from, out var fromDate))
                    {
                        orders = orders.Where(o => DateTime.Parse(o.OrderDate) >= fromDate);
                    }
                }

                
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

        /// <summary>
        /// Obtiene una orden específica mediante su ID único.
        /// </summary>
        /// <param name="id">ID de la orden.</param>
        /// <returns>La orden encontrada o un error si no existe.</returns>
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

        /// <summary>
        /// Actualiza el estado de una orden.
        /// </summary>
        /// <remarks>
        /// Solo administradores pueden ejecutar este endpoint.
        /// Los estados válidos son definidos por el OrderService.
        /// </remarks>
        /// <param name="id">ID de la orden a actualizar.</param>
        /// <param name="dto">Nuevo estado de la orden.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
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

        /// <summary>
        /// Cancela una orden existente.
        /// </summary>
        /// <remarks>
        /// El OrderService valida si la orden se puede cancelar o no según su estado.
        /// Requiere autenticación.
        /// </remarks>
        /// <param name="id">ID de la orden a cancelar.</param>
        /// <returns>Resultado de la cancelación.</returns>
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
