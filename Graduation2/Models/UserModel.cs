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
using Graduation2.Models;

namespace Graduation2.Models
{
/*
  public class UserInfo
  {// 취득교과목
    const List<String> keywords = 
    {
    "공통교양", "기본소양", "수학", // MSC랑 수학과학전산학은 어떻게?
    "과학", // 실험 포함? 따로?
    "전산학", "전공", // 전공 안에 필수, 설계 등을 type 형태로 구분?
    "전공필수", "전공설계",
    "기초설계", // 얘네는 성적표에 '전공설계'로 나옴
    "요소설계", "종합설계",
    };
    // <keyword - 과목리스트>
    public Dictionary<string, List<UserSubject>> subjectPair;
    public int calcKeywordCredit()
    {
      return 0;
    }
    */
    /*
      - publicLibCredit # 수강한 총 공통교양 학점
      - basicLibCredit # 수강한 총 기본소양 학점
      - majorCredit # 수강한 총 전공학점
      - majorDesignCredit # 수강한 총 전공 설계 학점
      - majorEssentialCredit # 수강한 총 전공 필수 학점
      - majorSpecialCredit # 수강한 총 전공 전문 학점
      - mscCredit # 수강한 총 MSC 학점
      - mscMathCredit # MSC 수학 / 과학 (실험) / 컴퓨터
      - mscScienceCredit
      - mscScienceExperimentCredit
      - mscComputerCredit
      - englishCredit # 영어 강의
      - englishMajorCredit # 영어 전공 강의
      - totalCredit # 전체 수강한 학점
      - gradeAverage # 학점 평균
    */
    public class TempRule
    {
        public string keyword;
        public string question_type;
        public string value;
    }
    public class UserInfo // 취득교과목
    {

        //public List<Rule.Models.Rule> rule = new List<Rule.Models.Rule>();
        public List<TempRule> rule = new List<TempRule>();
        //    public List<ListPair> subjectNameList = new List<ListPair>();
        //     public List<NumPair> subjectCreditList = new List<NumPair>(); 
        public Dictionary<string, List<string>> subjectNameList = new Dictionary<Subject, List<string>>();
        public Dictionary<string, int> subjectCreditList = new Dictionary<string, int>();


            // dev temp
        public static readonly string[] subjectKeywords = {
            "공통교양", "기본소양", "수학", // MSC랑 수학과학전산학은 어떻게?
            "과학", // 실험 포함? 따로?
            "전산학", "전공", // 전공 안에 필수, 설계 등을 type 형태로 구분?
            "전공필수", "전공설계",
            "기초설계", // 얘네는 성적표에 '전공설계'로 나옴
            "요소설계", "종합설계"
        };
        public void getRule()
        {
            using (MySqlConnection connection = new MySqlConnection("Server=101.101.216.163/;Port=5555;Database=testDB;Uid=CSDC;Pwd=1q2w3e4r"))
            {
                string selectQuery = "SELECT * FROM rule";
                connection.Open();
                MySqlCommand command = new MySqlCommand(selectQuery, connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // TempRule temp = new TempRule();
                        // temp.keyword=reader["keyword"].ToString();
                        // temp.question_type=reader["question_type"].ToString();
                        // temp.value = reader["value"].ToString();
                        rule.Add(new TempRule{
                reader["keyword"].ToString(),
                reader["question_type"].ToString(),
                reader["value"].ToString()
                });
                    }
                }
            }
        }


        public void getUserSubject(string filename_)
        {
            // using (var subjectStream = System.IO.File.Open(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            // {
            //     using (var subjectReader = ExcelReaderFactory.CreateReader(SubjectStream))
            //     {
            //         subjectReader.Read();
            //         using (MysqlConnection connection = new MysqlConnection("Server=101.101.216.163/;Port=5555;Database=testDB;Uid=CSDC;Pwd=1q2w3e4r"))
            //         {
            //             while(subjectReader.Read())
            //             {
            //             // string insertQuery = string.Format("INSERT INTO UserSubject(year,semester,completionDiv,completionDivField,SubjectCode,"
            //             //                     +"SubjectName,credit,engineeringFactor,engineeringFactorDetail,english,retake)"
            //             //                     +"VALUES ( '{0}','{1}','{2}','{3}','{4}','{5}',{6},'{7}','{8}','{9}','{10}')"
            //             //                     ,subjectReader
            //             }
            //         }

            //     }
            // }


            List<UserSubject> userSubjects = new List<UserSubject>();

            UserSubject testSubject = new UserSubject();
            testSubject.credit = 3;
            userSubjects = ReadUserSubject(filename_);
            // dev temp
            userSubjects.Add(testSubject);
            // dev temp
            Dictionary<string, List<UserSubject>> keywordSubjectPair = new Dictionary<string, List<UserSubject>>();
            Dictionary<string, int> keywordCreditPair = new Dictionary<string, int>();


            foreach (UserSubject userSubject in userSubjects)
            {
                int subjectCredit = Convert.ToInt32(userSubject.credit);
                // this.totalCredit += subjectCredit;

                // dev temp
                List<string> keywordsOfSubject = userSubject.getKeyword();
                foreach(string keyword in keywordsOfSubject)
                {
                  if(!subjectKeywords.Contains(keyword))
                  {
                    Console.WriteLine("Invalid keyword!");
                  }
                  else
                  {
                    // plus DB upload?
                    keywordSubjectPair[keyword].Add(userSubject);
                    keywordCreditPair[keyword] += userSubject.credit;
                  }
                }
            }
            // dev temp
            int _totalCredit = keywordCreditPair.Aggregate(0, (acc, subject) => acc + subject.Value);
        }

        public List<UserSubject> ReadUserSubject(string filename_)
        {
            List<UserSubject> temp = new List<UserSubject>();

            // 전체성적조회파일
            using (var gradeStream = System.IO.File.Open(filename_, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (var gradeReader = ExcelReaderFactory.CreateReader(gradeStream))
                {
                    gradeReader.Read();
                    string tempYear = "";
                    string tempSemester = "";
                    while (gradeReader.Read())
                    {
                        string[] valueArray = new string[19];
                        for (int i = 0; i < 19; i++)
                        {
                            if (gradeReader.GetValue(i) == null)
                                valueArray[i] = "";
                            else
                                valueArray[i] = Regex.Replace(gradeReader.GetValue(i).ToString(), @"\s", "");
                        }
                        if (valueArray[2] != "")
                        {
                            tempYear = valueArray[2];
                        }
                        if (valueArray[3] != "")
                        {
                            tempSemester = valueArray[3];
                        }
                        temp.Add(new UserSubject
                        {
                            year = tempYear, // 연도
                            semester = tempSemester, // 학기
                            completionDiv = valueArray[4], // 이수구분 : 전공, 전필, 학기, 공교 등
                            completionDivField = valueArray[5], // 이수구분영역 : 기초, 전문, 자연과학 등
                            classCode = valueArray[6], // 학수번호
                            className = valueArray[8], // 과목명
                            credit = valueArray[10], // 학점
                            engineeringFactor = valueArray[16], // 공학요소 : 전공, MSC, 전문교양
                            engineeringFactorDetail = valueArray[17], // 공학세부요소 : 전공설계, 수학, 과학 등
                            english = valueArray[18], // 원어강의 종류
                            retake = valueArray[13] //재수강 여부
                        });
                    }
                }
            }
            return temp;
        }

        public List<string> addToList(List<string> list_, string input_)
        {
            List<string> temp = new List<string>();

            temp = list_;
            temp.Add(input_);

            return temp;
        }

        public int addNum(int num_, int add_)
        {
            return num_ + add_;
        }

        // dictionary를 사용하기에 예외
        // public void initPairList()
        // {
        //     using (MySqlConnection connection = new MySqlConnection("Server=101.101.216.163/;Port=5555;Database=testDB;Uid=CSDC;Pwd=1q2w3e4r"))
        //     {
        //         string selectQuery = "SELECT DISTINCT keyword FROM rule";
        //         connection.Open();
        //         MySqlCommand command = new MySqlCommand(selectQuery, connection);
        //         List<string> tempList = new List<string>();

        //         using (var reader = command.ExecuteReader())
        //         {
        //             while (reader.Read())
        //             {
        //             subjectNameList.Add(reader["keyword"].ToString(),tempList);
        //             subjectCreditList.add(reader["keyword"].ToString(),0);
        //             }
        //         }
        //     }
        // }


    }
}