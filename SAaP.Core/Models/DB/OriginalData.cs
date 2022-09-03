using LinqToDB.Mapping;
using System.Xml.Linq;


namespace SAaP.Core.Models.DB
{
    [Table(Name = "ORIGINALDATA")]
    public class OriginalData
	{
        [PrimaryKey]
        public string CodeName { get; set; }

        [Column]
        public string Day { get; set; }

        [Column]
        public int Volume { get; set; }

        [Column]
        public double Opening { get; set; }

        [Column]
        public double High { get; set; }

        [Column]
        public double Low { get; set; }

        [Column]
        public double Ending { get; set; }

    }
}
