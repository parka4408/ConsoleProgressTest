namespace AppTool.Models;

public class TableInfo
{
    public string Name { get; }
    public string Description { get; }
    public string Category { get; }

    public TableInfo(string name, string description, string category)
    {
        Name = name;
        Description = description;
        Category = category;
    }
}
