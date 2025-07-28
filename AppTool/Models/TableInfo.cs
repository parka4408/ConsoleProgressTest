using System.Collections.Generic;

namespace AppTool.Models;

public class MappingInfo
{
    public string? DisplayName { get; set; }
    public string? InternalName { get; set; }
    public string? DataType { get; set; }
    public bool IsRequired { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsAlternativeKey { get; set; }
}

public class TableInfo
{
    public string Name { get; }
    public string Description { get; }
    public string Category { get; }
    public List<MappingInfo> MappingInfos { get; set; }

    public TableInfo(string name, string description, string category)
    {
        Name = name;
        Description = description;
        Category = category;
        MappingInfos = new List<MappingInfo>();
    }
}
