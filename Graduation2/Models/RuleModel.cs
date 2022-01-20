using System.Runtime.CompilerServices;
using System.Net.Cache;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ExcelDataReader;
using MySql.Data.MySqlClient;

namespace Rule.Models
{
    public class Rule
    {// 구분 (교양, 전공, 졸업요건, 예외)
        public string type { get; set; }
        // 일련번호
        public string sequenceNumber { get; set; }
        // 질문
        public string question { get; set; }
        // 엑셀 입력 데이터
        public string singleInput { get; set; }
        // rule passed
        public bool isPassed { get; set; }

        //public List<Subject> requiredSubjects {get; set;} 


    }

    public class RuleBuilder
    {
        
    }
}