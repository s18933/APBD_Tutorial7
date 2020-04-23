using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using APBD_Task4.Models;
using Microsoft.AspNetCore.Mvc;

namespace APBD_Task4.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : Controller
    {

        private string ConnString = "Data Source=db-mssql;Initial Catalog=s18933;Integrated Security=True";
      
        [HttpGet]
        public IActionResult GetStudents()
        {
            var result = new List<Student>();
            using (SqlConnection con = new SqlConnection(ConnString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "select FirstName, LastName, BirthDate, Name, Semester " +
                                  "from Student, Enrollment, Studies " +
                                  "where Studies.IdStudy = Enrollment.IdStudy AND Student.IdEnrollment = Enrollment.IdEnrollment";

                con.Open();

                SqlDataReader dr = com.ExecuteReader();
                while (dr.Read())
                {
                    var st = new Student();

                    st.FirstName = dr["FirstName"].ToString();
                    st.LastName = dr["LastName"].ToString();
                    st.BirthDate = DateTime.Parse(dr["BirthDate"].ToString());
                    st.StudiesName = dr["Name"].ToString();
                    st.Semester = Int32.Parse(dr["Semester"].ToString());

                    result.Add(st);

                }

            }
            return Ok(result);
        }

        [HttpGet("{indexNumber}")]
        public IActionResult GetStudent(string indexNumber)
        {
            if (indexNumber.Equals("s18933"))
            {
                return Ok("Mood Pill: https://www.youtube.com/watch?v=4Hg1Kudd_x4&t=2474s");
            }
            using (SqlConnection con = new SqlConnection(ConnString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "select  Semester from Student, Enrollment" +
                                  " where indexNumber=@index AND Student.IdEnrollment = Enrollment.IdEnrollment";

                SqlParameter par1 = new SqlParameter();
                par1.ParameterName = "index";
                par1.Value = indexNumber;

                com.Parameters.Add(par1);

                //com.Parameters.AddWithValue("index", indexNumber);

                con.Open();

                SqlDataReader dr = com.ExecuteReader();
                if (dr.Read())
                {
                    int semester = Int32.Parse(dr["Semester"].ToString());
                    return Ok(indexNumber + " -> " + semester + " semester");
                }

            }

            return NotFound("Invalid input has been performed. Please, try again");
        }
    }
}