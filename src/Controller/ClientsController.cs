using System;
using System.Collections.Generic;
using System.Linq;
using UserProto;
using Grpc.Core;
using censudex_api_gateway.src.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace censudex_api_gateway.src.Controller
{
    /// <summary>
    /// Controller for managing clients.
    /// </summary>
    [ApiController]
    [Route("clients")]
    public class ClientsController : ControllerBase
    {
        /// <summary>
        /// Client service client for gRPC communication.
        /// </summary>
        private readonly UserService.UserServiceClient _clientServiceClient;
        public ClientsController(UserService.UserServiceClient clientServiceClient)
        {
            _clientServiceClient = clientServiceClient;
        }

        /// <summary>
        /// Creates a new client.
        /// </summary>
        /// <param name="request">The client creation request.</param>
        /// <returns>The created client.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] CreateUserRequest request)
        {
            try
            {
                /// <summary>
                /// Creates a new client request for gRPC.
                /// </summary>
                /// <value></value>
                var requestGrpc = new CreateUserRequest
                {
                    Name = request.Name,
                    Lastnames = request.Lastnames,
                    Email = request.Email,
                    Username = request.Username,
                    Birthdate = request.Birthdate,
                    Address = request.Address,
                    Phone = request.Phone,
                    Password = request.Password
                };
                var response = await _clientServiceClient.CreateUserAsync(requestGrpc);
                return Ok(response);
            }
            catch (RpcException grpcException)
            {
                return GrpcErrors.GrpcErrortoHttp(grpcException);
            }
        }
        /// <summary>
        /// Retrieves a list of clients with optional filters.
        /// </summary>
        /// <param name="namefilter">Filter by name.</param>
        /// <param name="emailfilter">Filter by email.</param>
        /// <param name="isactivefilter">Filter by active status.</param>
        /// <param name="usernamefilter">Filter by username.</param>
        [HttpGet]
        public async Task<IActionResult> GetClients(
            [FromQuery(Name = "namefilter")] string namefilter = null,
            [FromQuery(Name = "emailfilter")] string emailfilter = null,
            [FromQuery(Name = "isactivefilter")] string isactivefilter = null,
            [FromQuery(Name = "usernamefilter")] string usernamefilter = null)
        {
            try
            {
                var request = new GetUserRequest();
                if (!string.IsNullOrWhiteSpace(namefilter)) request.Namefilter = namefilter;
                if (!string.IsNullOrWhiteSpace(emailfilter)) request.Emailfilter = emailfilter;
                if (!string.IsNullOrWhiteSpace(isactivefilter)) request.Isactivefilter = isactivefilter;
                if (!string.IsNullOrWhiteSpace(usernamefilter)) request.Usernamefilter = usernamefilter;

                var response = await _clientServiceClient.GetUserAsync(request);
                return Ok(response);
            }
            catch (RpcException grpcException)
            {
                return GrpcErrors.GrpcErrortoHttp(grpcException);
            }
        }
        /// <summary>
        /// Retrieves a client by ID.
        /// </summary>
        /// <param name="id">The ID of the client.</param>
        /// <returns>The client with the specified ID.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClientById([FromRoute] string id)
        {
            try
            {
                var request = new GetUserIdRequest { Id = id };
                var response = await _clientServiceClient.GetUserByIdAsync(request);
                return Ok(response);
            }
            catch (RpcException grpcException)
            {
                return GrpcErrors.GrpcErrortoHttp(grpcException);
            }
        }
        /// <summary>
        /// Updates an existing client.
        /// </summary>
        /// <param name="id">The ID of the client to update.</param>
        /// <param name="request">The client update request.</param>
        /// <returns>The updated client.</returns>
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateClient([FromRoute] string id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var requestGrpc = new UpdateUserRequest
                {
                    Id = id,
                    Name = request.Name,
                    Lastnames = request.Lastnames,
                    Email = request.Email,
                    Username = request.Username,
                    Birthdate = request.Birthdate,
                    Address = request.Address,
                    Phone = request.Phone,
                    Password = request.Password
                };
                var response = await _clientServiceClient.UpdateUserAsync(requestGrpc);
                return Ok(response);
            }
            catch (RpcException grpcException)
            {
                return GrpcErrors.GrpcErrortoHttp(grpcException);
            }
        }
        /// <summary>
        /// Deletes a client by ID.
        /// </summary>
        /// <param name="id">The ID of the client to delete.</param>
        /// <returns>The deletion response.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient([FromRoute] string id)
        {
            try
            {
                var request = new DeleteUserRequest { Id = id };
                var response = await _clientServiceClient.DeleteUserAsync(request);
                return Ok(response);
            }
            catch (RpcException grpcException)
            {
                return GrpcErrors.GrpcErrortoHttp(grpcException);
            }
        }
    }
}