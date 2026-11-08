namespace ReviveOS.Core;
public record ResourceSample(DateTime Utc,double? CpuPercent,long AvailableMemoryMB,long TotalMemoryMB,double? DiskQueue,bool? OnBattery,double? AgentCpuPercent,double AgentWorkingSetMB);
