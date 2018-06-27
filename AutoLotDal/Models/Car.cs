using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoLotDal.Models
{
    public class Car
    {
        public virtual long CarId{get;set;}
        public virtual string Color { get; set; }
        public virtual string Make { get; set; }
        public virtual string PetName { get; set; }
    }
}
