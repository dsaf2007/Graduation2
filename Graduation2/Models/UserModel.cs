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
//using Rule.Models;

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
        public Dictionary<string, List<Subject>> subjectNameList = new Dictionary<Subject, List<string>>();
        public Dictionary<string, int> subjectCreditList = new Dictionary<string, int>();


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

            userSubjects = ReadUserSubject(filename_);



            foreach (List<UserSubject> userSubject in userSubjects)
            {
                int subjectCredit = Convert.ToInt32(userSubjects.credit);
                this.totalCredit += subjectCredit;

                if (userSubjects.engineeringFactorDetail == "기초교양(교필)")
                {
                    // this.publicLibCredit += subjectCredit;
                    // this.publicClasses.Add(userSubject);
                    this.subjectNameList["공통교양"].Add(new Subject
                    {
                        subjectCode = userSubject.subjectCode,
                        subjectName = userSubject.subjectName,
                        credit = userSubject.credit,
                        year = userSubject.year,
                        designCredit = 0
                    });
                    this.subjectCreditList["공통교양"] += subjectCredit;
                }
                if (userSubjects.engineeringFactorDetail == "기본소양")
                {
                    // this.basicLibCredit += subjectCredit;
                    // this.basicClasses.Add(userSubject);
                    this.subjectNameList["기본소양"].Add(new Subject
                    {
                        subjectCode = userSubject.subjectCode,
                        subjectName = userSubject.subjectName,
                        credit = userSubject.credit,
                        year = userSubject.year,
                        designCredit = 0
                    });
                    this.subjectCreditList["기본소양"] += subjectCredit;
                }
                if (userSubjects.engineeringFactor == "MSC/BSM")
                {
                    //this.mscCredit += subjectCredit;
                    this.subjectCreditList["MSC/BSM"] += subjectCredit;
                    switch (userSubjects.engineeringFactorDetail)
                    {
                        case "수학":
                            //this.mscMathCredit += subjectCredit;
                            this.subjectCreditList["수학"] += subjectCredit;
                            break;
                        case "기초과학":
                            if (userSubjects.className.Contains("실험"))
                                // this.mscScienceExperimentCredit += subjectCredit;
                                // this.mscScienceCredit += subjectCredit;
                                this.subjectCreditList["실험"] += subjectCredit;
                            this.subjectCreditList["기초과학"] += subjectCredit;
                            break;
                        case "전산학":
                            //this.mscComputerCredit += subjectCredit;
                            this.subjectCreditList["전산학"] += subjectCredit;
                            break;
                        default:
                            break;
                    }
                    //this.mscClasses.Add(userSubject);
                    this.subjectNameList["MSC/BSM"].Add(new Subject
                    {
                        subjectCode = userSubject.subjectCode,
                        subjectName = userSubject.subjectName,
                        credit = userSubject.credit,
                        year = userSubject.year,
                        designCredit = 0
                    });
                }
                if (userSubjects.engineeringFactor == "전공" || userSubjects.completionDiv == "전공")
                {
                    //this.majorCredit += subjectCredit;
                    this.subjectCreditList["전공"] += subjectCredit;
                    if (userSubjects.completionDiv == "전필")
                    {
                        // this.majorEssentialList.Add(userSubject);
                        // this.majorEssentialCredit += subjectCredit;
                        this.subjectNameList["전필"].Add(new Subject
                        {
                            subjectCode = userSubject.subjectCode,
                            subjectName = userSubject.subjectName,
                            credit = userSubject.credit,
                            year = userSubject.year,
                            designCredit = 0
                        });
                        this.subjectCreditList["전필"] += subjectCredit;
                    }
                    if (userSubjects.completionDivField == "전문")
                    {
                        //this.majorSpecialCredit += subjectCredit;
                        this.subjectCreditList["전문"] += subjectCredit;
                    }
                    if (userSubjects.engineeringFactorDetail == "전공설계")
                    {
                        // this.majorDesignCredit += subjectCredit;
                        // this.majorDesignList.Add(userSubject);
                        // this.majorEssentialList.Add(userSubject);
                        this.subjectNameList["전공설계"].Add(new Subject
                        {
                            subjectCode = userSubject.subjectCode,
                            subjectName = userSubject.subjectName,
                            credit = userSubject.credit,
                            year = userSubject.year,
                            designCredit = 0
                        });
                        this.subjectCreditList["전공설계"] += subjectCredit;
                        this.subjectNameList["전문"].Add(new Subject
                        {
                            subjectCode = userSubject.subjectCode,
                            subjectName = userSubject.subjectName,
                            credit = userSubject.credit,
                            year = userSubject.year,
                            designCredit = 0
                        });
                    }
                    if (userSubjects.english == "영어")
                    {
                        // this.englishMajorCredit += subjectCredit;
                        // this.englishMajorList.Add(userSubject);
                        this.subjectNameList["영어"].Add(new Subject
                        {
                            subjectCode = userSubject.subjectCode,
                            subjectName = userSubject.subjectName,
                            credit = userSubject.credit,
                            year = userSubject.year,
                            designCredit = 0
                        });
                        this.subjectCreditList["영어"] += subjectCredit;
                    }
                    //this.majorClasses.Add(userSubject);
                    this.subjectNameList["전공"].Add(new Subject
                    {
                        subjectCode = userSubject.subjectCode,
                        subjectName = userSubject.subjectName,
                        credit = userSubject.credit,
                        year = userSubject.year,
                        designCredit = 0
                    });
                }
                if (userSubjects.english == "영어") // 영어 전공과 교양 분류 기준 필요
                {
                    // this.englishCredit += subjectCredit;
                    // this.englishList.Add(userSubject);
                    this.subjectNameList["영어"].Add(new Subject
                    {
                        subjectCode = userSubject.subjectCode,
                        subjectName = userSubject.subjectName,
                        credit = userSubject.credit,
                        year = userSubject.year,
                        designCredit = 0
                    });
                    this.subjectCreditList["영어"] += subjectCredit;
                }
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