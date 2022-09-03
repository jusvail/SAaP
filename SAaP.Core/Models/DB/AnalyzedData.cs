﻿using LinqToDB.Mapping;
using System.Xml.Linq;

namespace SAaP.Core.Models.DB
{
    [Table(Name = "AnalyzedData")]
    public class AnalyzedData
    {

        [PrimaryKey]
        public string CodeName { get; set; }

        [Column]
        public string CompanyName { get; set; }

        [Column]
        public int BelongTo { get; set; }
    }
}