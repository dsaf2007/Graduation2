using System.Runtime.CompilerServices;
using System.Net.Cache;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
// using ExcelDataReader;
// using MySql.Data.MySqlClient;

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

  public class UserSubject : Subject // 사용자 과목 read
  {
    public string completionDiv {get; set;} // 이수구분
    public string completionDivField {get; set;} // 이수구분영역
    public string engineeringFactor {get; set;} // 공학요소
    public string engineeringFactorDetail {get; set;} // 공학세부요소
    public string english {get; set;} // 원어강의종류
    public string retake {get; set;} // 재수강구분

    public string getKeyword() 
    {
      string keyword;
      // keyword 계산?
      return keyword;
    }
  }
  // Category-Credit Pair를 통해 사용자 과목 read
  // public class Pair<"name" , List<string>list /int/ "O,X">
  // {
  //   public string keyword {get; set;}
  //   // public T value {get; set;}
  // }
  // Pair<int> creditList;
  // Pair<List<Subject>> ...;

}