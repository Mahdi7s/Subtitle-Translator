using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace SubtitleTranslator.DataModel
{
    public class Dictionary
    {
        [Key]
        public long ID { get; set; }

        public string OrginalWord { get; set; }
        public string PersianWord { get; set; }
    }
}
