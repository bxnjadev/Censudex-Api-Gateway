using censudex_api_gateway.Service;
using censudex_api_gateway.src.Dtos.Product;
using Microsoft.AspNetCore.Mvc;

namespace censudex_api_gateway.src.Controller;

[ApiController]
[Route("api/[controller]")]
public class ProductController(IProductService productService) : ControllerBase
{
    
    [HttpPost]
    [Route("/api/products/create")]
    public async Task<IActionResult> Create(
        [FromForm] CreationProduct creationProduct,
        [FromForm] IFormFile image
    )
    {
        var createdProduct = await productService.
            Create(creationProduct, image);
        return Ok(createdProduct);
    }

 

    [HttpGet]
    [Route("/api/products/{uuid}")]
    public async Task<IActionResult> Get(string uuid)
    {   
        var product = await productService.Get(uuid);
        if (product == null)
        {
            return NotFound("Product not found");
        }

        return Ok(product);
    }

    [HttpDelete]
    [Route("/api/products/{uuid}")]
    public async Task<IActionResult> Delete(string uuid)
    {
        var productDeleted = await productService.Delete(uuid);
        if (productDeleted == null)
        {
            return NotFound("Product not found");
        }

        return Ok(productDeleted);
    }

    [HttpPatch]
    [Route("/api/products/{uuid}")]
    public async Task<IActionResult> Edit(string uuid, EditProduct editProduct)
    {
        var productEdited = await productService.Edit(uuid, editProduct);
        if (productEdited == null)
        {
            return NotFound("Product not found");
        }

        return Ok(productEdited);
    }

    [HttpGet]
    [Route("/api/products/all")]
    public async Task<IActionResult> All()
    {
        return Ok(await productService.All());
    }
    
}