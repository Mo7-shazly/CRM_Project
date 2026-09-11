using System.Collections.ObjectModel;
using CRM.Desktop.Models;

namespace CRM.Desktop.ViewModels;

public sealed class OpportunityColumn
{
    public OpportunityColumn(DealStage stage, string name) { Stage = stage; Name = name; }
    public DealStage Stage { get; }
    public string Name { get; }
    public ObservableCollection<Opportunity> Opportunities { get; } = new();
    public decimal TotalValue => Opportunities.Sum(x => x.Value);
}
