using System.Collections.Generic;
using System.Text;

namespace SSDStressTest
{
    public class Disk
    {
        public string driveLetter { get; set; }
        public string productName { get; set; }
        public string pnpId { get; set; }
        public Dictionary<string, int> smartData { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Disk ").Append(this.driveLetter).Append(": ").AppendLine(this.productName);
            if (this.smartData != null)
            {
                foreach (var item in this.smartData)
                    sb.Append(item.Key).Append(": ").Append(item.Value).AppendLine();
            }
            return sb.ToString();
        }
    }
}
