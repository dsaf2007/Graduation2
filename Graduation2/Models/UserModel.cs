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
using System.Linq;

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
        public string keyword { get; set; }
        public string question_type { get; set; }
        public string value { get; set; }

        public bool check { get; set; }
    }
    public class UserInfo // 취득교과목
    {

        //public List<Rule.Models.Rule> rule = new List<Rule.Models.Rule>();
        public List<TempRule> rule = new List<TempRule>();

        private Dictionary<string, List<UserSubject>> keywordSubjectPair;
        private Dictionary<string, int> keywordCreditPair;

        public Dictionary<string, string> errorMessageList;

        private int totalCredit;

        public static readonly string[] ruleKeywords = {
            "공통교양", "기본소양", "일반교양",
            "수학", "과학", "전산학", "과학실험", "MSC/BSM",
            "전공", "전공필수", "전공전문", "전공설계",
            "기초설계", "요소설계", "종합설계", // 얘네는 성적표에 '전공설계'로 나옴
            "영어"
        };

        public UserInfo()
        {
          keywordSubjectPair = new Dictionary<string, List<UserSubject>>();
          keywordCreditPair = new Dictionary<string, int>();
          foreach(string keyword in ruleKeywords)
          {
            keywordSubjectPair.Add(keyword, new List<UserSubject>());
            keywordCreditPair.Add(keyword, 0);
          }
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
                        rule.Add(new TempRule
                        {
                            keyword = reader["keyword"].ToString(),
                            question_type = reader["question_type"].ToString(),
                            value = reader["value"].ToString(),
                            check = false
                        });
                        errorMessageList.Add(reader["keyword"].ToString(), "");
                    }
                }
            }
        }


        public void getUserSubject(string studentScoreFile)
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
            userSubjects = readUserSubject(studentScoreFile);

            foreach (UserSubject userSubject in userSubjects)
            {
                List<string> keywordsOfSubject = userSubject.getKeywords();
                foreach (string keyword in keywordsOfSubject)
                {
                    keywordSubjectPair[keyword].Add(userSubject);
                    keywordCreditPair[keyword] += userSubject.credit;
                }
                totalCredit += userSubject.credit;
            }
            printUserSubjects();
        }
        // debug
        public void printUserSubjects()
        {
            foreach(string key in keywordSubjectPair.Keys)
            {
              Console.WriteLine("<{0}> 총 {1}과목 {2}학점 수강",key, keywordSubjectPair[key].Count, keywordCreditPair[key]);
              foreach(UserSubject subject in keywordSubjectPair[key])
              {
                Console.WriteLine("[{0}] {1}", subject.year+"-"+subject.semester, subject.subjectName);
              }
              Console.WriteLine();
            }
        }

        // 사용자 성적 파일 READ
        public List<UserSubject> readUserSubject(string studentScoreFile)
        {
            List<UserSubject> temp = new List<UserSubject>();

            // 전체성적조회파일
            using (var gradeStream = System.IO.File.Open(studentScoreFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (var gradeReader = ExcelReaderFactory.CreateReader(gradeStream))
                {
                    gradeReader.Read();
                    string tempYear = "";
                    string tempSemester = "";
                    while (gradeReader.Read())
                    {
                        string[] valueArray = new string[20];
                        for (int i = 0; i < 20; i++)
                        {
                            if (gradeReader.GetValue(i) == null)
                                valueArray[i] = "";
                            else // 빈칸 제거
                                valueArray[i] = Regex.Replace(gradeReader.GetValue(i).ToString(), @"\s", "");
                        }
                        if (valueArray[1] != "")
                        {
                            tempYear = valueArray[1];
                        }
                        if (valueArray[2] != "")
                        {
                            tempSemester = valueArray[2];
                        }
                        temp.Add(new UserSubject
                        {
                            year = tempYear, // 연도
                            semester = tempSemester, // 학기
                            completionDiv = valueArray[3], // 이수구분 : 전공, 전필, 학기, 공교 등
                            completionDivField = valueArray[4], // 이수구분영역 : 기초, 전문, 자연과학 등
                            subjectCode = valueArray[5], // 학수번호
                            subjectName = valueArray[7], // 과목명
                            credit = Convert.ToInt32(Convert.ToDouble(valueArray[9])), // 학점
                            engineeringFactor = valueArray[14], // 공학요소 : 전공, MSC, 전문교양
                            engineeringFactorDetail = valueArray[15], // 공학세부요소 : 전공설계, 수학, 과학 등
                            english = valueArray[16], // 원어강의 종류
                            retake = valueArray[12] //재수강 여부
                        }); 
                    }
                }
            }
            return temp;
        }

        public void CheckRule()
        {
          /*
            // refactoring
            tempRule.checkRule();
            
          */
            //keywordSubjectPair, keywordCreditPair
            foreach (TempRule temprule in rule)
            {
                if (temprule.question_type == "단수")
                {
                    if (Convert.ToInt32(temprule.value) != keywordCreditPair[temprule.keyword]) // rule의 단수가 일치하지 않을 때
                    {
                        //keyword 의 학점을 만족하지 않는다 error message 출력
                        string errMessage = temprule.keyword + "가" + keywordCreditPair[temprule.keyword] + "로 기준인" + temprule.value +"를 만족하지 않습니다.";
                        errorMessageList[temprule.keyword] = errMessage;
                    }
                    else
                    {
                        temprule.check = true;
                    }
                }
                else if (temprule.question_type == "목록")
                {
                    //하나의 String 형태인 value를 list 형태로 pharsing 후 비교
                    //List<string> valueList = new List<string>();
                    string[] valueArray = temprule.value.Split('/');
                    List<string> valueList = new List<string>();
                    foreach (string value in valueArray)
                    {
                        valueList.Add(value);
                    }
                    //List간 비교
                    bool compare = false;
                    List<string> subjectCodeList = new List<string>();
                    foreach (UserSubject subject in keywordSubjectPair[temprule.keyword])
                    {
                        subjectCodeList.Add(subject.subjectCode);
                    }

                    var result = subjectCodeList.Where(x => valueList.Count(s => x.Contains(s)) == 0).ToList();

                    if (result.IsEmpty() == false) // 필수교과목이 없을 때.
                    {
                        //에러메세지 출력
                        temprule.check = false;
                    }
                    else
                    {
                        temprule.check = true;
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