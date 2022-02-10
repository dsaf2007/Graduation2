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
using Graduation2.Models;

namespace Graduation2.Models
{
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
        public string keyword{get; set;}
        public string question_type{get; set;}
        public string value{get; set;}
    }
    public class UserInfo // 취득교과목
    {

        //public List<Rule.Models.Rule> rule = new List<Rule.Models.Rule>();
        public List<TempRule> rule = new List<TempRule>();

        private Dictionary<string, List<UserSubject>> keywordSubjectPair;
        private Dictionary<string, int> keywordCreditPair;

        private int totalCredit;

        public static readonly string[] ruleKeywords = {
            "공통교양", "기본소양", "수학", // MSC랑 수학과학전산학은 어떻게?
            "과학", // 실험 포함? 따로?
            "전산학", "전공", // 전공 안에 필수, 설계 등을 type 형태로 구분?
            "전공필수", "전공설계",
            "기초설계", // 얘네는 성적표에 '전공설계'로 나옴
            "요소설계", "종합설계"
        };

        public UserInfo()
        {
          keywordSubjectPair = new Dictionary<string, List<UserSubject>>();
          keywordCreditPair = new Dictionary<string, int>();
          totalCredit = 0;
        }

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
                keyword = reader["keyword"].ToString(),
                question_type = reader["question_type"].ToString(),
                value = reader["value"].ToString()
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
            // userSubjects = ReadUserSubject(filename_);

            foreach (UserSubject userSubject in userSubjects)
            {
                List<string> keywordsOfSubject = userSubject.getKeywords();
                foreach(string keyword in keywordsOfSubject)
                {
                  keywordSubjectPair[keyword].Add(userSubject);
                  keywordCreditPair[keyword] += userSubject.credit;
                }
                totalCredit += userSubject.credit;
            }
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
                            subjectCode = valueArray[6], // 학수번호
                            subjectName = valueArray[8], // 과목명
                            credit = Convert.ToInt32(valueArray[10]), // 학점
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

        public void CheckRule()
        {
            //keywordSubjectPair, keywordCreditPair

            foreach(TempRule temprule in rule)
            {
                if(temprule.question_type == "단수")
                {
                    if(temprule.value != keywordCreditPair[temprule.keyword]) // rule의 단수가 일치하지 않을 때
                    {
                        //keyword 의 학점을 만족하지 않는다 error message 출력
                    }
                }
                else if(temprule.question_type == "목록")
                {
                    //하나의 String 형태인 value를 list 형태로 pharsing 후 비교
                    List<string> valueList = temprule.value.Split('/');
                    //List간 비교
                    foreach(string value in valueList)
                    {
                        foreach(string subject in keywordSubjectPairl[temprule.keyword])
                        {
                            
                        }
                    }
                }
            }
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