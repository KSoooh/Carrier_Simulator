using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carrier_Simulator.Models
{
    public class Section
    {
        public int SectionNumber { get; set; }
        public double Speed { get; set; } //추후에 구간을 지난 속도 표시용
        public string Position { get; set; }
    }
}
