using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fyp.Models
{
    public class RoomDetail
    {

        [Key]
        public int RoomId { get; set; }
        public string RoomName { get; set; }
    }
}
