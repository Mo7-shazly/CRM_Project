namespace CRM.Desktop.Models;

public enum UserRole { Admin, Manager, Sales }
public enum LeadStatus { New, Contacted, Qualified, ProposalSent, Negotiation, Won, Lost }
public enum DealStage { NewLead, Qualified, Proposal, Negotiation, Won, Lost }
public enum CustomerStatus { Active, Prospect, Inactive }
public enum CustomerPriority { Low, Medium, High, VIP }
public enum QuotationStatus { Draft, Sent, Accepted, Rejected }
public enum ProjectStatus { Planned, InProgress, OnHold, Completed, Cancelled }

public sealed class AppUser { public int Id { get; set; } public string DisplayName { get; set; } = ""; public string Email { get; set; } = ""; public string PasswordHash { get; set; } = ""; public UserRole Role { get; set; } public bool IsActive { get; set; } = true; }
public sealed class Lead
{
    public int Id { get; set; }
    public string LeadCode { get; set; } = "";
    public int? CustomerId { get; set; }
    public int? OpportunityId { get; set; }
    public string Company { get; set; } = "";
    public string ContactName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string WhatsApp { get; set; } = "";
    public string Email { get; set; } = "";
    public string Industry { get; set; } = "";
    public string Source { get; set; } = "";
    public string AssignedTo { get; set; } = "";
    public LeadStatus Status { get; set; }
    public int Score { get; set; }
    public string Notes { get; set; } = "";
    public decimal EstimatedValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastContactAt { get; set; }
    public DateTime? NextFollowUpAt { get; set; }
    public Customer? Customer { get; set; }
    public Opportunity? Opportunity { get; set; }
}
public sealed class LeadSource { public int Id { get; set; } public string Name { get; set; } = ""; public bool IsActive { get; set; } = true; }
public sealed class Customer
{
    public int Id { get; set; }
    public string CustomerCode { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public string Industry { get; set; } = "";
    public string CompanySize { get; set; } = "";
    public string Website { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;
    public CustomerPriority Priority { get; set; } = CustomerPriority.Medium;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<ContactPerson> Contacts { get; set; } = new();
    public List<CustomerActivity> Activities { get; set; } = new();
}
public sealed class ContactPerson { public int Id { get; set; } public int CustomerId { get; set; } public string FullName { get; set; } = ""; public string JobTitle { get; set; } = ""; public string Phone { get; set; } = ""; public string Email { get; set; } = ""; public bool IsPrimary { get; set; } public Customer? Customer { get; set; } }
public sealed class CustomerActivity
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Details { get; set; } = "";
    public string AssignedTo { get; set; } = "";
    public DateTime OccurredAt { get; set; } = DateTime.Now;
    public DateTime? ReminderAt { get; set; }
    public bool IsCompleted { get; set; }
    public Customer? Customer { get; set; }
}
public sealed class Opportunity
{
    public int Id { get; set; }
    public int? CustomerId { get; set; }
    public string Name { get; set; } = "";
    public string OwnerName { get; set; } = "";
    public DealStage Stage { get; set; }
    public decimal Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpectedCloseAt { get; set; }
    public Customer? Customer { get; set; }
}
public sealed class Quotation
{
    public int Id { get; set; }
    public string QuoteNumber { get; set; } = "";
    public int CustomerId { get; set; }
    public string OwnerName { get; set; } = "";
    public QuotationStatus Status { get; set; } = QuotationStatus.Draft;
    public DateTime IssueDate { get; set; } = DateTime.Today;
    public DateTime? ValidUntil { get; set; }
    public string Notes { get; set; } = "";
    public Customer? Customer { get; set; }
    public List<QuotationItem> Items { get; set; } = new();
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal Total => Items.Sum(x => x.Quantity * x.UnitPrice);
}
public sealed class QuotationItem { public int Id { get; set; } public int QuotationId { get; set; } public string Description { get; set; } = ""; public decimal Quantity { get; set; } = 1; public decimal UnitPrice { get; set; } public Quotation? Quotation { get; set; } }
public sealed class CrmProject { public int Id { get; set; } public string ProjectCode { get; set; } = ""; public int CustomerId { get; set; } public string Name { get; set; } = ""; public string AssignedTo { get; set; } = ""; public ProjectStatus Status { get; set; } = ProjectStatus.Planned; public int Progress { get; set; } public DateTime StartDate { get; set; } = DateTime.Today; public DateTime? EndDate { get; set; } public string Notes { get; set; } = ""; public Customer? Customer { get; set; } }
