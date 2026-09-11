using System.Globalization;
using System.IO;
using System.Text;
using CRM.Desktop.Data;
using CRM.Desktop.Models;
using Microsoft.EntityFrameworkCore;

namespace CRM.Desktop.Services;

public sealed class QuotationService
{
    public List<Quotation> GetQuotations(AppUser user)
    {
        using var db = new CrmDbContext(); IQueryable<Quotation> quotes = db.Quotations.AsNoTracking().Include(x => x.Customer).Include(x => x.Items);
        if (user.Role == UserRole.Sales) quotes = quotes.Where(x => x.OwnerName == user.DisplayName);
        return quotes.OrderByDescending(x => x.IssueDate).ToList();
    }
    public List<Customer> GetCustomers() { using var db = new CrmDbContext(); return db.Customers.AsNoTracking().OrderBy(x => x.CompanyName).ToList(); }
    public Quotation Save(Quotation quote, AppUser user)
    {
        using var db = new CrmDbContext();
        if (quote.Id == 0) { quote.QuoteNumber = $"Q-{(db.Quotations.Max(x => (int?)x.Id) ?? 0) + 1:000}"; quote.OwnerName = user.DisplayName; db.Quotations.Add(quote); }
        else { var existing = db.Quotations.Include(x => x.Items).Single(x => x.Id == quote.Id); if (user.Role == UserRole.Sales && existing.OwnerName != user.DisplayName) throw new UnauthorizedAccessException("You can only edit your own quotations."); existing.CustomerId = quote.CustomerId; existing.Status = quote.Status; existing.IssueDate = quote.IssueDate; existing.ValidUntil = quote.ValidUntil; existing.Notes = quote.Notes; db.QuotationItems.RemoveRange(existing.Items); existing.Items = quote.Items; quote = existing; }
        db.SaveChanges(); return GetQuotation(quote.Id, user) ?? quote;
    }
    public Quotation? GetQuotation(int id, AppUser user) { using var db = new CrmDbContext(); IQueryable<Quotation> quotes = db.Quotations.AsNoTracking().Include(x => x.Customer).Include(x => x.Items).Where(x => x.Id == id); if (user.Role == UserRole.Sales) quotes = quotes.Where(x => x.OwnerName == user.DisplayName); return quotes.SingleOrDefault(); }
    public byte[] BuildPdf(Quotation quote)
    {
        var lines = new List<string> { "CRM", $"Quotation {quote.QuoteNumber}", $"Customer: {quote.Customer?.CompanyName}", $"Issue date: {quote.IssueDate:d}", "" };
        lines.AddRange(quote.Items.Select(x => $"{x.Description}  |  {x.Quantity:0.##} x {x.UnitPrice:N2} = {(x.Quantity * x.UnitPrice):N2}"));
        lines.Add(""); lines.Add($"Total: {quote.Items.Sum(x => x.Quantity * x.UnitPrice):N2}");
        if (!string.IsNullOrWhiteSpace(quote.Notes)) { lines.Add(""); lines.Add($"Notes: {quote.Notes}"); }
        var content = new StringBuilder("BT /F1 16 Tf 50 780 Td ");
        foreach (var line in lines) content.Append('(').Append(EscapePdf(line)).Append(") Tj 0 -22 Td ");
        content.Append("ET");
        var stream = content.ToString();
        var objects = new[] { "<< /Type /Catalog /Pages 2 0 R >>", "<< /Type /Pages /Kids [3 0 R] /Count 1 >>", "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>", "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>", $"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}\nendstream" };
        using var output = new MemoryStream(); using var writer = new StreamWriter(output, Encoding.ASCII, 1024, true);
        writer.Write("%PDF-1.4\n"); var offsets = new List<long> { 0 };
        for (var i = 0; i < objects.Length; i++) { writer.Flush(); offsets.Add(output.Position); writer.Write($"{i + 1} 0 obj\n{objects[i]}\nendobj\n"); }
        writer.Flush(); var xref = output.Position; writer.Write($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n"); foreach (var offset in offsets.Skip(1)) writer.Write($"{offset:0000000000} 00000 n \n"); writer.Write($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF"); writer.Flush(); return output.ToArray();
    }
    private static string EscapePdf(string value) => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)").Replace("\r", " ").Replace("\n", " ").Replace("—", "-");
}
