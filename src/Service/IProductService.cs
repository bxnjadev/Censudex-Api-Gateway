using censudex_api_gateway.src.Dtos.Product;
using ProductProto;
using CreationProduct = censudex_api_gateway.src.Dtos.Product.CreationProduct;
using EditProduct = censudex_api_gateway.src.Dtos.Product.EditProduct;

namespace censudex_api_gateway.Service;

public interface IProductService
{

    Task<ResponseProduct?> Create(CreationProduct creationProduct,
       IFormFile image);

    Task<ResponseProduct?> Delete(string uuid);

    Task<ResponseProduct?> Edit(string uuid,
        EditProduct editProduct);

    Task<ResponseProduct?> Get(string uuid);

    Task<ProductResponseList> All();

}