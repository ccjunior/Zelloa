namespace Zelloa.Domain.Catalog;

public sealed class Product
{
    private const decimal MaximumPrice = 9_999_999_999_999_999.99m;

    private Product() { }

    public Product(
        Guid id,
        Guid tenantId,
        Guid categoryId,
        string name,
        string? description,
        decimal price,
        string? imageUrl,
        bool available)
    {
        if (id == Guid.Empty) throw new ArgumentException("Product ID cannot be empty.", nameof(id));
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        if (categoryId == Guid.Empty) throw new ArgumentException("Category ID cannot be empty.", nameof(categoryId));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ValidateText(name, description, imageUrl);
        ValidatePrice(price);

        Id = id;
        TenantId = tenantId;
        CategoryId = categoryId;
        Name = name.Trim();
        Description = NormalizeOptional(description);
        Price = price;
        ImageUrl = NormalizeOptional(imageUrl);
        IsActive = true;
        IsAvailable = available;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsAvailable { get; private set; }

    public void Update(string name, Guid categoryId, string? description, decimal price, string? imageUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (categoryId == Guid.Empty) throw new ArgumentException("Category ID cannot be empty.", nameof(categoryId));
        ValidateText(name, description, imageUrl);
        ValidatePrice(price);

        Name = name.Trim();
        CategoryId = categoryId;
        Description = NormalizeOptional(description);
        Price = price;
        ImageUrl = NormalizeOptional(imageUrl);
    }

    public void SetAvailability(bool available) => IsAvailable = available;

    public void SetStatus(bool active) => IsActive = active;

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0 || price > MaximumPrice || decimal.Round(price, 2) != price)
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be positive and have at most two decimal places.");
    }

    private static void ValidateText(string name, string? description, string? imageUrl)
    {
        if (name.Trim().Length > 200) throw new ArgumentOutOfRangeException(nameof(name));
        if (description?.Trim().Length > 2000) throw new ArgumentOutOfRangeException(nameof(description));
        if (imageUrl?.Trim().Length > 2048) throw new ArgumentOutOfRangeException(nameof(imageUrl));
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
