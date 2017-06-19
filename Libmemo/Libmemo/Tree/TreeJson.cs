using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libmemo {
    public class TreeJson {
        public int user_id { get; set; }
        public TreeDataJson[] data { get; set; }
    }

    public class TreeDataJson { 
        public int type { get; set; }    
        public int id { get; set; } //current id        
        public int m_id { get; set; } //mother id      
        public int f_id { get; set; } //father id       
        public int[] s_ids { get; set; } //siblings id
    }
}
