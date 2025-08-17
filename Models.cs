using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace KnjiznicaBlazor.Models
{
    public class Avtor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ime je obvezno")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ime mora imeti med 2 in 50 znakov")]
        [RegularExpression(@"^[a-žA-Ž\s]+$", ErrorMessage = "Ime sme vsebovati samo črke in presledke")]
        public string Ime { get; set; } = string.Empty;

        [Required(ErrorMessage = "Priimek je obvezen")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Priimek mora imeti med 2 in 50 znakov")]
        [RegularExpression(@"^[a-žA-Ž\s]+$", ErrorMessage = "Priimek sme vsebovati samo črke in presledke")]
        public string Priimek { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum rojstva je obvezen")]
        [DataType(DataType.Date)]
        [Range(typeof(DateTime), "1900-01-01", "2010-12-31", ErrorMessage = "Datum rojstva mora biti med letom 1900 in 2010")]
        public DateTime DatumRojstva { get; set; } = DateTime.Today.AddYears(-30);

        [Required(ErrorMessage = "Email je obvezen")]
        [EmailAddress(ErrorMessage = "Neveljaven email naslov")]
        [StringLength(100, ErrorMessage = "Email ne sme biti daljši od 100 znakov")]
        public string Email { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Biografija ne sme biti daljša od 1000 znakov")]
        public string? Biografija { get; set; }

        // Navigation properties za frontend
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Knjiga> Knjige { get; set; } = new();

        // Computed properties
        [JsonIgnore]
        public string PolnoIme => $"{Ime} {Priimek}".Trim();

        [JsonIgnore]
        public int Starost
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - DatumRojstva.Year;
                if (DatumRojstva.Date > today.AddYears(-age))
                    age--;
                return Math.Max(0, age);
            }
        }

        [JsonIgnore]
        public bool JeMlad => Starost < 40;

        [JsonIgnore]
        public bool JeProduktiven => Knjige?.Count > 0;
    }

    public class Kategorija
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ime kategorije je obvezno")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Ime kategorije mora imeti med 3 in 100 znakov")]
        [RegularExpression(@"^[a-žA-Ž0-9\s\-]+$", ErrorMessage = "Ime kategorije sme vsebovati samo črke, številke, presledke in vezaje")]
        public string Ime { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Opis ne sme biti daljši od 500 znakov")]
        public string? Opis { get; set; }

        // Navigation properties za frontend
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Knjiga> Knjige { get; set; } = new();

        // Computed properties
        [JsonIgnore]
        public int SteviloKnjig => Knjige?.Count ?? 0;

        [JsonIgnore]
        public bool JePriljubljena => SteviloKnjig >= 5;

        [JsonIgnore]
        public bool ImaOpis => !string.IsNullOrWhiteSpace(Opis);
    }

    public class Knjiga
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Naslov je obvezen")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Naslov mora imeti med 1 in 200 znakov")]
        public string Naslov { get; set; } = string.Empty;

        [Required(ErrorMessage = "ISBN je obvezen")]
        [StringLength(17, MinimumLength = 10, ErrorMessage = "ISBN mora imeti med 10 in 17 znakov")]
        [RegularExpression(@"^(?:ISBN(?:-1[03])?:? )?(?=[0-9X]{10}$|(?=(?:[0-9]+[- ]){3})[- 0-9X]{13}$|97[89][0-9]{10}$|(?=(?:[0-9]+[- ]){4})[- 0-9]{17}$)(?:97[89][- ]?)?[0-9]{1,5}[- ]?[0-9]+[- ]?[0-9]+[- ]?[0-9X]$",
            ErrorMessage = "ISBN format ni veljaven")]
        public string ISBN { get; set; } = string.Empty;

        [Required(ErrorMessage = "Datum izdaje je obvezen")]
        [DataType(DataType.Date)]
        [Range(typeof(DateTime), "1450-01-01", "2030-12-31", ErrorMessage = "Datum izdaje mora biti med letom 1450 in 2030")]
        public DateTime DatumIzdaje { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Avtor je obvezen")]
        [Range(1, int.MaxValue, ErrorMessage = "Izberite veljavnega avtorja")]
        public int AvtorId { get; set; }

        [Required(ErrorMessage = "Kategorija je obvezna")]
        [Range(1, int.MaxValue, ErrorMessage = "Izberite veljavno kategorijo")]
        public int KategorijaId { get; set; }

        // Navigation properties za frontend
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Avtor? Avtor { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Kategorija? Kategorija { get; set; }

        // Computed properties
        [JsonIgnore]
        public int StarostKnjige
        {
            get
            {
                var age = DateTime.Now.Year - DatumIzdaje.Year;
                return Math.Max(0, age);
            }
        }

        [JsonIgnore]
        public bool JeNova => StarostKnjige <= 1;

        [JsonIgnore]
        public bool JeKlasika => StarostKnjige >= 50;

        [JsonIgnore]
        public string StarostOpis
        {
            get
            {
                return StarostKnjige switch
                {
                    0 => "Nova knjiga",
                    1 => "1 leto stara",
                    < 5 => $"{StarostKnjige} leta stara",
                    < 20 => $"{StarostKnjige} let stara",
                    < 50 => $"{StarostKnjige} let stara",
                    _ => "Klasična knjiga"
                };
            }
        }

        [JsonIgnore]
        public string KratekNaslov => Naslov.Length > 30 ? Naslov.Substring(0, 30) + "..." : Naslov;

        [JsonIgnore]
        public string FormatiranISBN
        {
            get
            {
                if (string.IsNullOrEmpty(ISBN)) return ISBN;

                // Format ISBN-13
                if (ISBN.Length == 13 && ISBN.All(char.IsDigit))
                {
                    return $"{ISBN.Substring(0, 3)}-{ISBN.Substring(3, 1)}-{ISBN.Substring(4, 5)}-{ISBN.Substring(9, 3)}-{ISBN.Substring(12, 1)}";
                }

                // Format ISBN-10
                if (ISBN.Length == 10)
                {
                    return $"{ISBN.Substring(0, 1)}-{ISBN.Substring(1, 3)}-{ISBN.Substring(4, 5)}-{ISBN.Substring(9, 1)}";
                }

                return ISBN;
            }
        }
    }

    // DTO classes for API communication
    public class CreateAvtorDto
    {
        [Required] public string Ime { get; set; } = string.Empty;
        [Required] public string Priimek { get; set; } = string.Empty;
        [Required] public DateTime DatumRojstva { get; set; }
        [Required] public string Email { get; set; } = string.Empty;
        public string? Biografija { get; set; }
    }

    public class CreateKategorijaDto
    {
        [Required] public string Ime { get; set; } = string.Empty;
        public string? Opis { get; set; }
    }

    public class CreateKnjigaDto
    {
        [Required] public string Naslov { get; set; } = string.Empty;
        [Required] public string ISBN { get; set; } = string.Empty;
        [Required] public DateTime DatumIzdaje { get; set; }
        [Required] public int AvtorId { get; set; }
        [Required] public int KategorijaId { get; set; }
    }

    // Response wrapper for API responses
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    // Search/Filter DTOs
    public class SearchCriteria
    {
        public string? SearchTerm { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Category { get; set; }
        public string? SortBy { get; set; } = "name";
        public bool SortDescending { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }
}