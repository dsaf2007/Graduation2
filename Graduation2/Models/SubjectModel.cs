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

namespace Graduation2.Models
{
  public class Subject // 과목
  {
      public string subjectCode {get; set;} // 학수번호
      public string subjectName {get; set;} // 과목명
      public int credit {get;set;} // 학점
      public string year {get;set;} // 개설연도
      public int designCredit {get;set;} // 설계학점
  }
  // Category-Credit Pair를 통해 사용자 과목 read
  public class Pair<"name" , List<string>list /int/ "O,X">
  {
    public string keyword {get; set;}
    // public T value {get; set;}
  }
  // Pair<int> creditList;
  // Pair<List<Subject>> ...;

}