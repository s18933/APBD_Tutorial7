using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace APBD_Task4.DTOs.Requests
{
    public class EnrollStudentRequest
    {
        [RegularExpression("^(?i)s[0-9]+$")]
        public string IndexNumber { get; set; }
      
        [MaxLength(20)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
 
        public string StudiesName { get; set; }
        public int Semester { get; set; }
    }
}
