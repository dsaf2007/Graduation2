using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Graduation2.Models;

// using ExcelDataReader;
// using MySql.Data.MySqlClient;
namespace Graduation2.Models
{
    public class
    Subject // 과목
    {
        public string subjectCode { get; set; } // 학수번호

        public string subjectName { get; set; } // 과목명

        public int credit { get; set; } // 학점

        public string year { get; set; } // 개설연도

        public string semester { get; set; }

        public int designCredit { get; set; } // 설계학점

        public bool IsSameSubject(Subject subject)
        {
            return subjectCode.Equals(subject.subjectCode);
        }
    }

    // todo 기초설계 요소설계 관련 수동 추가해야함
    public class UserSubject : Subject // 사용자 과목 read
    {
        public string completionDiv { get; set; } // 이수구분

        public string completionDivField { get; set; } // 이수구분영역

        public string engineeringFactor { get; set; } // 공학요소

        public string engineeringFactorDetail { get; set; } // 공학세부요소

        public string english { get; set; } // 원어강의종류

        public string retake { get; set; } // 재수강구분

        public List<string> GetKeywords()
        {
            List<string> keywords = new List<string>();
            if (engineeringFactorDetail == "기초교양(교필)")
            {
                keywords.Add("공통교양");
            }
            if (engineeringFactorDetail == "기본소양")
            {
                keywords.Add("기본소양");
            }
            if (engineeringFactor == "MSC/BSM")
            {
                keywords.Add("MSC/BSM");

                // msc 전체 학점 추가해야함
                if (engineeringFactorDetail == "수학")
                {
                    keywords.Add("수학");
                }
                if (engineeringFactorDetail == "기초과학")
                {
                    string key =
                        subjectName.Contains("실험") ? "과학실험" : "과학";
                    keywords.Add (key);
                }
                if (engineeringFactorDetail == "전산학")
                {
                    keywords.Add("전산학");
                }
            }
            if (engineeringFactor == "전공" || completionDiv == "전공")
            {
                keywords.Add("전공");
                if (completionDiv == "전필")
                {
                    keywords.Add("전공필수");
                }
                if (completionDivField == "전문")
                {
                    keywords.Add("전공전문");
                }
                if (engineeringFactorDetail == "전공설계")
                {
                    keywords.Add("설계");
                    if(UserInfo.gichoDesignSubjects.Select(s => s.subjectCode).Contains(subjectCode))
                      keywords.Add("기초설계");
                    if(UserInfo.yosoDesignSubjects.Select(s => s.subjectCode).Contains(subjectCode))
                      keywords.Add("요소설계");
                    if(UserInfo.jonghabDesignSubjects.Select(s => s.subjectCode).Contains(subjectCode))
                      keywords.Add("종합설계");
                }
            }
            if (english == "영어")
            {
                keywords.Add("영어강의");
            }
            if (completionDiv == "일교")
            {
                keywords.Add("일반교양");
            }
            return keywords;
        }
    }
    public class DesignSubject : Subject
    {
      // 기초설계 요소설계 종합설계
      public string designType { get; set; }
    }
}
