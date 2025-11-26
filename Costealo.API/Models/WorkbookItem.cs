using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Costealo.API.Models;

public class WorkbookItem
{
    [Key]
    public int Id { get; set; }

    public int WorkbookId { get; set; }
    
    [ForeignKey("WorkbookId")]
    public Workbook Workbook { get; set; } = null!;

    public int PriceItemId { get; set; }
    
    [ForeignKey("PriceItemId")]
    public PriceItem PriceItem { get; set; } = null!;

    // User Inputs
    [Column(TypeName = "decimal(18,4)")]
    public decimal Quantity { get; set; } // Quantity needed for the recipe

    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty; // Unit used in recipe

    [Column(TypeName = "decimal(18,2)")]
    public decimal AdditionalCost { get; set; } // Manual extra cost
}
