using System.Collections.ObjectModel;

namespace DA_TendonToolsWpf
{
    public class CommonTendonStyles:ObservableCollection<string>
    {
        public CommonTendonStyles():base()
        {
            for (int i = 1; i <= 19; i++)
            {
                Add($"Φ15-{i}");
            }
            Add("Φ15-21");
            Add("Φ15-22");
            Add("Φ15-25");
            Add("Φ15-27");
            Add("Φ15-37");
            Add("Φ15-43");
            Add("Φ15-55");
        }        
    }
}
