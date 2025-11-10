using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace censudex_api_gateway.src.Extensions
{
    /// <summary>
    /// Extension methods for handling gRPC errors.
    /// </summary>
    public static class GrpcErrors
    {
        /// <summary>
        /// Converts a gRPC RpcException to an appropriate HTTP response.
        /// </summary>
        /// <param name="grpcException">The gRPC RpcException to convert.</param>
        /// <returns>An IActionResult representing the HTTP response.</returns>
        public static IActionResult GrpcErrortoHttp(RpcException grpcException)
        {
            var statusCode = grpcException.StatusCode switch
            {
                StatusCode.OK => 200,
                StatusCode.InvalidArgument => 400,
                StatusCode.NotFound => 404,
                StatusCode.AlreadyExists => 409,
                StatusCode.PermissionDenied => 403,
                StatusCode.Unauthenticated => 401,
                StatusCode.FailedPrecondition => 412,
                StatusCode.Internal => 500,
                StatusCode.Unimplemented => 501,
                StatusCode.Unavailable => 503,
                _ => 500,
            };

            return new ObjectResult(new
            {
                Error = grpcException.Status.Detail,
                Code = grpcException.StatusCode.ToString()
            })
            {
                StatusCode = statusCode
            };
        }   
    }
}