using Bookstore.Domain.Common;
using Bookstore.Domain.Models;
using Bookstore.Domain.Specifications;
using Bookstore.Migrations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bookstore.Pages;

public class BookDetailsModel : PageModel
{
    private readonly Discounts _discounts;

    public record PriceLine(string Label, Money Amount);

    private readonly ILogger<IndexModel> _logger;
    private readonly BookstoreDbContext _dbContext;
    public Book Book { get; private set; } = null!;

    public IReadOnlyList<PriceLine> PriceSpecification { get; private set; } = Array.Empty<PriceLine>();



    public BookDetailsModel(ILogger<IndexModel> logger, BookstoreDbContext dbContext, Discounts discounts)
    {
        _discounts = discounts;
        (_logger, _dbContext) = (logger, dbContext);
    }

    public async Task<IActionResult> OnGet(Guid id)
    {
        if ((await _dbContext.Books.GetBooks().ById(id)) is Book book)
        {
            this.Book = book;

            var originalPrice = BookPricing.SeedPriceFor(book, Currency.USD).Value;

            if (_discounts.RelativeDiscount > 0 && _discounts.RelativeDiscount < 1)
                this.PriceSpecification = CalculatePriceSpecificationWithDiscount(originalPrice, _discounts);
            else
                this.PriceSpecification = CalculatePriceSpecificationWithoutDiscount(originalPrice);
            
            return Page();
        }

        return Redirect("/books");
    }

    private IReadOnlyList<PriceLine> CalculatePriceSpecificationWithDiscount(Money originalPrice, Discounts discounts)
    {
        var priceLines = new List<PriceLine>();
        priceLines.Add(new( "Original Price", originalPrice));

        var discount = new Money(originalPrice.Amount * discounts.RelativeDiscount, originalPrice.Currency);
        priceLines.Add(new( "Discount", discount));

        var total = new Money(originalPrice.Amount - discount.Amount, originalPrice.Currency);
        priceLines.Add(new( "TOTAL", total));

        return priceLines;

    }

    private IReadOnlyList<PriceLine> CalculatePriceSpecificationWithoutDiscount(Money originalPrice)
    {
        return new List<PriceLine>() { new("Price", originalPrice) };
    }
}
