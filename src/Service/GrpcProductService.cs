using censudex_api_gateway.src.Dtos.Product;
using ImageProto;
using ProductProto;
using CreationProduct = censudex_api_gateway.src.Dtos.Product.CreationProduct;
using EditProduct = ProductProto.EditProduct;
using Empty = ProductProto.Empty;

namespace censudex_api_gateway.Service;

public class GrpcProductService(ProductService.ProductServiceClient productClient,
    ImageService.ImageServiceClient imageClient) : IProductService
{

    private readonly Empty _empty = new Empty();
    
    public async Task<ResponseProduct?> Create(CreationProduct creationProduct, IFormFile image)
    {
        byte[] imageBytes;
        using (var memoryStream = new MemoryStream())
        {
            await image.CopyToAsync(memoryStream);
            imageBytes = memoryStream.ToArray();
        }

        var uploadedImage = await imageClient.UploadAsync(new UploadImage
        {
            Image = Google.Protobuf.ByteString.CopyFrom(imageBytes)
        });

        var productCreated = await productClient.StoreAsync(
            new ProductProto.CreationProduct()
            {
                Name = creationProduct.Name,
                Category = creationProduct.Category,
                Description = creationProduct.Description,
                ImageId = uploadedImage.Id,
                Url = uploadedImage.Url,
                Price = creationProduct.Price
            }
        );


        return new ResponseProduct
        {
            Name = productCreated.Name,
            Category = productCreated.Category,
            Date = productCreated.Date,
            Price = productCreated.Price,
            Url = productCreated.Url
        };
    }

    public async Task<ResponseProduct?> Delete(string uuid)
    {

        var deletedProduct = await productClient.DeleteAsync(new ProductRequest
        {
            Id = uuid
        });

        if (deletedProduct == null)
        {
            return null;
        }

        return new ResponseProduct
        {
            Name = deletedProduct.Name,
            Category = deletedProduct.Category,
            Date = deletedProduct.Date,
            Price = deletedProduct.Price,
            Url = deletedProduct.Url
        };
    }

    
    
    public async Task<ResponseProduct?> Edit(string uuid, src.Dtos.Product.EditProduct editProduct,
        IFormFile? image)
    {

        var imageId = "";
        var url = "";
        
        if (image != null)
        {
            byte[] imageBytes;
            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
            }

            var uploadedImage = await imageClient.UploadAsync(new UploadImage
            {
                Image = Google.Protobuf.ByteString.CopyFrom(imageBytes)
            });
            
            imageId = uploadedImage.Id;
            url = uploadedImage.Url;
        }

        
        var editedProduct = await productClient.EditAsync(new EditProduct
        {
            Id   = uuid,
            Name = editProduct.Name,
            Description = editProduct.Description,
            Price = editProduct.Price,
            Category = editProduct.Category,
            ImageId = imageId,
            Url = url
        });

        if (editedProduct == null)
        {
            return null;
        }
        
        return new ResponseProduct
        {
            Name = editedProduct.Name,
            Category = editedProduct.Category,
            Date = editedProduct.Date,
            Price = editedProduct.Price,
            Url = editedProduct.Url
        };
    }

    public async Task<ResponseProduct?> Get(string uuid)
    {
        
        
        
        var product = await productClient.GetAsync(new ProductRequest
        {
            Id = uuid
        });

        if (product == null)
        {
            return null;
        }
        
        return new ResponseProduct
        {
            Name = product.Name,
            Category = product.Category,
            Date = product.Date,
            Price = product.Price,
            Url = product.Url
        };
    }

    public async Task<ProductResponseList> All()
    {
        return await productClient.AllAsync(_empty);
    }
    
}