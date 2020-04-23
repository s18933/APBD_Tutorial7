using APBD_Task4.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task7.Models;

namespace APBD_Task4.Sevices
{
     public interface IStudentsDbService
    {
        EnrollStudentRequest EnrollStudent(EnrollStudentRequest request);
        EnrollStudentRequest PromoteStudents(EnrollStudentRequest request);
        List<String> Login(LoginRequestDto request);
        List<String> TokenExists(string requestToken);
        void RefreshToken(string requestToken, string IndexNumber);
        bool encodePasswords(string connString);
    }
}
