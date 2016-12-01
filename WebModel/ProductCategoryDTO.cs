using System;

namespace WebModel
{
    public partial class ProductCategoryDTO: WebModelDTO<ProductCategoryDTO>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
