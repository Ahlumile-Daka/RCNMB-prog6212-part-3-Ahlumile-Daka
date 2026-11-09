using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels;

public class CategoryFormVm
{
    public int? CategoryID { get; set; }
    public int EventID { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Category Name")]
    public string CategoryName { get; set; } = string.Empty;

    [Display(Name = "Min Age")]
    public int? MinAge { get; set; }

    [Display(Name = "Max Age")]
    public int? MaxAge { get; set; }

    [Required, Range(0, 10000)]
    [Display(Name = "Entry Fee")]
    public decimal EntryFee { get; set; }

    [Display(Name = "Max Entries")]
    public int? MaxEntries { get; set; }
}
