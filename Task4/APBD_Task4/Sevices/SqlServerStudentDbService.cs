using APBD_Task4.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Task7.Models;

namespace APBD_Task4.Sevices
{

    public class SqlServerStudentDbService : IStudentsDbService
    {
        private string _connString = "Data Source=db-mssql;Initial Catalog=s18933;Integrated Security=True";

        public List<string> Login(LoginRequestDto request)
        {

            string salt = "";
            string password = "";

            string IndexNubmer = "";
            string FirstName = "";
            string Role = "";
            using (var con = new SqlConnection(_connString))
            using (var com = new SqlCommand())

            {
                com.Connection = con;
                com.CommandText = "Select Salt, Password from Student where Student.IndexNumber=@ID";
                com.Parameters.AddWithValue("ID", request.ID);
               
                con.Open();
                var dr = com.ExecuteReader();
                if (!dr.Read())
                {
                    dr.Close();
                    return null;
                }
                    salt = dr["Salt"].ToString();
                    password = dr["Password"].ToString();

                dr.Close();
                if (!Encryption.Encryption.Validate(request.Haslo, salt, password))
                {
                    return null;
                }
                com.CommandText = "Select Student.IndexNumber AS IndexN, FirstName , Role.Role AS Role from Student, Role where Student.IndexNumber=@ID AND Student.Password=@Password AND Student.Role_ID = Role.IndexNumber";
                com.Parameters.AddWithValue("Password", password);


                dr = com.ExecuteReader();
                while (dr.Read())
                {
                    IndexNubmer = dr["IndexN"].ToString();
                    FirstName = dr["FirstName"].ToString();
                    Role = dr["Role"].ToString();
                }
                dr.Close();
            }
            var list = new List<String>();
            list.Add(IndexNubmer);
            list.Add(FirstName);
            list.Add(Role);

            return list;
        }


        public List<String> TokenExists(string requestToken)
        {

            string IndexNubmer = "";
            string FirstName = "";
            string Role = "";
            var list = new List<String>();

            using (var con = new SqlConnection(_connString))
            using (var com = new SqlCommand())
            {
                com.Connection = con;

                com.CommandText = "Select Student.IndexNumber AS IndexN, FirstName , Role.Role AS Role from Student, Role where Student.RefreshToken=@Token AND Student.Role_ID = Role.IndexNumber";
                com.Parameters.AddWithValue("Token", requestToken);

                con.Open();

                var dr = com.ExecuteReader();
                while (dr.Read())
                {
                    IndexNubmer = dr["IndexN"].ToString();
                    FirstName = dr["FirstName"].ToString();
                    Role = dr["Role"].ToString();
                }
                dr.Close();
            }

            list.Add(IndexNubmer);
            list.Add(FirstName);
            list.Add(Role);
            return (list);
        }

        public void RefreshToken(string requestToken, string IndexNumber)
        {
            using (var con = new SqlConnection(_connString))
            using (var com = new SqlCommand())

            {
                com.Connection = con;

                com.CommandText = "UPDATE Student SET RefreshToken =@Token WHERE IndexNumber=@IndexNumber";
                com.Parameters.AddWithValue("IndexNumber", IndexNumber);
                com.Parameters.AddWithValue("Token", requestToken);

                con.Open();

                com.ExecuteNonQuery();
            }
        }

        public bool encodePasswords(string connString) 
        {
            List<String> list = new List<String>();
            using (var con = new SqlConnection(connString))
            using (var com = new SqlCommand())

            {
                com.Connection = con;

                com.CommandText = "SELECT Password from Student";

                con.Open();

                var dr = com.ExecuteReader();
                if (!dr.Read())
                {
                    dr.Close();
                    return false;
                }
                else
                {
                    while (dr.Read()) 
                    {
                        var password = dr["Password"].ToString();
                        list.Add(password);
                    }
                    dr.Close();

                    com.Parameters.AddWithValue("Original", "");
                    com.Parameters.AddWithValue("Salt", "");
                    com.Parameters.AddWithValue("Encoded", "");
                    for (int i = 0; i < list.Count; i++)
                    {
                        com.CommandText = "UPDATE Student SET Password=@Encoded, Salt=@Salt WHERE Password=@Original";

                        string salt = Encryption.Encryption.CreateSalt();
                        com.Parameters["Original"].Value = list[i];
                        com.Parameters["Salt"].Value = salt;
                        com.Parameters["Encoded"].Value = Encryption.Encryption.Create(list[i], salt);

                        com.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            return false;
        }

        public EnrollStudentRequest EnrollStudent(EnrollStudentRequest request)
        {
            if (request.FirstName == null || request.BirthDate == null || request.StudiesName == null)
            {
                return null;
            }

            var enrollment = new EnrollStudentRequest();
            using (var con = new SqlConnection(_connString))
            {
                con.Open();
                var tran = con.BeginTransaction();

                try
                {
                    using (var com = new SqlCommand())
                    {
                        int idStudy = 0;
                        int idEnrollment = 0;

                        com.Connection = con;
                        com.Transaction = tran;
                        com.CommandText = "select IdStudy from Studies where Name=@StudiesName";
                        com.Parameters.AddWithValue("StudiesName", request.StudiesName);



                        var dr = com.ExecuteReader();
                        if (!dr.Read())
                        {
                            dr.Close();
                            tran.Rollback();
                            return null;
                        }
                        else
                        {
                            idStudy = (int)dr["IdStudy"];
                            com.Parameters.AddWithValue("IdStudy", idStudy);
                        }

                        dr.Close();

                        com.CommandText = "Select IdEnrollment from Enrollment where Semester=1 AND IdStudy=@idStudy";

                        dr = com.ExecuteReader();
                        if (!dr.Read())
                        {
                            dr.Close();

                            com.CommandText = "Select MAX(IdEnrollment) from Enrollment";
                            DateTime currentDate = DateTime.Now;
                            com.Parameters.AddWithValue("CurrentDate", currentDate);

                            var dr1 = com.ExecuteReader();
                            while (dr1.Read())
                            {
                                idEnrollment = int.Parse(dr1["IdEnrollment"].ToString()) + 1;
                                com.Parameters.AddWithValue("IdEnrollment", idEnrollment);
                            }
                            dr1.Close();

                            com.CommandText = "Insert into Enrollment(IdEnrollment, Semester, IdStudy, StartDate) values (@IdEnrollment, 1, @idStudy, @CurrentDate)";
                            com.ExecuteNonQuery();
                        }
                        else
                        {
                            idEnrollment = (int)dr["IdEnrollment"];
                            com.Parameters.AddWithValue("IdEnrollment", idEnrollment);
                            dr.Close();
                        }

                        com.CommandText = "Insert into Student(IndexNumber, FirstName, LastName, Birthdate, IdEnrollment) values (@IndexNumber, @FirstName, @LastName, @BirthDate, @IdEnrollment)";

                        string indexNumber = $"s{new Random().Next(1, 2000)}";
                        com.Parameters.AddWithValue("IndexNumber", indexNumber);
                        com.Parameters.AddWithValue("FirstName", request.FirstName);
                        com.Parameters.AddWithValue("LastName", request.LastName);
                        com.Parameters.AddWithValue("BirthDate", request.BirthDate);

                        dr.Close();
                        com.ExecuteNonQuery();

                        com.CommandText = "Select IndexNumber, FirstName, LastName, BirthDate, Name, Semester from Student, Studies, Enrollment " +
                            "where Enrollment.IdEnrollment = Student.IdEnrollment AND Enrollment.IdStudy = Studies.IdStudy " +
                            "AND IndexNumber = @IndexNumber;";

                        dr = com.ExecuteReader();
                        while (dr.Read())
                        {
                            enrollment.IndexNumber = dr["IndexNumber"].ToString();
                            enrollment.FirstName = dr["FirstName"].ToString();
                            enrollment.LastName = dr["LastName"].ToString();
                            enrollment.BirthDate = DateTime.Parse(dr["BirthDate"].ToString());
                            enrollment.StudiesName = dr["Name"].ToString();
                            enrollment.Semester = Int32.Parse(dr["Semester"].ToString());
                        }
                        dr.Close();
                    }
                }
                catch (Exception)
                {
                    tran.Rollback();
                }
                tran.Commit();
            }
            return enrollment;
        }

        public EnrollStudentRequest PromoteStudents(EnrollStudentRequest request)
        {
            var enrollment = new EnrollStudentRequest();
            using (var con = new SqlConnection(_connString))
            using (var com = new SqlCommand())

            {
                com.Connection = con;

                com.CommandText = "EXEC PromoteStudents @Name, @Semester"; 
                com.Parameters.AddWithValue("Name", request.StudiesName);
                com.Parameters.AddWithValue("Semester", request.Semester);

                con.Open();
                com.ExecuteNonQuery();

                com.CommandText = "Select IndexNumber, FirstName, LastName, BirthDate, Name, Semester from Student, Studies, Enrollment " +
                            "WHERE Enrollment.IdEnrollment = Student.IdEnrollment AND Enrollment.IdStudy = Studies.IdStudy " +
                            "AND Semester = (@Semester+1)";
                var dr = com.ExecuteReader();
                while (dr.Read())
                {
                    enrollment.IndexNumber = dr["IndexNumber"].ToString();
                    enrollment.FirstName = dr["FirstName"].ToString();
                    enrollment.LastName = dr["LastName"].ToString();
                    enrollment.BirthDate = DateTime.Parse(dr["BirthDate"].ToString());
                    enrollment.StudiesName = dr["Name"].ToString();
                    enrollment.Semester = Int32.Parse(dr["Semester"].ToString());
                }
                dr.Close();
            }
            return enrollment;

        }
    }
}
