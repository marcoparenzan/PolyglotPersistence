namespace WebModel
{
    using SqlDataModel;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class ProductExtension
    {
        public static IEnumerable<ProductDetailDTO> ToDTO(this IQueryable<Product> that)
        {
            var mapped = that.Select(xx => new
            {
                rowguid = xx.rowguid
                ,
                Name = xx.Name
                // ...
                ,
                ModifiedDate = xx.ModifiedDate
                ,
                Color = xx.Color
                ,
                DiscontinuedDate = xx.DiscontinuedDate
                ,
                ListPrice = xx.ListPrice
                ,
                ProductNumber = xx.ProductNumber
                ,
                Weight = xx.Weight
                ,
                SellEndDate = xx.SellEndDate
                ,
                SellStartDate = xx.SellStartDate
                ,
                Size = xx.Size
                ,
                StandardCost = xx.StandardCost
                ,
                ThumbnailPhotoFileName = xx.ThumbnailPhotoFileName
                ,
                ProductModel = new
                {
                    rowguid = xx.ProductModel.rowguid
                    ,
                    Name = xx.ProductModel.Name
                    ,
                    CatalogDescription = xx.ProductModel.CatalogDescription
                }
                ,
                ProductCategory = new
                {
                    rowguid = xx.ProductCategory.rowguid
                    ,
                    Name = xx.ProductCategory.Name
                }
            }).ToList();

            var materialized = mapped.Select(xx => new ProductDetailDTO
            {
                id = xx.rowguid
                ,
                Name = xx.Name
                // ...
                ,
                ModifiedDate = xx.ModifiedDate
                ,
                Color = xx.Color
                ,
                DiscontinuedDate = xx.DiscontinuedDate
                ,
                ListPrice = xx.ListPrice
                ,
                ProductNumber = xx.ProductNumber
                ,
                Weight = xx.Weight
                ,
                SellEndDate = xx.SellEndDate
                ,
                SellStartDate = xx.SellStartDate
                ,
                Size = xx.Size
                ,
                StandardCost = xx.StandardCost
                ,
                ThumbnailPhotoFileName = xx.ThumbnailPhotoFileName
                ,
                ProductModel = new ProductModelDTO
                {
                    Id = xx.ProductModel.rowguid
                    ,
                    Name = xx.ProductModel.Name
                    ,
                    CatalogDescription = xx.ProductModel.CatalogDescription
                }
                ,
                ProductCategory = new ProductCategoryDTO
                {
                    Id = xx.ProductCategory.rowguid
                    ,
                    Name = xx.ProductCategory.Name
                }
            });

            return materialized;
        }
    }
}
