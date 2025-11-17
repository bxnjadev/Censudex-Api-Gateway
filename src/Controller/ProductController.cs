using censudex_api_gateway.Service;
using censudex_api_gateway.src.Dtos.Product;
using Grpc.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace censudex_api_gateway.src.Controller;

[ApiController]
[Route("api/[controller]")]
public class ProductController(IProductService productService) : ControllerBase
{
    [HttpPost]
    [Route("/api/products/")]
    public async Task<IActionResult> Create(
        [FromForm] CreationProduct creationProduct,
        [FromForm] IFormFile image
    )
    {

        try
        {
            var createdProduct = await productService.Create(creationProduct, image);
            return Ok(createdProduct);    
        } catch(RpcException _)
        { }

        return BadRequest("Ese nombre ya esta tomado");
    }


    [HttpGet]
    [Route("/api/products/{uuid}")]
    public async Task<IActionResult> Get(string uuid)
    {

        try
        {
            new Guid(uuid);
        }
        catch (FormatException e)
        {
            return BadRequest("uuid bad format");
        }

        try
        {
            var product = await productService.Get(uuid);
            return Ok(product);
        }
        catch (RpcException _)
        {
        }
        return NotFound("product not found");
    }

    [HttpDelete]
    [Route("/api/products/{uuid}")]
    public async Task<IActionResult> Delete(string uuid)
    {
        try
        {
            new Guid(uuid);
        }
        catch (FormatException e)
        {
            return BadRequest("uuid bad format");
        }


        try
        {
            var productDeleted = await productService.Delete(uuid);
            return Ok(productDeleted);
        }
        catch (RpcException _)
        {
        }

        return NotFound("product not found");
    }

    [HttpPatch]
    [Route("/api/products/{uuid}")]
    public async Task<IActionResult> Edit(string uuid, 
        [FromForm] EditProduct editProduct,
        [FromForm] IFormFile? image)
    {
        try
        {
            new Guid(uuid);
        }
        catch (FormatException _)
        {
            return BadRequest("uuid bad format");
        }
        
        try
        {
            var productEdited = await productService.Edit(uuid, editProduct, image);
            return Ok(productEdited);
        }
        catch (RpcException e)
        {
            if (e.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
            {
                return BadRequest("Ese nombre ya esta tomado");
            }

            return NotFound("El producto no existe");
        }

        
    }

    [HttpGet]
    [Route("/api/products/all")]
    public async Task<IActionResult> All()
    {
        return Ok(await productService.All());
    }
}