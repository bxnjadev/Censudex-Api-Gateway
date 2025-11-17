using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace censudex_api_gateway.src.Dtos.Orders
{
    /// <summary>
    /// DTO utilizado por la API Gateway para recibir solicitudes REST
    /// relacionadas a la creación de una nueva orden.
    /// Este modelo representa el cuerpo JSON enviado desde el cliente.
    /// </summary>
    public class CreateOrderDto
    {
        /// <summary>
        /// Identificador del cliente que realiza la orden.
        /// Debe ser un GUID válido representado como string.
        /// </summary>
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del cliente asociado a la orden.
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Correo del cliente. Usado para enviar notificaciones (e.g., SendGrid).
        /// </summary>
        public string CustomerEmail { get; set; } = string.Empty;

        /// <summary>
        /// Lista de ítems incluidos en la orden.
        /// Cada ítem representa un producto comprado por el cliente.
        /// </summary>
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Representa un producto que forma parte de una orden.
    /// Incluye su identificador, nombre, cantidad solicitada y precio unitario.
    /// </summary>
    public class CreateOrderItemDto
    {
        /// <summary>
        /// Identificador del producto comprado (representado como string GUID).
        /// </summary>
        public string ProductId { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del producto en el momento de la compra.
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Cantidad del producto comprada por el cliente.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Precio unitario del producto.
        /// </summary>
        public double UnitPrice { get; set; }
    }
}
