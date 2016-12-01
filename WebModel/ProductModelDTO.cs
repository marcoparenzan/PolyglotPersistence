namespace WebModel
{
    using System;

    public partial class ProductModelDTO:WebModelDTO<ProductModelDTO>
    {
        public string Name { get; set; }

        public string CatalogDescription { get; set; }

        public Guid Id { get; set; }
    }
}
