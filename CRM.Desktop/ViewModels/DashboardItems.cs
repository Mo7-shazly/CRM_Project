namespace CRM.Desktop.ViewModels;
public sealed record KpiItem(string Icon, string Title, string Value, string Change, string Accent);
public sealed record PipelineStageItem(string Name, int Count, decimal Value, string Color);
public sealed record SalesItem(string Name, int Deals, decimal Sales);
public sealed record SalesMonthItem(string Name, decimal Value, double Height = 8);
public sealed record FollowUpItem(string Type, string Title, string Subtitle, string AssignedTo, DateTime DueAt, bool IsOverdue);
public sealed record RecentActivityItem(string Type, string Title, string Customer, DateTime OccurredAt);
