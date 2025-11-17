using System.ComponentModel.DataAnnotations;

namespace censudex_api_gateway.src.Dtos.Product;

public class CreationProduct
{
    [Required]
    public string Name { get; set; }
        
    [Required]
    public string Category { get; set; }
    
    [Required]
    public string Description { get; set; }
        
    [Required]
    public int Price { get; set; }
    
}